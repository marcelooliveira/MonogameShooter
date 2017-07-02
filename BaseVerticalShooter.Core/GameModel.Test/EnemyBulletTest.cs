using BaseVerticalShooter.GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Moq;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel.Test
{
    [TestClass]
    public class EnemyBulletTest : BaseEnemyTest
    {
        [TestMethod]
        public void Update_Flying_Down_Colliding_Go_Dead()
        {
            var bullet = new EnemyBullet((centerCell + Directions.Down) * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            bullet.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            bullet.PixelsPerSec = ENEMY_PIXEL_SIZE;
            bullet.Direction = Directions.Down;
            Assert.AreEqual(CharacterState.Alive, bullet.State);
            bullet.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            bullet.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(CharacterState.Dead, bullet.State);
        }
    }
}
