using System;
namespace BaseVerticalShooter
{
    interface IBaseVerticalShooterGame
    {
        Shooter.GameModel.BossMovement[] BossMovements { get; set; }
    }
}
