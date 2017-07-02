using BaseVerticalShooter;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Input;
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
    public class PowerUp : PhysicalObject
    {
        float scrollRows;
        protected int tickInMS = 20;
        protected TimeSpan accumElapsedGameTime = TimeSpan.FromSeconds(0);
        public event EventHandler OffScreen;
        protected Vector2? onWindowPosition;
        protected TimeSpan lastShotTime = TimeSpan.FromSeconds(0);
        protected TimeSpan accumShootingTime = TimeSpan.FromSeconds(0);
        ButtonState lastButtonState = ButtonState.Released;
        bool reloaded = true;
        IBasePlayer player;
        public Vector2 StartPosition;
        float acceleration = .1f;
        IScreenPad screenPad;

        Vector2 direction = new Vector2(0, 1);
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        protected float speed = .75f;
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        PowerUpState state = PowerUpState.Shield;
        public PowerUpState State
        {
            get { return state; }
            set { state = value; }
        }

        public PowerUp(Vector2 position, IBasePlayer player)
        : base()
        {
            Size = new Vector2(2, 2);
            StartPosition =
            Position = position;
            this.player = player;
            this.screenPad = BaseResolver.Instance.Resolve<IScreenPad>();
        }

        protected Texture2D powerUpSpriteSheet;
        public override void LoadContent(IContentHelper contentHelper)
        {
            powerUpSpriteSheet = powerUpSpriteSheet ?? contentHelper.GetContent<Texture2D>("PowerUpSpriteSheet");
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows)
        {
            this.scrollRows = scrollRows;

            if (screenPad.GetState().Buttons.X == ButtonState.Released && screenPad.GetState().Buttons.X != lastButtonState)
            {
                reloaded = true;
            }
            
            lastButtonState = screenPad.GetState().Buttons.X;

            accumShootingTime = accumShootingTime.Add(gameTime.ElapsedGameTime);

            if (onWindowPosition != null)
            {
                if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
                {
                    if (speed < 1f)
                        Speed += acceleration;

                    accumElapsedGameTime = TimeSpan.FromSeconds(0);
                    onWindowPosition = onWindowPosition + speed * new Vector2(0, 1) + direction;
                }
                accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);
                Position = (onWindowPosition.Value / scrollRowHeight) + new Vector2(0, scrollRows);
            }

            if (IsOffScreen())
            {
                OnOffScreen(new EventArgs());
            }
        }

        public bool IsOffScreen()
        {
            var thisRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            var windowRectangle = new Rectangle(0, 0, (int)GameSettings.Instance.WindowTilesSize.X, (int)GameSettings.Instance.WindowTilesSize.Y);
            Rectangle intersectArea = Rectangle.Intersect(thisRectangle, windowRectangle);
            var isOffScreen = intersectArea.X * intersectArea.Y == 0;
            return isOffScreen;
        }
        
        protected virtual void OnOffScreen(EventArgs e)
        {
            if (OffScreen != null)
                OffScreen(this, e);
        }

        protected virtual Rectangle DestinationRectangle()
        {
            var destinationRectangle = new Rectangle(
                      (int)(TopLeftCorner.X + Position.X * TileWidth)
                    , (int)(TopLeftCorner.Y + (Position.Y - scrollRows) * TileWidth)
                    , (int)(Size.X * TileWidth)
                    , (int)(Size.Y * TileWidth));
            return destinationRectangle;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            if (onWindowPosition == null)
            {
                onWindowPosition = Position * TileWidth
                - new Vector2(0, scrollRowHeight * scrollRows);
            }

            var powerUpRectangle = new Rectangle((int)state * powerUpSpriteSheet.Height, 0, (int)(Size.X * TileWidth), (int)(Size.Y * TileWidth));
            spriteBatch.Draw(powerUpSpriteSheet
                , DestinationRectangle()
                , powerUpRectangle
                , Color.White);
        }

        public void Hit(Vector2 bulletPosition)
        {
            var powerUpStateIndex = (int)State;
            powerUpStateIndex++;
            if (powerUpStateIndex > 2)
                powerUpStateIndex = 0;

            State = (PowerUpState)powerUpStateIndex;

            Speed = -3f;

            direction = new Vector2(this.Position.X - bulletPosition.X, 1);

            NewMessenger.Default.Send(new PowerUpStateChangedMessage { PowerUp = this });
        }

        public void RestorePosition()
        {
            Position = StartPosition;
        }
    }

    public enum PowerUpState
    {
        Shield = 0,
        Invulnerable = 1,
        Invisible = 2        
    }

    public class PowerUpStateChangedMessage { public PowerUp PowerUp { get; set; } }
}
