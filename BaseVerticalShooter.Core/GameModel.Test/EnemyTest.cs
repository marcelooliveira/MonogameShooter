using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.GameModel;
using BaseVerticalShooter.Core.GameModel.Test;
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

namespace BaseVerticalShooter.Test.GameModel
{
    [TestClass]
    public class EnemyTest
    {
        public const float ENEMY_PIXEL_SIZE = 32f;
        public const float ENEMY_CELL_SIZE = 2f;

        Vector2 centerCell = new Vector2(2, 2);

        [TestMethod]
        public void Update()
        {
            var enemy1 = new Enemy1(centerCell, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);
            enemy1.Update(new GameTime(new TimeSpan(0), new TimeSpan(0)), 0, 0f);
            Assert.AreEqual(CharacterState.Alive, enemy1.State);
        }

        [TestMethod]
        public void Update_UpdateScrollRows()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            Assert.AreEqual(0, enemy1.ScrollRows);
            enemy1.Update(new GameTime(new TimeSpan(0), new TimeSpan(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(0, enemy1.ScrollRows);
            enemy1.Update(new GameTime(new TimeSpan(0), new TimeSpan(0)), 0, 1.7f, onScreenEnemies, gameMap);
            Assert.AreEqual(1.7f, enemy1.ScrollRows);
            enemy1.Update(new GameTime(new TimeSpan(0), new TimeSpan(0)), 0, 49.3f, onScreenEnemies, gameMap);
            Assert.AreEqual(49.3f, enemy1.ScrollRows);
        }

        [TestMethod]
        public void Update_Walking_Down_Position_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Down;
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Down) * enemy1.Size, enemy1.Position);
        }

        [TestMethod]
        public void Update_Walking_Up_Position_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Up;
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Up) * enemy1.Size, enemy1.Position);
        }

        [TestMethod]
        public void Update_Walking_Left_Position_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Left;
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Left) * enemy1.Size, enemy1.Position);
        }

        [TestMethod]
        public void Update_Walking_Right_Position_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Right;
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * enemy1.Size, enemy1.Position);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Right) * enemy1.Size, enemy1.Position);
        }

        [TestMethod]
        public void Update_Walking_Down_OnWindowPosition_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Down;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Down) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Down) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Up_OnWindowPosition_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Up;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Up) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Up) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Left_OnWindowPosition_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Left;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Left) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Left) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Right_OnWindowPosition_Changes()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();

            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Right;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Right) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Right) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Down_Collide_With_Map()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Down;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Down) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Down) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Up_Collide_With_Map()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Up;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Up) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Up) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Left_Collide_With_Map()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Left;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Left) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Left) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Right_Collide_With_Map()
        {
            var enemy1 = new Enemy1(centerCell * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Right;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(centerCell * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Right) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual((centerCell + Directions.Right) * ENEMY_PIXEL_SIZE, enemy1.OnWindowPosition);
        }

        [TestMethod]
        public void Update_Walking_Down_Colliding_Go_Left()
        {
            var enemy1 = new Enemy1((centerCell + Directions.Down) * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Down;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(Directions.Left, enemy1.Direction);
        }

        [TestMethod]
        public void Update_Walking_Left_Colliding_Go_Up()
        {
            var enemy1 = new Enemy1((centerCell + Directions.Left) * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Left;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(Directions.Up, enemy1.Direction);
        }

        [TestMethod]
        public void Update_Walking_Up_Colliding_Go_Right()
        {
            var enemy1 = new Enemy1((centerCell + Directions.Up) * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Up;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(Directions.Right, enemy1.Direction);
        }

        [TestMethod]
        public void Update_Walking_Right_Colliding_Go_Down()
        {
            var enemy1 = new Enemy1((centerCell + Directions.Right) * ENEMY_CELL_SIZE, 0);
            var contentHelperMock = new Mock<IContentHelper>();
            enemy1.LoadContent(contentHelperMock.Object);

            var onScreenEnemies = new List<IEnemy>();
            var gameMap = GetDummyMap();
            enemy1.PixelsPerSec = ENEMY_PIXEL_SIZE;
            enemy1.Direction = Directions.Right;
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0)), 0, 0f, onScreenEnemies, gameMap);
            enemy1.Update(new GameTime(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000)), 0, 0f, onScreenEnemies, gameMap);
            Assert.AreEqual(Directions.Down, enemy1.Direction);
        }

        private Map GetDummyMap()
        {
            var jsonManager = new DummyJsonMapManager();
            Map gameMap = new Map(0, jsonManager);
            gameMap.ScrollRows = 0;
            gameMap.LoadJsonMap();
            return gameMap;
        }
    }
}
