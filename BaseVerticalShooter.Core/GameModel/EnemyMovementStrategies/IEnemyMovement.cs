using BaseVerticalShooter.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel.EnemyMovementStrategies
{
    public interface IEnemyMovement
    {
        Action<TimeSpan, IBasePlayer> GetTimedAction(BaseEnemy enemy, IBasePlayer player, IBaseMap gameMap);
        RoutineType RoutineType { get; set; }
    }
}
