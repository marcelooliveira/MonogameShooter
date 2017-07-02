using BaseVerticalShooter.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ScreenControlsSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shooter.GameModel
{
    public class Boss : BaseBoss
    {
        public Boss(Vector2 windowTilesSize, int levelNumber, BossMovement bossMovement) 
            : base(windowTilesSize, levelNumber, bossMovement)
            {
                position = new Vector2(16, 16);
            }
    }
}
