using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Users;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;

namespace PG
{
    /// <summary>
    /// For user multiplatform control. This way of implementing the input is chosen to be able to implement control of several players for one device.
    /// </summary>
    public class CarControllerInput :InitializePlayer, ICarControl
    {
        public float HorizontalChangeSpeed = 10;            //To simulate the use of a keyboard trigger.
        public bool RotateCameraWithMousePressed;
        public float Horizontal { get; private set; }
        public float Acceleration { get; private set; }
        public float BrakeReverse { get; private set; }
        public float Pitch { get; private set; }
        public bool HandBrake { get; private set; }
        public bool Boost { get; private set; }
        public Vector2 ViewDelta { get; private set; }
        public bool ViewDeltaFromGamepad { get; private set; }
        public InputDevice ActiveDevice { get; private set; }
        public bool ManualCameraRotation { get; private set; }

        public event System.Action OnChangeViewAction;

        float TargetHorizontal;

        CarLighting CarLighting;

        IInputActionCollection PlayerInputActions;
        int TouchCount;
        List<TouchControl> TouchesInProgress;
        TouchControl RotateTouch;
        event System.Action OnRemoveActions;

        InputUser User = default(InputUser);
        public bool IsFirstPlayer { get; private set; }

        public static int GamepadP1no;
        public static int GamepadP2no;

        private void Update ()
        {
            Horizontal = Mathf.MoveTowards (Horizontal, TargetHorizontal, Time.deltaTime * HorizontalChangeSpeed);

            var touchScreen = Touchscreen.current;

            if (Mouse.current != null)
            {
                ManualCameraRotation = RotateCameraWithMousePressed && !ViewDeltaFromGamepad ? Mouse.current.leftButton.isPressed : ViewDelta.sqrMagnitude > 0.05f;
            }
            else if (touchScreen != null)
            {
                var touchCount = touchScreen.touches.Count(t => t.isInProgress);
                if (touchCount != TouchCount)
                {
                    TouchesInProgress = touchScreen.touches.Where (t => t.isInProgress).ToList ();
                    if (touchCount > TouchCount && !ManualCameraRotation)
                    {
                        var lastTouch = TouchesInProgress.Last (t => t.press.wasPressedThisFrame);
                        if (!IsPointerOverUIObject (lastTouch))
                        {
                            RotateTouch = lastTouch;
                            ManualCameraRotation = true;
                        }

                    }
                    else if (touchCount < TouchCount && ManualCameraRotation)
                    {
                        ManualCameraRotation = false;
                        RotateTouch = null;
                        for (int i = 0; i < touchCount; i++)
                        {
                            if (!IsPointerOverUIObject (TouchesInProgress[i]))
                            {
                                RotateTouch = TouchesInProgress[i];
                                ManualCameraRotation = true;
                                break;
                            }
                        }
                    }

                    TouchCount = touchCount;
                }

                if (RotateTouch != null)
                {
                    ViewDelta = RotateTouch.delta.ReadValue ();
                }
            }

            if (IsFirstPlayer && Keyboard.current != null && GameController.Instance)
            {
                if (Keyboard.current.f3Key.wasPressedThisFrame)
                {
                    GameController.Instance.RestartScene ();
                }

                if (!GameController.SplitScreen && Keyboard.current.nKey.wasPressedThisFrame)
                {
                    GameController.Instance.SetNextCar ();
                }

                if (Keyboard.current.equalsKey.wasPressedThisFrame)
                {
                    GameController.Instance.ChangeTimeScale (0.1f);
                }

                if (Keyboard.current.minusKey.wasPressedThisFrame)
                {
                    GameController.Instance.ChangeTimeScale (-0.1f);
                }
            }
        }

        private void OnDestroy ()
        {
            if (PlayerInputActions != null)
            {
                PlayerInputActions.Disable ();
            }

            OnRemoveActions.SafeInvoke ();

            if (User != null && !GameSettings.IsMobilePlatform && User.id != InputUser.InvalidId)
            {
                User.UnpairDevicesAndRemoveUser ();
            }
        }

        void OnSetPause (bool value)
        {
            if (value)
            {
                PlayerInputActions.Disable ();
            }
            else
            {
                PlayerInputActions.Enable ();
            }
        }

        public void PairWithDevice (InputDevice device)
        {
            if (device != null)
            {
                User = InputUser.PerformPairingWithDevice (device, User);
            }
        }

        public override bool Initialize (VehicleController car)
        {
            base.Initialize (car);

            if (GameController.SplitScreen)
            {
                foreach (var device in InputSystem.devices)
                {
                    if (device is Keyboard)
                    {
                        PairWithDevice (device);
                    }
                }

                if (GameController.PlayerCar1 == car)
                {
                    IsFirstPlayer = true;
                    PlayerInputActions = new P1Input ();
                    PairWithDevice (Mouse.current);

                    int gamePadIndex = GamepadP1no - 1;

                    if (gamePadIndex >= 0 && gamePadIndex < Gamepad.all.Count)
                    {
                        PairWithDevice (Gamepad.all[gamePadIndex]);
                    }

                }
                else
                {
                    PlayerInputActions = new P2Input ();

                    int gamePadIndex = GamepadP2no - 1;

                    if (gamePadIndex >= 0 && gamePadIndex < Gamepad.all.Count)
                    {
                        PairWithDevice (Gamepad.all[gamePadIndex]);
                    }
                }

                User.AssociateActionsWithUser (PlayerInputActions);
            }
            else
            {
                PlayerInputActions = new P1Input ();
                IsFirstPlayer = true;
            }

            PlayerInputActions.Enable ();
            var actions = PlayerInputActions.ToList();

            InputAction action;

            action = actions.Find (a => a.name == "Acceleration");
            AddAction (action, OnAcceleration, InputActionPhase.Performed | InputActionPhase.Canceled);

            action = actions.Find (a => a.name == "BrakeReverse");
            AddAction (action, OnBrakeReverse, InputActionPhase.Performed | InputActionPhase.Canceled);

            action = actions.Find (a => a.name == "Steer");
            AddAction (action, OnSteer, InputActionPhase.Performed | InputActionPhase.Canceled);

            action = actions.Find (a => a.name == "Pitch");
            AddAction (action, OnPitch, InputActionPhase.Performed | InputActionPhase.Canceled);

            action = actions.Find (a => a.name == "NextGear");
            AddAction (action, OnNextGear, InputActionPhase.Started);

            action = actions.Find (a => a.name == "PrevGear");
            AddAction (action, OnPrevGear, InputActionPhase.Started);

            action = actions.Find (a => a.name == "Lights");
            AddAction (action, OnLights, InputActionPhase.Started);

            action = actions.Find (a => a.name == "LeftTurnSignal");
            AddAction (action, OnLeftTurnSignal, InputActionPhase.Started);

            action = actions.Find (a => a.name == "RightTurnSignal");
            AddAction (action, OnRightTurnSignal, InputActionPhase.Started);

            action = actions.Find (a => a.name == "Alarm");
            AddAction (action, OnAlarm, InputActionPhase.Started);

            action = actions.Find (a => a.name == "ResetCar");
            AddAction (action, OnResetCar, InputActionPhase.Started);

            action = actions.Find (a => a.name == "RestoreCar");
            AddAction (action, OnRestoreCar, InputActionPhase.Started);

            action = actions.Find (a => a.name == "ChangeView");
            AddAction (action, OnChangeView, InputActionPhase.Started);

            action = actions.Find (a => a.name == "ViewDelta");
            AddAction (action, OnViewDelta, InputActionPhase.Performed | InputActionPhase.Canceled);

            action = actions.Find (a => a.name == "HandBrake");
            AddAction (action, OnHandBrake, InputActionPhase.Started | InputActionPhase.Canceled);

            action = actions.Find (a => a.name == "Boost");
            AddAction (action, OnBoost, InputActionPhase.Started | InputActionPhase.Canceled);

            action = actions.Find (a => a.name == "ConnectTrailer");
            AddAction (action, OnConnectTrailer, InputActionPhase.Started);

            action = actions.Find (a => a.name == "EnterExit");
            AddAction (action, OnTryExitFromCar, InputActionPhase.Started);

            if (Car)
            {
                CarLighting = Car.GetComponent<CarLighting> ();
                var aiControl = Car.GetComponent<ICarControl>();
                if (aiControl == null || !(aiControl is PositioningAIControl))
                {
                    Car.CarControl = this;
                }
            }

            return IsInitialized;
        }

        public override void Uninitialize ()
        {
            if (Car != null && Car.CarControl == this as ICarControl)
            {
                Car.CarControl = null;
            }

            if (PlayerInputActions != null)
            {
                PlayerInputActions.Disable ();
            }

            OnRemoveActions.SafeInvoke ();

            if (User != null && !GameSettings.IsMobilePlatform && User.id != InputUser.InvalidId)
            {
                User.UnpairDevicesAndRemoveUser ();
            }

            CarLighting = null;
            base.Uninitialize ();
        }

        void AddAction (InputAction action, InputActionDelegate actionDelegate, InputActionPhase phases)
        {
            if (phases.HasFlag (InputActionPhase.Started))
            {
                action.started += actionDelegate.Invoke;

                OnRemoveActions += () =>
                {
                    action.started -= actionDelegate.Invoke;
                };
            }

            if (phases.HasFlag (InputActionPhase.Performed))
            {
                action.performed += actionDelegate.Invoke;

                OnRemoveActions += () =>
                {
                    action.performed -= actionDelegate.Invoke;
                };
            }

            if (phases.HasFlag (InputActionPhase.Canceled))
            {
                action.canceled += actionDelegate.Invoke;

                OnRemoveActions += () =>
                {
                    action.canceled -= actionDelegate.Invoke;
                };
            }
        }

        #region InputSystem actions

        void OnAcceleration (InputAction.CallbackContext context)
        {
            SetAcceleration (context.ReadValue<float> ());
        }

        void OnBrakeReverse (InputAction.CallbackContext context)
        {
            SetBrakeReverse (context.ReadValue<float> ());
        }

        void OnSteer (InputAction.CallbackContext context)
        {
            SetSteer (context.ReadValue<float> ());
        }

        void OnPitch (InputAction.CallbackContext context)
        {
            SetPitch (context.ReadValue<float> ());
        }

        void OnNextGear (InputAction.CallbackContext context)
        {
            NextGear ();
        }

        void OnPrevGear (InputAction.CallbackContext context)
        {
            PrevGear ();
        }

        void OnLights (InputAction.CallbackContext context)
        {
            SwitchLights ();
        }

        void OnLeftTurnSignal (InputAction.CallbackContext context)
        {
            SwitchLeftTurnSignal ();
        }

        void OnRightTurnSignal (InputAction.CallbackContext context)
        {
            SwitchRightTurnSignal ();
        }

        void OnAlarm (InputAction.CallbackContext context)
        {
            SwitchAlarm ();
        }

        void OnConnectTrailer (InputAction.CallbackContext context)
        {
            ConnectTrailer ();
        }

        void OnResetCar (InputAction.CallbackContext context)
        {
            ResetCar ();
        }

        void OnRestoreCar (InputAction.CallbackContext context)
        {
            RestoreCar ();
        }

        void OnChangeView (InputAction.CallbackContext context)
        {
            ChangeView ();
        }

        void OnTryExitFromCar (InputAction.CallbackContext context)
        {
            TryExitFromCar ();
        }

        void OnViewDelta (InputAction.CallbackContext context)
        {
            SetViewDelta (context.ReadValue<Vector2> ());
            ViewDeltaFromGamepad = context.control.device is Gamepad;
        }

        void OnHandBrake (InputAction.CallbackContext context)
        {
            HandBrake = context.phase == InputActionPhase.Started;
        }

        void OnBoost (InputAction.CallbackContext context)
        {
            Boost = context.phase == InputActionPhase.Started;
        }

        #endregion //InputSystem actions

        #region Set input

        public void SetAcceleration (float value)
        {
            Acceleration = value;
        }

        public void SetBrakeReverse (float value)
        {
            BrakeReverse = value;
        }

        public void SetSteer (float value)
        {
            TargetHorizontal = value;
        }

        public void SetPitch (float value)
        {
            Pitch = value;
        }

        public void NextGear ()
        {
            if (Car)
            {
                Car.NextGear ();
            }
        }

        public void PrevGear ()
        {
            if (Car)
            {
                Car.PrevGear ();
            }
        }

        public void SwitchLights ()
        {
            CarLighting.SwitchMainLights ();
        }

        public void SwitchLeftTurnSignal ()
        {
            CarLighting.TurnsEnable (TurnsStates.Left);
        }

        public void SwitchRightTurnSignal ()
        {
            CarLighting.TurnsEnable (TurnsStates.Right);
        }

        public void SwitchAlarm ()
        {
            CarLighting.TurnsEnable (TurnsStates.Alarm);
        }

        public void ConnectTrailer ()
        {
            if (Car)
            {
                Car.TryConnectDisconnectTrailer ();
            }
        }

        public void ResetCar ()
        {
            Vehicle.ResetVehicle ();
        }

        public void RestoreCar ()
        {
            Vehicle.RestoreVehicle ();
        }

        public void ChangeView ()
        {
            OnChangeViewAction.SafeInvoke ();
        }

        public void TryExitFromCar ()
        {
            var playerController = GetComponentInParent<PlayerController>();
            if (playerController != null)
            {
                playerController.ExitFromCar ();
            }
        }

        public void SetViewDelta (Vector2 value)
        {
            ViewDelta = value;
        }

        public void SetHandBrake (bool value)
        {
            HandBrake = value;
        }

        public void SetBoost (bool value)
        {
            Boost = value;
        }

        #endregion //Set input

        bool IsPointerOverUIObject (TouchControl touch)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = touch.position.ReadValue ();
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll (eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        delegate void InputActionDelegate (InputAction.CallbackContext context);
    }
}
