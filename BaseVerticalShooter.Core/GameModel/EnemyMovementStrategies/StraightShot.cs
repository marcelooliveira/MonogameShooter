using BaseVerticalShooter.GameModel;
using Microsoft.Xna.Framework;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel.EnemyMovementStrategies
{
    public class StraightShot : IEnemyMovement
    {
        RoutineType routineType = RoutineType.Pause;
        public RoutineType RoutineType
        {
            get { return routineType; }
            set { routineType = value; }
        }

        public Action<TimeSpan, BaseVerticalShooter.GameModel.IBasePlayer> GetTimedAction(BaseEnemy bullet, IBasePlayer player, IBaseMap gameMap)
        {
            var action = new Action<TimeSpan, IBasePlayer>((t, p) =>
            {
                if (bullet.State == CharacterState.Alive)
                {
                    var candidateWindowPosition =
                        bullet.OnWindowPosition
                        + bullet.Direction * (float)t.TotalSeconds * bullet.PixelsPerSec;
                    //var mapCollisionType = bullet.CheckMapCollision(gameMap, candidateWindowPosition);
                    //if (mapCollisionType == CollisionType.None)
                    //{
                        bullet.OnWindowPosition = candidateWindowPosition;
                    //}
                    //else
                    //{
                    //    bullet.State = CharacterState.Dead;
                    //}
                }
            });
            return action;
        }
    }
}
