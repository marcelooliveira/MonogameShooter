using BaseVerticalShooter;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.GameModel.EnemyMovementStrategies;
using BaseVerticalShooter.Core.JsonModels;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using Windows.Storage;

namespace Shooter.GameModel
{
    public class EnemyBullet : Enemy
    {
        PhysicalObject owner;
        private float radPosition = 0;

        public float RadPosition
        {
            get { return radPosition; }
            set { radPosition = value; }
        }

        public PhysicalObject Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        float revsPerSec = 1f;
        public float RevsPerSec
        {
            get { return revsPerSec; }
            set { revsPerSec = value; }
        }

        public EnemyBullet(Vector2 position, int groupId)
            : base(position, groupId)
        {
            Size = new Vector2(1, 1);
            PixelsPerSec = 128f;
            Position = position;
            IsBullet = true;
            this.MovementStrategy = new SattelliteBullet();
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, IBaseMap gameMap)
        {
            base.Update(gameTime, tickCount, scrollRows, onScreenEnemies, gameMap);
            if (this.State == CharacterState.Dead)
                this.Owner = null;
        }

        public override void Hit(BaseBullet bullet, int tickCount)
        {
            //a bullet was hit? Nothing happens.
        }

        //public override Action<TimeSpan, IBasePlayer> GetTimedAction(BaseEnemy enemy, IBasePlayer player, IBaseMap gameMap)
        //{
        //    var action = new Action<TimeSpan, IBasePlayer>((t, p) =>
        //    {
        //        if (State == CharacterState.Alive)
        //        {
        //            var candidateWindowPosition =
        //                this.OnWindowPosition
        //                + this.Direction * (float)t.TotalSeconds * this.PixelsPerSec;
        //            var mapCollisionType = CheckMapCollision(gameMap, candidateWindowPosition);
        //            if (mapCollisionType == CollisionType.None)
        //            {
        //                onWindowPosition = candidateWindowPosition;
        //            }
        //            else
        //            {
        //                this.State = CharacterState.Dead;
        //            }
        //        }
        //    });
        //    return action;
        //}
    }

    public class EnemyBullet2 : EnemyBullet
    {
        public EnemyBullet2(Vector2 position, int groupId)
            : base(position, groupId)
        {
            PixelsPerSec = 128f;
            Position = position;
            IsBullet = true;
            this.MovementStrategy = new StraightShot();
        }
    }

    public class EnemyBullet3 : EnemyBullet
    {
        public EnemyBullet3(Vector2 position, int groupId)
            : base(position, groupId)
        {
            PixelsPerSec = 128f;
            Position = position;
            IsBullet = true;
            this.MovementStrategy = new StraightShot();
        }
    }
}
