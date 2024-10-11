using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PG
{
    /// <summary>
    /// For character input, UI input and device input are combined in this component.
    /// </summary>
    public class CharacterInput :MonoBehaviour
    {
        [Header ("UI input settings")]
        public GameObject PfrentForUI;              //Shown if mobile platform is selected.
        public MobileStickUI MoveStick;
        public MobileStickUI ViewStick;

        public ButtonCustom EntrerInCarBtn;

        public event System.Action OnEntrerInCar;

        CharacterInputActions InputActions;

        public Vector2 MoveInput { get; private set; }
        public Vector2 ViewInput { get; private set; }

        private void Awake ()
        {
            InputActions = new CharacterInputActions ();
        }

        private void Start ()
        {
            EntrerInCarBtn.onClick.AddListener (OnEntrerInCar.SafeInvoke);
            PfrentForUI.SetActive (GameSettings.IsMobilePlatform);

            InputActions.Input.EnterExit.started += (context) => OnEntrerInCar.SafeInvoke();
        }

        private void OnEnable ()
        {
            InputActions.Enable ();
        }

        private void OnDisable ()
        {
            InputActions.Disable ();
        }

        private void Update ()
        {
            if (MoveStick.IsPressed)
            {
                MoveInput = MoveStick.InputValue;
            }
            else
            {
                MoveInput = InputActions.Input.Move.ReadValue<Vector2> ();            
            }

            if (ViewStick.IsPressed)
            {
                ViewInput = ViewStick.InputValue;
            }
            else
            {
                ViewInput = InputActions.Input.View.ReadValue<Vector2> ();
            }
        }
    }
}
