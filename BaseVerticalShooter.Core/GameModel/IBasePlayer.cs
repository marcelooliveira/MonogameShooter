using BaseVerticalShooter.Core;
using BaseVerticalShooter.Input;
using Microsoft.Xna.Framework;
using Shooter.GameModel;
using System;
namespace BaseVerticalShooter.GameModel
{
    public interface IBasePlayer : IPhysicalObject
    {
        bool CanShoot { get; set; }
        Vector2 Direction { get; set; }
        Vector2 LastDirection { get; set; }
        Vector2 SpriteDirection { get; }
        SpriteLine GetSpriteLine();
        void Initialize();
        void LoadContent(IContentHelper contentHelper);
        void ProcessDeath(int tickCount, bool respawn = false);
        void ProcessGamePad(IScreenPad screenPad, Shooter.GameModel.PhysicalObject gameMap, float scrollRows);
        void Respawn();
        Microsoft.Xna.Framework.Vector2 SavedPosition { get; set; }
        void SavePosition();
        CharacterState State { get; set; }
        CollisionResult TestCollision(IPhysicalObject that, Microsoft.Xna.Framework.Vector2 thatNewPosition, float scrollRows);
        void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows);
        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows);
        float Lives { get; set; }
    }
}
