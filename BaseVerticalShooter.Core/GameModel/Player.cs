using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.Input;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ScreenControlsSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shooter.GameModel
{
    public class Player : BasePlayer
    {
        const int PLAYER_STEP_COUNT = 3;
        public Player()
            : base()
        {
            Size = new Vector2(2, 2);
            IsFlying = false;
        }

        Texture2D shadowSpriteSheet;
        public override void LoadContent(IContentHelper contentHelper)
        {
            base.LoadContent(contentHelper);
            shadowSpriteSheet = shadowSpriteSheet ?? contentHelper.GetContent<Texture2D>("ShadowSpriteSheet");
        }

        protected override Rectangle GetAlivePlayerRectangle(IScreenPad screenPad, int tickCount)
        {
            var frameIndex = 0;
            var frameIndexWhenIdle = 1;
            var frameIndexWhenWalking = (frameIndex
                    + (int)(Position.X)
                    + (int)(Position.Y)) % PLAYER_STEP_COUNT;

            if (InputManager.Instance.IsIdle(screenPad))
                frameIndex = frameIndexWhenIdle;
            else
                frameIndex = frameIndexWhenWalking;

            var spriteLine = SpriteLine.FaceUp;

            spriteLine = GetSpriteLine();

            var playerRectangle = new Rectangle(
                frameIndex * (int)(Size.X * TileWidth)
                , (int)spriteLine * (int)(Size.Y * TileWidth)
                , (int)(Size.X * TileWidth)
                , (int)(Size.Y * TileWidth));
            return playerRectangle;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            spriteBatch.Draw(shadowSpriteSheet, 
                TopLeftCorner 
                + this.Position * TileWidth 
                + new Vector2(0, this.Size.Y) * 1.5f, Color.White);
            base.Draw(spriteBatch, gameTime, tickCount, scrollRows);
        }

    }
}
