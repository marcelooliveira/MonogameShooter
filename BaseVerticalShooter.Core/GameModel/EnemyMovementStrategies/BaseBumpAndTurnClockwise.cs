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
    public abstract class BaseBumpAndTurnClockwise : IEnemyMovement
    {
        TimeSpan accumulatedTime = TimeSpan.FromSeconds(0);
        double[] pauseMovePattern = new double[] { 1, 1 };
        List<Milestone> Milestones = new List<Milestone>();
        
        RoutineType routineType = RoutineType.Pause;
        public RoutineType RoutineType
        {
            get { return routineType; }
            set { routineType = value; }
        }

        public BaseBumpAndTurnClockwise(double[] pauseMovePattern = null)
        {
            if (pauseMovePattern != null)
                this.pauseMovePattern = pauseMovePattern;

            SetMilestones(this.pauseMovePattern);
        }

        private void SetMilestones(double[] pauseMovePattern)
        {
            var accumTimeSpan = TimeSpan.FromSeconds(0);
            for (var i = 0; i < pauseMovePattern.Length; i++)
            {
                var timeSpan = TimeSpan.FromSeconds(pauseMovePattern[i]);
                var milestone = new Milestone
                {
                    Start = accumTimeSpan,
                    RoutineType = (i % 2 == 0)
                        ? RoutineType.Pause
                        : RoutineType.Walk
                };
                accumTimeSpan += timeSpan;
                milestone.End = accumTimeSpan;
                Milestones.Add(milestone);
            }
        }

        public Action<TimeSpan, IBasePlayer> GetTimedAction(BaseEnemy enemy, IBasePlayer player, BaseVerticalShooter.GameModel.IBaseMap gameMap)
        {
            var action = new Action<TimeSpan, IBasePlayer>((t, p) =>
            {
                if (enemy.State == CharacterState.Alive)
                {
                    if (accumulatedTime > Milestones.Last().End)
                        accumulatedTime = TimeSpan.FromSeconds(0);

                    var milestone = GetCurrentMilestone();
                    this.RoutineType = milestone.RoutineType;

                    var milestoneExists = (milestone.Start != milestone.End);
                    if (milestoneExists && milestone.RoutineType == RoutineType.Walk)
                        Walk(enemy, gameMap, t);

                    accumulatedTime = accumulatedTime.Add(t);
                    enemy.CheckReload();
                }
            });
            return action;
        }

        private Milestone GetCurrentMilestone()
        {
            var milestone = Milestones
                .Where(m => m.Start <= accumulatedTime
                            && m.End > accumulatedTime)
                .SingleOrDefault();
            return milestone;
        }

        private void Walk(BaseEnemy enemy, BaseVerticalShooter.GameModel.IBaseMap gameMap, TimeSpan t)
        {
            var candidateWindowPosition =
                enemy.OnWindowPosition
                + enemy.Direction * (float)t.TotalSeconds * enemy.PixelsPerSec;
            var mapCollisionType = enemy.CheckMapCollision(gameMap, candidateWindowPosition);
            if (mapCollisionType != CollisionType.Blocked)
            {
                enemy.OnWindowPosition = candidateWindowPosition;
            }
            else
            {
                Vector2 newDirection = enemy.Direction;
                newDirection = GetDirectionAfterCollision(enemy);
                enemy.Direction = newDirection;
            }
        }

        public abstract Vector2 GetDirectionAfterCollision(BaseEnemy enemy);
    }

    public enum RoutineType
    {
        Pause = 0,
        Walk = 1
    }

    public struct Milestone
    {
        public TimeSpan Start;
        public TimeSpan End;
        public RoutineType RoutineType;
    }
}
