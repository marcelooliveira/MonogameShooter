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
    public class BumpAndTurnClockwise : BaseBumpAndTurnClockwise
    {
        public BumpAndTurnClockwise(double[] pauseMovePattern = null)
            : base(pauseMovePattern)
        {
        }

        public override Vector2 GetDirectionAfterCollision(BaseEnemy enemy)
        {
            Vector2 newDirection = enemy.Direction;
            if (enemy.Direction == Directions.Down)
                newDirection = Directions.Left;
            else if (enemy.Direction == Directions.Left)
                newDirection = Directions.Up;
            else if (enemy.Direction == Directions.Up)
                newDirection = Directions.Right;
            else if (enemy.Direction == Directions.Right)
                newDirection = Directions.Down;
            return newDirection;
        }
    }
}
