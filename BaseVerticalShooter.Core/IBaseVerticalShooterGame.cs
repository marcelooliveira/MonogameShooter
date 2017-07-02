using BaseVerticalShooter.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace BaseVerticalShooter
{
    public interface IBaseVerticalShooterGame
    {
        BossMovement[] BossMovements { get; set; }
        GraphicsDevice GraphicsDevice { get; }
    }
}
