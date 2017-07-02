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
    public class BumpAndTurnCounterClockwise : BaseBumpAndTurnClockwise
    {
        public BumpAndTurnCounterClockwise(double[] pauseMovePattern = null)
            : base(pauseMovePattern)
        {
        }

        public override Vector2 GetDirectionAfterCollision(BaseEnemy enemy)
        {
            Vector2 newDirection = enemy.Direction;
            if (enemy.Direction == Directions.Down)
                newDirection = Directions.Right;
            else if (enemy.Direction == Directions.Right)
                newDirection = Directions.Up;
            else if (enemy.Direction == Directions.Up)
                newDirection = Directions.Left;
            else if (enemy.Direction == Directions.Left)
                newDirection = Directions.Down;
            return newDirection;
        }
    }
}
