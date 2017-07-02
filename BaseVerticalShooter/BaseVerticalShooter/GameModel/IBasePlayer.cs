using System;
namespace BaseVerticalShooter.GameModel
{
    public interface IBasePlayer : IPhysicalObject
    {
        bool CanShoot { get; set; }
        Microsoft.Xna.Framework.Vector2 Direction { get; set; }
        Microsoft.Xna.Framework.Vector2 GetSpriteDirection();
        int GetSpriteLine();
        void Initialize();
        void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content);
        void ProcessDeath(int tickCount, bool respawn = false);
        void ProcessGamePad(BaseVerticalShooter.ScreenInput.IScreenPad screenPad, Shooter.GameModel.PhysicalObject gameMap, float scrollRows);
        void Respawn();
        Microsoft.Xna.Framework.Vector2 SavedPosition { get; set; }
        void SavePosition();
        CharacterState State { get; set; }
        Shooter.GameModel.CollisionResult TestCollision(IPhysicalObject that, Microsoft.Xna.Framework.Vector2 thatNewPosition, float scrollRows);
        void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows);
        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows);
        float Lives { get; set; }
    }
}
