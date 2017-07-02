using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.JsonModels
{
    public class JsonOpposition
    {
        public List<JsonEnemy> Enemies { get; set; }
    }

    public class JsonEnemy
    {
        public string EnemyClass { get; set; }
        public string GetEnemyDirection { get; set; }
        public string GetBulletDirection { get; set; }
    }
}
