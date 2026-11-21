using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonSurvivor.Core
{

    public static class InputHelper
    {
        public static Vector2 GetMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                float x = 0f;
                float y = 0f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                return new Vector2(x, y);
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                return pad.leftStick.ReadValue();
            }
            return Vector2.zero;
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        public static Vector2 GetLookInput()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                return mouse.delta.ReadValue() * 0.15f;
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                return pad.rightStick.ReadValue() * 2f;
            }
            return Vector2.zero;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }

        public static bool IsSprintHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                return kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                return pad.leftStickButton.isPressed;
            }
            return false;
#else
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
        }

        public static bool IsJumpPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                return kb.spaceKey.wasPressedThisFrame;
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                return pad.buttonSouth.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        public static bool IsDodgePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                return kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame || kb.leftAltKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame;
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                return pad.buttonEast.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.C);
#endif
        }

        public static bool IsAttackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                return true;
            }
            var pad = Gamepad.current;
            if (pad != null && pad.rightTrigger.wasPressedThisFrame)
            {
                return true;
            }
            return false;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static bool IsBlockHeld()
        {
            bool isHeld = false;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed) isHeld = true;
            var kb = Keyboard.current;
            if (kb != null && kb.fKey.isPressed) isHeld = true;
            var pad = Gamepad.current;
            if (pad != null && pad.leftTrigger.isPressed) isHeld = true;
#endif
            try
            {
                if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.F)) isHeld = true;
            }
            catch {}

            return isHeld;
        }

        public static bool IsInteractPressed()
        {
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) pressed = true;
            var pad = Gamepad.current;
            if (pad != null && pad.buttonSouth.wasPressedThisFrame) pressed = true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.E)) pressed = true;
            }
            catch {}

            return pressed;
        }

        public static bool IsPotionPressed()
        {
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && kb.qKey.wasPressedThisFrame) pressed = true;
            var pad = Gamepad.current;
            if (pad != null && pad.buttonNorth.wasPressedThisFrame) pressed = true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Q)) pressed = true;
            }
            catch {}

            return pressed;
        }

        public static bool IsPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                return kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame;
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                return pad.startButton.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
        }

        public static bool IsCheatPressed(int num)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                switch (num)
                {
                    case 1: return kb.f1Key.wasPressedThisFrame;
                    case 2: return kb.f2Key.wasPressedThisFrame;
                    case 3: return kb.f3Key.wasPressedThisFrame;
                    case 4: return kb.f4Key.wasPressedThisFrame;
                    case 5: return kb.f5Key.wasPressedThisFrame;
                }
            }
            return false;
#else
            switch (num)
            {
                case 1: return Input.GetKeyDown(KeyCode.F1);
                case 2: return Input.GetKeyDown(KeyCode.F2);
                case 3: return Input.GetKeyDown(KeyCode.F3);
                case 4: return Input.GetKeyDown(KeyCode.F4);
                case 5: return Input.GetKeyDown(KeyCode.F5);
                default: return false;
            }
#endif
        }
    }
}

