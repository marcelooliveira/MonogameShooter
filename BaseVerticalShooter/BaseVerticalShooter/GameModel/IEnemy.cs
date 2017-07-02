using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
namespace BaseVerticalShooter.GameModel
{
    public interface IEnemy : IPhysicalObject
    {
        string Code { get; set; }
        Vector2 Direction { get; set; }
        void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows);
        EnemyBullet GetBullet(IBasePlayer player, Map gameMap);
        int GroupId { get; set; }
        void Hit(BaseBullet bullet, int tickCount);
        bool IsBullet { get; set; }
        bool IsPassingBy { get; set; }
        bool IsSpriteVisible();
        float Lives { get; set; }
        void LoadContent(ContentManager content);
        Vector2? OnWindowPosition { get; set; }
        void ProcessDeath(int tickCount, bool respawn = false);
        bool Reloaded { get; set; }
        void RestorePosition();
        float Speed { get; set; }
        CharacterState State { get; set; }
        void Update(GameTime gameTime, int tickCount, float scrollRows);
        void Update(GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, Map gameMap);
        void UpdateDirection(IBasePlayer player, Map gameMap);
    }
}
