using Microsoft.Xna.Framework;
using ScreenControlsSample;
using System;
namespace BaseVerticalShooter.Input
{
    public interface IScreenPad
    {
        WeaponType CurrentWeapon { get; set; }
        void Draw(global::Microsoft.Xna.Framework.GameTime gameTime, global::Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);
        bool FireButtonPressed { get; set; }
        ScreenPadState GetState();
        Vector2 LeftStick { get; }
        Vector2 RightStick { get; }
        void Update();
        bool LeftStickIsIdle { get; }
    }
}
