using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.GameModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
namespace BaseVerticalShooter.GameModel
{
    public interface IEnemy : IPhysicalObject, IDirectionable
    {
        string Code { get; set; }
        Vector2 Direction { get; set; }
        void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows);
        void DrawShadow(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows);
        EnemyBullet GetBullet(IBasePlayer player, IBaseMap gameMap);
        int GroupId { get; set; }
        bool IsAlone { get; }
        void Hit(BaseBullet bullet, int tickCount);
        bool IsBullet { get; set; }
        bool IsPassingBy { get; set; }
        bool IsSpriteVisible();
        float Lives { get; set; }
        void LoadContent(IContentHelper contentHelper);
        Vector2? OnWindowPosition { get; set; }
        void ProcessDeath(int tickCount, bool respawn = false);
        bool Reloaded { get; set; }
        void RestorePosition();
        float PixelsPerSec { get; set; }
        CharacterState State { get; set; }
        EnemyBullet Bullet { get; set; }
        void Update(GameTime gameTime, int tickCount, float scrollRows);
        void Update(GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, IBaseMap gameMap);
        void UpdateDirection(IBasePlayer player, IBaseMap gameMap);
    }
}
