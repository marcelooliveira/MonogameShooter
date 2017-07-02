using BaseVerticalShooter;
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
    public class BaseBullet : PhysicalObject
    {
        protected Texture2D bulletSpriteSheet;
        protected SoundEffectInstance soundEffectInstance;
        protected bool shouldPlaySound = true;

        protected float damage = 1;
        public float Damage
        {
            get { return damage; }
            set { damage = value; }
        }

        protected Vector2 direction = new Vector2(0, 1);
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        protected float rotation = 0;
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        protected float speed = .15f;
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public event EventHandler OffScreen;

        public BaseBullet(float damage)
            : base()
        {
            Size = new Vector2(2, 2);
            this.damage = damage;
        }

        public override void LoadContent(ContentManager content)
        {
            bulletSpriteSheet = bulletSpriteSheet ?? ContentHelper.Instance.GetContent<Texture2D>(this.GetType().Name + "SpriteSheet");
            soundEffectInstance = soundEffectInstance ?? ContentHelper.Instance.GetSoundEffectInstance(this.GetType().Name + "Shooting");
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows)
        {
            Position = Position + speed * direction;
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

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            var spriteCount = bulletSpriteSheet.Width / bulletSpriteSheet.Height;
            var bulletStep = tickCount % spriteCount;
            var pos = TopLeftCorner + (Position) * tileWidth;
            var destinationRectangle = new Rectangle((int)pos.X + (int)(Size.X * tileWidth / 2f), (int)pos.Y, (int)Size.X * tileWidth, (int)Size.Y * tileWidth);

            spriteBatch.Draw(bulletSpriteSheet
                , destinationRectangle
                , new Rectangle((int)(bulletStep * Size.X * tileWidth), 0, (int)Size.X * tileWidth, (int)Size.Y * tileWidth)
                , Color.White
                , rotation
                , new Vector2(Size.X * tileWidth / 2f, Size.Y * tileWidth / 2f)
                , SpriteEffects.None
                , 0);
        }

        protected virtual void OnOffScreen(EventArgs e)
        {
            if (OffScreen != null)
                OffScreen(this, e);
        }

        public void Shoot()
        {
            if (shouldPlaySound)
            {
                soundEffectInstance.Stop();
                soundEffectInstance.Play();
            }
        }
    }

    public class BossBullet : BaseBullet
    {
        public BossBullet(float damage)
            : base(damage)
        {
            Size = new Vector2(1, 1);
        }
    }
}
