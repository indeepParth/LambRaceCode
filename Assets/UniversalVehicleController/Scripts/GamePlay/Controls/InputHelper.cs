using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine;

namespace PG
{
    public static class InputHelper
    {
        public static bool InputSupportSplitScreen => true;

        #region Keyboard

        public static bool EscapeWasPresed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        #endregion //Keyboard

        #region Gamepad

        public static int GetGamepadNames (out string[] names)
        {
            names = new string[Gamepad.all.Count];
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                names[i] = Gamepad.all[i].displayName;
            }

            return names.Length;
        }

        #endregion //Gamepad

        #region Accelerometer

        public static void EnableAccelerometer ()
        {
            if (Accelerometer.current != null)
            {
                InputSystem.EnableDevice (Accelerometer.current);
            }
        }

        public static void DisableAccelerometer ()
        {
            if (Accelerometer.current != null)
            {
                InputSystem.DisableDevice (Accelerometer.current);
            }
        }

        public static Vector3 GetAccelerometerData ()
        {
            if (Accelerometer.current != null)
            {
                return Accelerometer.current.acceleration.ReadValue ();
            }

            return Vector3.zero;
        }

        #endregion //Accelerometer

        #region Touches

        public static Vector2 GetNearestTouchPosition (Vector2 pos)
        {
            if (Touchscreen.current != null)
            {
                float minDist = float.MaxValue;
                TouchControl touchResult = null;

                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.isInProgress)
                    {
                        float dist = (pos - touch.position.ReadValue()).sqrMagnitude;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            touchResult = touch;
                        }
                    }
                }

                if (touchResult != null)
                {
                    return touchResult.position.ReadValue ();
                }
            }
            else if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue ();
            }

            return Vector2.zero;
        }

        #endregion //Touches
    }
}
