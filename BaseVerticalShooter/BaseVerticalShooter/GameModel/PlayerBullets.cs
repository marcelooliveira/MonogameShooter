using BaseVerticalShooter;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.ScreenInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ScreenControlsSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shooter.GameModel
{
    public class PlayerBullet1 : PlayerBullet
    {
        public PlayerBullet1(float damage, IBasePlayer player)
        : base(damage, player)
        {
            Size = new Vector2(1, 1);
        }
    }

    public class PlayerBullet2 : PlayerBullet
    {
        public PlayerBullet2(float damage, IBasePlayer player)
            : base(damage, player)
        {
        }
    }

    public class PlayerBullet3 : PlayerBullet
    {
        public PlayerBullet3(float damage, IBasePlayer player)
            : base(damage, player)
        {
        }
    }

    public class PlayerBullet4 : PlayerBullet
    {
        public PlayerBullet4(float damage, IBasePlayer player)
            : base(damage, player)
        {
        }
    }

    public class PlayerBullet5 : PlayerBullet
    {
        public PlayerBullet5(float damage, IBasePlayer player, Vector2 direction, float rotation, bool shouldPlaySound)
            : base(damage, player)
        {
            this.direction = direction;
            this.rotation = rotation;
            this.shouldPlaySound = shouldPlaySound;
        }
    }

    public class PlayerBullet6 : PlayerBullet
    {
        BoomerangDirection boomerangDirection = BoomerangDirection.Up;
        float acceleration = 0f;
        IScreenPad screenPad;

        public PlayerBullet6(float damage, IBasePlayer player)
            : base(damage, player)
        {
            speed = 20f;
            screenPad = Resolver.Instance.Resolve<IScreenPad>();
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows)
        {
            var t = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Speed += acceleration;

            Position = Position + (float)(Speed * t) * Direction;
            Position = new Vector2(player.Position.X + (player.Size.X - this.Size.X) / 2f, Position.Y);
            if (IsOffScreen() || Position.Y >= player.Position.Y)
            {
                OnOffScreen(new EventArgs());
            }

            if (boomerangDirection == BoomerangDirection.Up && screenPad.GetState().Buttons.X == ButtonState.Released)
            {
                acceleration = -2;
                boomerangDirection = BoomerangDirection.Down;
            }
        }
    }

    public enum BoomerangDirection
    {
        Up,
        Down
    }
}
