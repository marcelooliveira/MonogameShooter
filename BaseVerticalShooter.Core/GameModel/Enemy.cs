using BaseVerticalShooter;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.GameModel.EnemyMovementStrategies;
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shooter.GameModel
{
    public class Enemy : BaseEnemy
    {
        public Enemy(Vector2 position, int groupId)
            : base(position, groupId)
        {
            
        }
    }

    public class Enemy1 : BaseEnemy
    {
        protected int enemyNumber = 1;
        Texture2D enemyTexture = null;

        protected Texture2D shadowSpriteSheet;

        public int DirectionIndex { get; set; }

        public Enemy1(Vector2 position, int groupId) : base(position, groupId) 
        {
            this.MovementStrategy = new BumpAndTurnClockwise(new double[] { 1, .2, 2, .3, 1, .5 });
        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            base.LoadContent(contentHelper);
            shadowSpriteSheet = shadowSpriteSheet ?? contentHelper.GetContent<Texture2D>("ShadowSpriteSheet");
        }
        
        protected override void LoadEnemySpriteSheet(IContentHelper contentHelper)
        {
            enemyTexture = enemyTexture ?? contentHelper.GetContent<Texture2D>("EnemySpriteSheet");
        }

        public override EnemyBullet GetBullet(IBasePlayer player, IBaseMap gameMap)
        {
            bullet = GetBulletByEnemyNumber();
            bullet.Direction = this.Direction;
            bullet.Owner = this;
            this.Reloaded = true;
            return bullet;
        }

        private EnemyBullet GetBulletByEnemyNumber()
        {
            EnemyBullet newBullet = null;
            switch (enemyNumber % 3)
            {
                case 0:
                    newBullet = new EnemyBullet(this.Position, 0);
                    break;
                case 1:
                    newBullet = new EnemyBullet2(this.Position, 0);
                    break;
                case 2:
                    newBullet = new EnemyBullet3(this.Position, 0);
                    break;
            }
            return newBullet;
        }

        public override void DrawShadow(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            if (IsSpriteVisible())
            {
                var destinationRectangle = DestinationRectangle();
                DrawShadowSpriteSheet(spriteBatch, destinationRectangle);
            }
        }

        protected override void DrawAliveEnemy(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            if (IsSpriteVisible())
            {
                var spriteLine = GetSpriteLine();
                var spriteCount = 3;
                var isIdle = this.MovementStrategy.RoutineType == RoutineType.Pause;
                var step = isIdle ? 1 : tickCount % spriteCount;

                var destinationRectangle = DestinationRectangle();
                DrawEnemy(spriteBatch, spriteLine, step, destinationRectangle);
            }
        }

        private void DrawEnemy(SpriteBatch spriteBatch, int spriteLine, int step, Rectangle destinationRectangle)
        {
            spriteBatch.Draw(enemyTexture
                , destinationRectangle
                , new Rectangle(
                    (int)(step * Size.X * TileWidth) + (enemyNumber % 4) * 96,
                    0 + (int)(enemyNumber / 4) * 128
                    + spriteLine * 32,
                    (int)Size.X * TileWidth,
                    (int)Size.Y * TileWidth)
                , Color.White);
        }

        private void DrawShadowSpriteSheet(SpriteBatch spriteBatch, Rectangle destinationRectangle)
        {
            spriteBatch.Draw(shadowSpriteSheet,
                new Vector2(destinationRectangle.X, destinationRectangle.Y)
                + new Vector2(0, this.Size.Y) * 1.5f, Color.White);
        }

        private int GetSpriteLine()
        {
            var dir = new Vector2(
                Math.Abs(Direction.X) > Math.Abs(Direction.Y) ?
                    (Direction.X > 0 ? 1 : -1) : 0,
                Math.Abs(Direction.X) > Math.Abs(Direction.Y) ?
                    0 : (Direction.Y > 0 ? 1 : -1));

            var directionKey = string.Format("{0}{1}", dir.X, dir.Y);
            var directionKeys = new List<string> { "01", "-10", "10", "0-1" };
            var spriteLine = directionKeys.IndexOf(directionKey);
            return spriteLine;
        }
    }
    public class Enemy2 : Enemy1
    {
        public Enemy2(Vector2 position, int groupId) : base(position, groupId) { 
            enemyNumber = 2;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] {0.1, 1, .2, .5, 1, 2, .2});
        }
    }
    public class Enemy3 : Enemy1
    {
        public Enemy3(Vector2 position, int groupId) : base(position, groupId) { 
            enemyNumber = 3;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { 0.5, .5, 1, .1, .3, 2 });
        }
    }
    public class Enemy4 : Enemy1
    {
        public Enemy4(Vector2 position, int groupId) : base(position, groupId) {
            enemyNumber = 4;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { .5, 1, .1, .3, 2, .2 });
         }
    }
    public class Enemy5 : Enemy1
    {
        public Enemy5(Vector2 position, int groupId) : base(position, groupId) { 
            enemyNumber = 5;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { 1, .1, .3, 2, .2, .5 });
        }
    }
    public class Enemy6 : Enemy1
    {
        public Enemy6(Vector2 position, int groupId) : base(position, groupId) { 
            enemyNumber = 6;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { .1, .3, 2, .2, .5, 1.5 });
        }
    }
    public class Enemy7 : Enemy1
    {
        public Enemy7(Vector2 position, int groupId) : base(position, groupId) { 
            enemyNumber = 7;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { .3, 2, .2, .5, 1.5, .2 });
        }
    }
    public class Enemy8 : Enemy1
    {
        public Enemy8(Vector2 position, int groupId) : base(position, groupId) { 
            enemyNumber = 8;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { 2, .2, .5, 1.5, .2, .7 });
        }
    }
    public class Enemy9 : Enemy1
    {
        public Enemy9(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 9;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { .2, .5, 1.5, .2, .7, 1 });
        }
    }
    public class Enemy10 : Enemy1
    {
        public Enemy10(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 10;
            this.MovementStrategy = new BumpAndTurnCounterClockwise(new double[] { .5, 1.5, .2, .7, 1, .2 });
        }
    }
    public class Enemy11 : Enemy1
    {
        public Enemy11(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 11;
        }
    }
    public class Enemy12 : Enemy1
    {
        public Enemy12(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 12;
        }
    }
    public class Enemy13 : Enemy1
    {
        public Enemy13(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 13;
        }
    }
    public class Enemy14 : Enemy1
    {
        public Enemy14(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 14;
        }
    }
    public class Enemy15 : Enemy1
    {
        public Enemy15(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 15;
        }
    }
    public class Enemy16 : Enemy1
    {
        public Enemy16(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 16;
        }
    }
    public class Enemy17 : Enemy1
    {
        public Enemy17(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 17;
        }
    }
    public class Enemy18 : Enemy1
    {
        public Enemy18(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 18;
        }
    }
    public class Enemy19 : Enemy1
    {
        public Enemy19(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 19;
        }
    }
    public class Enemy20 : Enemy1
    {
        public Enemy20(Vector2 position, int groupId)
            : base(position, groupId)
        {
            enemyNumber = 20;
        }
    }
}
