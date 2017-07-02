using BaseVerticalShooter;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.JsonModels;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.ScreenInput;
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
using Windows.Storage;

namespace Shooter.GameModel
{
    public class EnemyBullet : Enemy
    {
        public EnemyBullet(Vector2 position, int groupId)
            : base(position, groupId)
        {
            Size = new Vector2(1, 1);
            Speed = 2f;
            Position = position;
            IsBullet = true;
        }

        public override void Hit(BaseBullet bullet, int tickCount)
        {
            //a bullet was hit? Nothing happens.
        }
    }

    public class EnemyBullet2 : EnemyBullet
    {
        public EnemyBullet2(Vector2 position, int groupId)
            : base(position, groupId)
        {
            Speed = 4f;
            Position = position;
            IsBullet = true;
        }
    }

    public class EnemyBullet3 : EnemyBullet
    {
        public EnemyBullet3(Vector2 position, int groupId)
            : base(position, groupId)
        {
            Speed = 5f;
            Position = position;
            IsBullet = true;
        }
    }
}
