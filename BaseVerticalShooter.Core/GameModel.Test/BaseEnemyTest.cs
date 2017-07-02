using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel.Test
{
    public class BaseEnemyTest
    {
        protected const float ENEMY_PIXEL_SIZE = 32f;
        protected const float ENEMY_CELL_SIZE = 2f;

        protected Vector2 centerCell = new Vector2(2, 2);

        protected Map GetDummyMap()
        {
            var jsonManager = new DummyJsonMapManager();
            Map gameMap = new Map(0, jsonManager);
            gameMap.ScrollRows = 0;
            gameMap.LoadJsonMap();
            return gameMap;
        }
    }
}
