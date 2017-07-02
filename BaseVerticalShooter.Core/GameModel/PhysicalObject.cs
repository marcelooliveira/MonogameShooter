using BaseVerticalShooter;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.GameModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xInput = Microsoft.Xna.Framework.Input;

namespace Shooter.GameModel
{
    public abstract class PhysicalObject : IPhysicalObject
    {
        public Guid Guid = Guid.NewGuid();
        protected Vector2 stageSize = new Vector2(512, 3776);
        protected int scrollRowHeight = 16;
        protected ContentManager content;
        protected SpriteFont font;
        public string Key = Guid.NewGuid().ToString();
        public Vector2 TopLeftCorner;
        public Vector2 StartPosition;

        protected bool isAnchored = false;
        public bool IsAnchored
        {
            get { return isAnchored; }
            set { isAnchored = value; }
        }

        int tileWidth = 16;
        public int TileWidth
        {
            get { return tileWidth; }
            set { tileWidth = value; }
        }

        protected Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        protected Vector2 onWindowPosition;
        public virtual Vector2 OnWindowPosition
        {
            get { return onWindowPosition; }
            set { onWindowPosition = value; }
        }

        protected Vector2 size;
        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }
        protected bool isFlying = false;
        public bool IsFlying
        {
            get { return isFlying; }
            set { isFlying = value; }
        }
        static Dictionary<string, Texture2D> dicTexture = new Dictionary<string,Texture2D>();

        public PhysicalObject()
        {
            this.TopLeftCorner = new Vector2((GameSettings.Instance.DeviceScreenSize.X - GameSettings.Instance.GameScreenTilesSize.X * tileWidth) / 2, (GameSettings.Instance.DeviceScreenSize.Y - GameSettings.Instance.GameScreenTilesSize.Y * tileWidth) / 2);
        }

        protected virtual void SetNewContent(ContentManager content)
        {
            this.content = content;
        }

        public override bool Equals(object obj)
        {
            return ((PhysicalObject)this).Guid.Equals(((PhysicalObject)obj).Guid);
        }

        public virtual void LoadContent(IContentHelper contentHelper)
        {
            font = font ?? contentHelper.GetContent<SpriteFont>("Super-Contra-NES");
        }

        //protected Texture2D GetTexture(IContentHelper contentHelper, string name)
        //{
        //    if (dicTexture.ContainsKey(name))
        //        return dicTexture[name];
        //    else
        //    {
        //        var texture = contentHelper.GetContent<Texture2D>(name);
        //        dicTexture.Add(name, texture);
        //        return texture;
        //    }                
        //}

        public abstract void Update(GameTime gameTime, int tickCount, float scrollRows);

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows);

        public virtual CollisionResult TestCollision(IPhysicalObject that, Vector2 thatNewPosition, float scrollRows)
        {
            var collisionResult = new CollisionResult();

            var thisScreenRectangle =
                new Rectangle((int)Position.X
                    , (int)Position.Y
                    , (int)this.Size.X
                    , (int)this.Size.Y);

            var thatScreenRectangle =
                new Rectangle((int)thatNewPosition.X
                    , (int)(thatNewPosition.Y)
                    , (int)that.Size.X
                    , (int)that.Size.Y);

            Rectangle intersectArea = Rectangle.Intersect(thisScreenRectangle, thatScreenRectangle);
            if (intersectArea.X * intersectArea.Y != 0)
            {
                 collisionResult.CollisionType = CollisionType.Blocked;
            }

            return collisionResult;
        }
    }

    public interface IDamageable: IPhysicalObject
    {
        float Damage { get; set; }
    }
}
