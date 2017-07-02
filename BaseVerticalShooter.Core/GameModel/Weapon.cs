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
    public class Weapon : PhysicalObject
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
        IContentHelper contentHelper;

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

        WeaponState state = WeaponState.Arrow;
        public WeaponState State
        {
            get { return state; }
            set { state = value; }
        }

        public Weapon(Vector2 position, IBasePlayer player)
            : base()
        {
            Size = new Vector2(2, 2);
            StartPosition =
            Position = position;
            this.player = player;
            this.screenPad = BaseResolver.Instance.Resolve<IScreenPad>();
        }

        protected Texture2D weaponSpriteSheet;
        public override void LoadContent(IContentHelper contentHelper)
        {
            this.contentHelper = contentHelper;
            weaponSpriteSheet = weaponSpriteSheet ?? contentHelper.GetContent<Texture2D>("WeaponSpriteSheet");
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows)
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

            var weaponRectangle = new Rectangle((int)state * weaponSpriteSheet.Height, 0, (int)(Size.X * TileWidth), (int)(Size.Y * TileWidth));
            spriteBatch.Draw(weaponSpriteSheet
                , DestinationRectangle()
                , weaponRectangle
                , Color.White);
        }

        public void Hit(Vector2 bulletPosition)
        {
            var weaponIndex = (int)State;
            weaponIndex++;
            if (weaponIndex > 6)
                weaponIndex = 0;

            State = (WeaponState)weaponIndex;

            Speed = -3f;

            direction = new Vector2(this.Position.X - bulletPosition.X, 1);

            NewMessenger.Default.Send(new WeaponStateChangedMessage { Weapon = this });
        }

        public List<PlayerBullet> Shoot()
        {
            List<PlayerBullet> weaponBullets = new List<PlayerBullet>();
            if (reloaded)
            {
                if (accumShootingTime.TotalMilliseconds >= MinShootingIntervalMS())
                {
                    reloaded = false;
                    accumShootingTime = TimeSpan.FromSeconds(0);
                    PlayerBullet playerBullet = null;
                    switch (State)
                    {
                        case WeaponState.Arrow:
                            playerBullet = new PlayerBullet1(WeaponDamage(), player);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            break;
                        case WeaponState.DoubleArrow:
                            playerBullet = new PlayerBullet2(WeaponDamage(), player);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            break;
                        case WeaponState.Knife:
                            playerBullet = new PlayerBullet3(WeaponDamage(), player);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            break;
                        case WeaponState.FireArrow:
                            playerBullet = new PlayerBullet4(WeaponDamage(), player);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            break;
                        case WeaponState.FireBall:
                            playerBullet = new PlayerBullet5(WeaponDamage(), player, new Vector2(0, -1), 0, true);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            playerBullet = new PlayerBullet5(WeaponDamage(), player, new Vector2(-.5f, -1), (float)-Math.PI / 6, false);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            playerBullet = new PlayerBullet5(WeaponDamage(), player, new Vector2(.5f, -1), (float)Math.PI / 6, false);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            break;
                        case WeaponState.Boomerang:
                            playerBullet = new PlayerBullet6(WeaponDamage(), player);
                            playerBullet.LoadContent(contentHelper);
                            weaponBullets.Add(playerBullet);
                            break;
                    }

                    if (float.IsNaN(player.Direction.X) ||
                        float.IsNaN(player.Direction.Y))
                        playerBullet.Direction = player.SpriteDirection;
                    else
                        playerBullet.Direction = player.Direction;

                    reloaded = IsAutoShooting();
                }
            }
            return weaponBullets;
        }

        int MinShootingIntervalMS()
        {
            var interval = 0;
            switch (State)
            {
                case WeaponState.Arrow:
                    interval = 300;
                    break;
                case WeaponState.DoubleArrow:
                    interval = 300;
                    break;
                case WeaponState.Knife:
                    interval = 500;
                    break;
                case WeaponState.FireArrow:
                    interval = 300;
                    break;
                case WeaponState.FireBall:
                    interval = 500;
                    break;
                case WeaponState.Boomerang:
                    interval = 300;
                    break;
            }
            return interval;
        }

        float WeaponDamage()
        {
            var interval = 0;
            switch (State)
            {
                case WeaponState.Arrow:
                    interval = 1;
                    break;
                case WeaponState.DoubleArrow:
                    interval = 2;
                    break;
                case WeaponState.Knife:
                    interval = 2;
                    break;
                case WeaponState.FireArrow:
                    interval = 2;
                    break;
                case WeaponState.FireBall:
                    interval = 1;
                    break;
                case WeaponState.Boomerang:
                    interval = 1;
                    break;
            }
            return interval;
        }

        bool IsAutoShooting()
        {
            var isAutoShooting = false;
            switch (State)
            {
                case WeaponState.Arrow:
                    isAutoShooting = true;
                    break;
                case WeaponState.DoubleArrow:
                    isAutoShooting = true;
                    break;
                case WeaponState.Knife:
                    isAutoShooting = true;
                    break;
                case WeaponState.FireArrow:
                    isAutoShooting = true;
                    break;
                case WeaponState.FireBall:
                    isAutoShooting = false;
                    break;
                case WeaponState.Boomerang:
                    isAutoShooting = false;
                    break;
            }
            return isAutoShooting;
        }

        public void RestorePosition()
        {
            Position = StartPosition;
        }
    }

    public enum WeaponState
    {
        OneThousandPoints = 0,
        Arrow = 1,
        DoubleArrow = 2,
        Knife = 3,
        FireArrow = 4,
        FireBall = 5,
        Boomerang = 6
    }

    public class WeaponStateChangedMessage { public Weapon Weapon { get; set; } }
}
