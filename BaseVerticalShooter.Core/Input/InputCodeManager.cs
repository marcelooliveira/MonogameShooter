using BaseVerticalShooter.Core.GameModel;
using BaseVerticalShooter.Input;
using Microsoft.Xna.Framework;
using ScreenControlsSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xInput = Microsoft.Xna.Framework.Input;

namespace BaseVerticalShooter.Core.Input
{
    public class InputManager
    {
        bool buttonAPressed = false;
        bool buttonBPressed = false;
        bool buttonXPressed = false;
        bool buttonDownPressed = false;
        bool buttonUpPressed = false;
        
        static InputManager instance;
        private InputManager()
        {
        }

        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new InputManager();

                return instance;
            }
        }

        public char GetInputCode(IScreenPad screenPad, int blinkCount)
        {
            var inputCode = InputCodes.Empty;

            if (GameSettings.Instance.PlatformType == PlatformType.WindowsPhone)
            {
                if (screenPad.GetState().Buttons.B == ButtonState.Pressed)
                {
                    buttonBPressed = true;
                }
                else if (screenPad.GetState().Buttons.B == ButtonState.Released && buttonBPressed)
                {
                    buttonBPressed = false;
                    inputCode = InputCodes.B;
                }
                if (screenPad.GetState().Buttons.A == ButtonState.Pressed)
                {
                    buttonAPressed = true;
                }
                else if (screenPad.GetState().Buttons.A == ButtonState.Released && buttonAPressed)
                {
                    buttonAPressed = false;
                    inputCode = InputCodes.A;
                }
                if (screenPad.GetState().Buttons.X == ButtonState.Pressed)
                {
                    buttonXPressed = true;
                    inputCode = InputCodes.X;
                }
                else if (screenPad.GetState().Buttons.X == ButtonState.Released && buttonXPressed)
                {
                    buttonXPressed = false;
                }

                if (screenPad.LeftStick.Y > .75f && (inputCode != 'U' || blinkCount % 4 == 0))
                {
                    inputCode = InputCodes.U;
                }

                if (screenPad.LeftStick.Y < -.75f && (inputCode != 'D' || blinkCount % 4 == 0))
                {
                    inputCode = InputCodes.D;
                }
            }

            if (GameSettings.Instance.PlatformType == PlatformType.Windows
                || GameSettings.Instance.PlatformType == PlatformType.WindowsUniversal)
            {
                var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                if (keyboardState.IsKeyDown(xInput.Keys.A))
                {
                    buttonAPressed = true;
                }
                else if (keyboardState.IsKeyUp(xInput.Keys.A) && buttonAPressed)
                {
                    buttonAPressed = false;
                    inputCode = InputCodes.A;
                }

                if (keyboardState.IsKeyDown(xInput.Keys.B))
                {
                    buttonBPressed = true;
                }
                else if (keyboardState.IsKeyUp(xInput.Keys.B) && buttonBPressed)
                {
                    buttonBPressed = false;
                    inputCode = InputCodes.B;
                }

                if (keyboardState.IsKeyDown(xInput.Keys.Space))
                {
                    buttonXPressed = true;
                    inputCode = InputCodes.X;
                }

                if (keyboardState.IsKeyDown(xInput.Keys.Up))
                {
                    buttonUpPressed = true;
                }
                else if (keyboardState.IsKeyUp(xInput.Keys.Up) && buttonUpPressed)
                {
                    buttonUpPressed = false;
                    inputCode = InputCodes.U;
                }

                if (keyboardState.IsKeyDown(xInput.Keys.Down))
                {
                    buttonDownPressed = true;
                }
                else if (keyboardState.IsKeyUp(xInput.Keys.Down) && buttonDownPressed)
                {
                    buttonDownPressed = false;
                    inputCode = InputCodes.D;
                }
            }

            return inputCode;
        }

        public bool IsIdle(IScreenPad screenPad)
        {
            var keyboardIsIdle = KeyboardIsIdle();
            var screenPadIsIdle = ScreenPadIsIdle(screenPad);
            return keyboardIsIdle && screenPadIsIdle;
        }

        private bool KeyboardIsIdle()
        {
            var keyboardIsIdle = true;
            if (GameSettings.Instance.PlatformType == PlatformType.Windows
                || GameSettings.Instance.PlatformType == PlatformType.WindowsUniversal)
            {
                var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

                keyboardIsIdle =
                   !keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)
                && !keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right)
                && !keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up)
                && !keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down);
            }
            return keyboardIsIdle;
        }

        private bool ScreenPadIsIdle(IScreenPad screenPad)
        {
            return screenPad.LeftStick == Vector2.Zero;
        }
    }
}
