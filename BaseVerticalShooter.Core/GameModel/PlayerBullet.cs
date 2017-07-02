using BaseVerticalShooter.GameModel;
using Microsoft.Xna.Framework;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shooter.GameModel
{
    public class PlayerBullet : BaseBullet
    {
        protected IBasePlayer player;
        public PlayerBullet(float damage, IBasePlayer player)
            : base(damage)
        {
            Size = new Vector2(1, 1);
            speed = .4f;
            this.damage = damage;
            this.player = player;
            direction = new Vector2(0, -1);
        }
    }
}
