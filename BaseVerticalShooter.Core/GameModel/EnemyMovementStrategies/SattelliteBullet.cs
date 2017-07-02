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
    public class SattelliteBullet : IEnemyMovement
    {
        RoutineType routineType = RoutineType.Pause;
        public RoutineType RoutineType
        {
            get { return routineType; }
            set { routineType = value; }
        }

        public Action<TimeSpan, BaseVerticalShooter.GameModel.IBasePlayer> GetTimedAction(BaseEnemy enemy, IBasePlayer player, IBaseMap gameMap)
        {
            var thisBullet = enemy as EnemyBullet;
            var action = new Action<TimeSpan, IBasePlayer>((t, p) =>
            {
                if (thisBullet.Owner != null
                    && ((BaseEnemy)thisBullet.Owner).State == CharacterState.Alive)
                {
                    if (thisBullet.State == CharacterState.Alive)
                    {
                        var range = (thisBullet.Owner.Size * GameSettings.Instance.MapTileWidth) / 2f;
                        thisBullet.RadPosition += thisBullet.RevsPerSec * ((float)t.TotalMilliseconds / 1000f) * (2f * (float)Math.PI);
                        var candidateWindowPosition =
                            ((thisBullet.Owner.Position + thisBullet.Size - new Vector2(0, gameMap.ScrollRows))
                                * thisBullet.TileWidth)
                            + range * new Vector2((float)Math.Cos(thisBullet.RadPosition), -(float)Math.Sin(thisBullet.RadPosition));

                        thisBullet.Rotation = -thisBullet.RadPosition;

                        //var mapCollisionType = CheckMapCollision(gameMap, candidateWindowPosition);
                        //if (mapCollisionType == CollisionType.None)
                        //{
                        thisBullet.OnWindowPosition = candidateWindowPosition;
                        //}
                        //else
                        //{
                        //    this.State = CharacterState.Dead;
                        //}
                        ((BaseEnemy)thisBullet.Owner).Reloaded = false;
                    }
                }

                if (thisBullet.Owner != null
                    && ((BaseEnemy)thisBullet.Owner).State
                    == CharacterState.Dead)
                    thisBullet.State = CharacterState.Dead;

            });
            return action;   
        }
    }
}
