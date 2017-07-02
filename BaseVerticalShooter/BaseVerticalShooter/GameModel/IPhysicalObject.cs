using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.GameModel;
using System;
namespace BaseVerticalShooter.GameModel
{
    public interface IPhysicalObject
    {
        void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows);
        bool Equals(object obj);
        bool IsAnchored { get; set; }
        Vector2 Size { get; set; }
        Vector2 Position { get; set; }
        bool IsFlying { get; set; }
        void LoadContent(ContentManager content);
        CollisionResult TestCollision(IPhysicalObject that, Vector2 thatNewPosition, float scrollRows);
        void Update(GameTime gameTime, int tickCount, float scrollRows);
    }
}
