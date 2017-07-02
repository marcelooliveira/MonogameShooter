using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.GameModel;
using BaseVerticalShooter.Core.GameModel.EnemyMovementStrategies;
using BaseVerticalShooter.Core.JsonModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseVerticalShooter.GameModel
{
    public abstract class BaseEnemy : PhysicalObject, IEnemy
    {
        static SoundEffectInstance fireSoundInstance;
        protected int tickInMS = 20;
        protected int changeDirectionInMS = 500;
        protected TimeSpan accumElapsedGameTime = TimeSpan.FromSeconds(0);
        protected int onWindowTicks = 0;
        Texture2D destructionSpriteSheet;
        Texture2D comboSpriteSheet;
        int respawnTimeOut = 10;
        protected int shotTimeOutMs = 5000;
        TimeSpan accumShotTime = TimeSpan.FromMilliseconds(0);
        int blinkTickStart = 0;
        IBasePlayer player;
        protected CollisionType lastMapCollisionType = CollisionType.None;
        protected static Vector2 DOWN_DIRECTION = new Vector2(0, 1);
        protected static Vector2 UP_DIRECTION = new Vector2(0, -1);
        protected static Vector2 RIGHT_DIRECTION = new Vector2(1, 0);
        protected static Vector2 LEFT_DIRECTION = new Vector2(-1, 0);

        protected EnemyBullet bullet = null;

        public EnemyBullet Bullet
        {
            get { return bullet; }
            set { bullet = value; }
        }

        float scrollRows = 0;
        public float ScrollRows
        {
            get { return scrollRows; }
            set { scrollRows = value; }
        }

        protected Vector2? onWindowPosition;
        public Vector2? OnWindowPosition
        {
            get
            {
                return onWindowPosition;
            }
            set
            {
                onWindowPosition = value;
                if (value != null)
                {
                    Position = ScrolledGridPosition();
                }
            }
        }

        private Vector2 ScrolledGridPosition()
        {
            return GridPosition() + new Vector2(0, this.scrollRows);
        }

        private Vector2 GridPosition()
        {
            return onWindowPosition.Value / scrollRowHeight;
        }

        private IEnemyMovement movementStrategy = null;
        protected IEnemyMovement MovementStrategy
        {
            get { return movementStrategy; }
            set { movementStrategy = value; }
        }

        protected string code = "a";
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        bool reloaded = true;
        public bool Reloaded
        {
            get { return reloaded; }
            set { reloaded = value; }
        }

        float lives = 1f;
        public float Lives
        {
            get { return lives; }
            set { lives = value; }
        }

        float rotation = 0f;
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public int DirectionIndex { get; set; }

        Vector2 direction = DOWN_DIRECTION;
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        Vector2 lastDirection = DOWN_DIRECTION;
        public Vector2 LastDirection
        {
            get { return lastDirection; }
            set { lastDirection = value; }
        }

        float pixelsPerSec = 64f;
        public float PixelsPerSec
        {
            get { return pixelsPerSec; }
            set { pixelsPerSec = value; }
        }

        CharacterState state = CharacterState.Alive;
        public CharacterState State
        {
            get { return state; }
            set
            {
                state = value;
                if (state == CharacterState.Combo)
                {
                    direction = UP_DIRECTION;
                    pixelsPerSec = 3;
                }
            }
        }

        int groupId = 0;
        public int GroupId
        {
            get { return groupId; }
            set { groupId = value; }
        }

        bool isBullet = false;
        public bool IsBullet
        {
            get { return isBullet; }
            set { isBullet = value; }
        }

        bool isPassingBy = true;
        public bool IsPassingBy
        {
            get { return isPassingBy; }
            set { isPassingBy = value; }
        }

        public BaseEnemy(Vector2 position, int groupId)
            : base()
        {
            Size = new Vector2(2, 2);
            StartPosition =
            Position = position;
            this.groupId = groupId;
        }

        protected Texture2D enemySpriteSheet;
        public override void LoadContent(IContentHelper contentHelper)
        {
            LoadEnemySpriteSheet(contentHelper);
            destructionSpriteSheet = destructionSpriteSheet ?? contentHelper.GetContent<Texture2D>("DestructionSpriteSheet");
            comboSpriteSheet = comboSpriteSheet ?? contentHelper.GetContent<Texture2D>("ComboSpriteSheet");
            var fireEffect = contentHelper.GetContent<SoundEffect>("Fire");
            if (fireEffect != null)
                fireSoundInstance = fireSoundInstance ?? fireEffect.CreateInstance();
        }

        protected virtual void LoadEnemySpriteSheet(IContentHelper contentHelper)
        {
            enemySpriteSheet = enemySpriteSheet ?? contentHelper.GetContent<Texture2D>(this.GetType().Name + "SpriteSheet");
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows)
        {
        }

        public virtual void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, IBaseMap gameMap)
        {
            CheckFirstAppearance(scrollRows);

            this.scrollRows = scrollRows;

            var action = movementStrategy.GetTimedAction(this, player, gameMap);

            ExecuteTimedAction(gameTime, player, action);

            Position = GetPositionFromWindowPosition(scrollRows);

            accumShotTime = accumShotTime.Add(gameTime.ElapsedGameTime);

            UpdateBlink(tickCount);

            //if ((State == CharacterState.Alive || State == CharacterState.Combo) && onWindowPosition != null)
            //{
            //    CheckReload();
            //    accumShotTime = accumShotTime.Add(gameTime.ElapsedGameTime);

            //    if (onWindowPosition.HasValue)
            //    {
            //        if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
            //        {
            //            if (gameMap.State == MapState.Scrolling && !isAnchored)
            //            {
            //                accumElapsedGameTime = TimeSpan.FromSeconds(0);
            //                onWindowTicks++;
            //                Vector2? candidateWindowPosition;

            //                var selectedSpeed = (OnWindowPosition.Value.Y < this.Size.Y * tileWidth * 2) ? 1 : speed;

            //                if (State == CharacterState.Combo)
            //                    candidateWindowPosition = onWindowPosition + speed * UP_DIRECTION;
            //                else
            //                    candidateWindowPosition = onWindowPosition + speed * direction;

            //                var mapCollisionType = CheckMapCollision(gameMap, candidateWindowPosition);

            //                if (!(lastMapCollisionType == mapCollisionType
            //                    && mapCollisionType == CollisionType.Blocked
            //                    && lastDirection == direction))
            //                {
            //                    if (!isBullet
            //                        && state == CharacterState.Alive
            //                        && candidateWindowPosition.Value.Y > 0
            //                        && State != CharacterState.Combo)
            //                    {
            //                        var collidedWithEnemy = CheckEnemyCollision(onScreenEnemies, candidateWindowPosition);

            //                        if (collidedWithEnemy || mapCollisionType == CollisionType.Blocked)
            //                        {
            //                            candidateWindowPosition = onWindowPosition;
            //                            while (collidedWithEnemy || mapCollisionType == CollisionType.Blocked)
            //                            {
            //                                candidateWindowPosition = candidateWindowPosition - speed * direction;
            //                                mapCollisionType = CheckMapCollision(gameMap, candidateWindowPosition);
            //                                collidedWithEnemy = CheckEnemyCollision(onScreenEnemies, candidateWindowPosition);
            //                            }
            //                            lastMapCollisionType = mapCollisionType;
            //                            if (!this.IsBullet && !this.IsFlying && this.State == CharacterState.Alive)
            //                            {
            //                                var candidateDirection = direction;
            //                                while (candidateDirection == direction)
            //                                {
            //                                    var directionIndex = (RandomProvider.GetThreadRandom().Next(0, 100)) % 4;
            //                                    var directions = new Vector2[] { DOWN_DIRECTION, LEFT_DIRECTION, RIGHT_DIRECTION, UP_DIRECTION };
            //                                    candidateDirection = directions[directionIndex];
            //                                }
            //                                direction = candidateDirection;
            //                                this.direction = InvertDirection();
            //                            }
            //                        }
            //                    }

            //                    lastDirection = direction;
            //                }

            //                onWindowPosition = candidateWindowPosition;
            //            }
            //        }

            //        accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);
            //        if (!isAnchored)
            //            Position = (onWindowPosition.Value / scrollRowHeight) + new Vector2(0, scrollRows);

            //    }
            //}
        }

        private void ExecuteTimedAction(Microsoft.Xna.Framework.GameTime gameTime, IBasePlayer player, Action<TimeSpan, IBasePlayer> action)
        {
            accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);
            if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
            {
                var time = accumElapsedGameTime;

                action(time, player);
                accumElapsedGameTime = TimeSpan.FromSeconds(0);
            }
        }

        private Vector2 GetPositionFromWindowPosition(float scrollRows)
        {
            var p = (onWindowPosition.Value / scrollRowHeight) + new Vector2(0, scrollRows);
            return p;
        }

        private void CheckFirstAppearance(float scrollRows)
        {
            var isFirstAppearance = onWindowPosition == null || isAnchored;
            if (isFirstAppearance)
            {
                onWindowPosition = Position * TileWidth
                - new Vector2(0, 1) * scrollRowHeight * scrollRows;
            }
        }

        public void CheckReload()
        {
            if (state == CharacterState.Alive && !reloaded && accumShotTime >= TimeSpan.FromMilliseconds(shotTimeOutMs))
            {
                accumShotTime = TimeSpan.FromSeconds(0);
                reloaded = true;
            }
        }

        public Vector2 InvertDirection()
        {
            return new Vector2(-1 * direction.X, -1 * direction.Y);
        }

        private bool CheckEnemyCollision(List<IEnemy> onScreenEnemies, Vector2? candidateWindowPosition)
        {
            var collided = false;
            if (!this.isBullet && this.State == CharacterState.Alive)
            {
                foreach (var enemy in onScreenEnemies)
                {
                    if (!enemy.IsBullet && !enemy.IsFlying && enemy != this && enemy.OnWindowPosition != null)
                    {
                        var thisRectangle = new Rectangle((int)candidateWindowPosition.Value.X, (int)candidateWindowPosition.Value.Y, (int)Size.X * TileWidth, (int)Size.Y * TileWidth);
                        var thatRectangle = new Rectangle((int)enemy.OnWindowPosition.Value.X, (int)enemy.OnWindowPosition.Value.Y, (int)enemy.Size.X * TileWidth, (int)enemy.Size.Y * TileWidth);
                        Rectangle intersectArea = Rectangle.Intersect(thisRectangle, thatRectangle);
                        collided = intersectArea.X * intersectArea.Y > 0;

                        if (collided)
                        {
                            break;
                        }
                    }
                }
            }
            return collided;
        }

        public CollisionType CheckMapCollision(IBaseMap gameMap, Vector2? candidateWindowPosition)
        {
            CollisionResult mapCollisionResult = new CollisionResult { CollisionType = CollisionType.None };
                var candidatePosition = (candidateWindowPosition.Value / scrollRowHeight);

                 mapCollisionResult = gameMap.TestCollision(this, candidatePosition, gameMap.ScrollRows);
            return mapCollisionResult.CollisionType;
        }

        public virtual void UpdateDirection(IBasePlayer player, IBaseMap gameMap)
        {
            this.player = player;
        }

        protected void UpdateBlink(int tickCount)
        {
            if (blinkTickStart == 0 && (state == CharacterState.Dead || state == CharacterState.Combo))
            {
                blinkTickStart = tickCount;
            }
            else if (state == CharacterState.Dead && tickCount > blinkTickStart + respawnTimeOut)
            {   
                NewMessenger.Default.Send(new EnemyDeathMessage { Enemy = this });
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            if (onWindowPosition == null || isAnchored)
            {
                onWindowPosition = Position * TileWidth
                - new Vector2(0, scrollRowHeight * scrollRows);
            }

            Rectangle enemyRectangle;
            switch (State)
            {
                case GameModel.CharacterState.Combo:
                    var comboStepCount = comboSpriteSheet.Width / (TileWidth * Size.Y);
                    enemyRectangle = new Rectangle((int)(((tickCount - blinkTickStart) % comboStepCount) * (int)(Size.X * TileWidth)), 0, (int)(Size.X * TileWidth), (int)(Size.Y * TileWidth));
                    spriteBatch.Draw(comboSpriteSheet
                        , DestinationRectangle()
                        , enemyRectangle
                        , Color.White);
                    break;
                case GameModel.CharacterState.Dead:
                    if (tickCount <= blinkTickStart + respawnTimeOut)
                    {
                        var destructionStepCount = destructionSpriteSheet.Width / (TileWidth * Size.Y);
                        enemyRectangle = new Rectangle((int)(((tickCount - blinkTickStart) % destructionStepCount) * (int)(Size.X * TileWidth)), 0, (int)(Size.X * TileWidth), (int)(Size.Y * TileWidth));
                        spriteBatch.Draw(destructionSpriteSheet
                            , DestinationRectangle()
                            , enemyRectangle
                            , Color.White);
                    }
                    break;
                default:
                    DrawShadow(spriteBatch, gameTime, tickCount, scrollRows);
                    DrawAliveEnemy(spriteBatch, gameTime, tickCount, scrollRows);
                    break;
            }
        }

        public virtual void DrawShadow(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
        }

        protected virtual void DrawAliveEnemy(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            if (IsSpriteVisible())
            {
                var spriteCount = enemySpriteSheet.Width / enemySpriteSheet.Height;
                var bulletStep = tickCount % spriteCount;
                spriteBatch.Draw(enemySpriteSheet
                    , DestinationRectangle()
                    , new Rectangle((int)(bulletStep * Size.X * TileWidth), 0, (int)Size.X * TileWidth, (int)Size.Y * TileWidth)
                    , Color.White
                    , Rotation
                    , (Size * TileWidth) / 2f
                    , SpriteEffects.None
                    , 0);
            }
        }

        public bool IsSpriteVisible()
        {
            if (onWindowPosition == null)
                return false;

            var windowRectangle = new Rectangle(
                (int)TopLeftCorner.X
                , (int)TopLeftCorner.Y
                , (int)GameSettings.Instance.WindowTilesSize.X * TileWidth
                , (int)GameSettings.Instance.WindowTilesSize.Y * TileWidth);

            return windowRectangle.Intersects(DestinationRectangle());
        }

        protected virtual Rectangle DestinationRectangle()
        {
            Rectangle destinationRectangle = new Rectangle(0, 0, 0, 0);
            if (isAnchored)
            {
                destinationRectangle = new Rectangle(
                      (int)(TopLeftCorner.X + Position.X * TileWidth)
                    , (int)(TopLeftCorner.Y + (Position.Y - scrollRows) * TileWidth)
                    , (int)(Size.X * TileWidth)
                    , (int)(Size.Y * TileWidth));
            }
            else
            {
                destinationRectangle = new Rectangle(
                        (int)(TopLeftCorner.X + onWindowPosition.Value.X)
                    , (int)(TopLeftCorner.Y + onWindowPosition.Value.Y)
                    , (int)(Size.X * TileWidth)
                    , (int)(Size.Y * TileWidth));
            }

            return destinationRectangle;
        }

        protected Rectangle ShadowDestinationRectangle()
        {
            var destinationRectangle = new Rectangle(
                      (int)(TopLeftCorner.X + onWindowPosition.Value.X)
                    , (int)(TopLeftCorner.Y + onWindowPosition.Value.Y + TileWidth)
                    , (int)(Size.X * TileWidth)
                    , (int)(Size.Y * TileWidth));
            return destinationRectangle;
        }

        public virtual void Hit(BaseBullet bullet, int tickCount)
        {
            if (State == CharacterState.Alive)
            {
                var temp = bullet.Damage;
                bullet.Damage -= this.Lives;
                this.lives -= temp;
                if (this.lives <= 0)
                {
                    ProcessDeath(tickCount);
                }
            }
        }

        public void ProcessDeath(int tickCount, bool respawn = false)
        {
            State = CharacterState.Dead;
            if (fireSoundInstance.State == SoundState.Stopped)
                fireSoundInstance.Play();
        }

        public virtual void RestorePosition()
        {
            Lives = 1;
            Position = StartPosition;
            onWindowPosition = null;
            direction = DOWN_DIRECTION;
            blinkTickStart = 0;
            bullet = null;
        }

        public virtual EnemyBullet GetBullet(IBasePlayer player, IBaseMap gameMap)
        {
            var bullet = new EnemyBullet(this.Position, 0);
            bullet.direction = this.Direction;
            bullet.Owner = this;
            //this.Reloaded = false;
            return bullet;
        }

        public Vector2 GetPlayerDirection()
        {
            return GetVectorDiff(player.Position);
        }

        public Vector2 GetEnemyToPlayerDirection()
        {
            if (lastDirection == direction && lastMapCollisionType == CollisionType.Blocked)
            {
                return -direction;
            }
            else
            {
                var diff = GetVectorDiff(player.Position);
                diff = new Vector2(
                    Math.Abs(diff.X) > Math.Abs(diff.Y) ? diff.X : 0,
                    Math.Abs(diff.X) > Math.Abs(diff.Y) ? 0 : diff.Y);
                return diff;
            }
        }

        public Vector2 GetCenterAxisDirection()
        {
            return GetVectorDiff(new Vector2(GameSettings.Instance.WindowTilesSize.X / 2, this.Position.Y - scrollRows));
        }

        public Vector2 GetDownDirection() { return DOWN_DIRECTION; }
        public Vector2 GetLeftDirection() { return LEFT_DIRECTION; }
        public Vector2 GetRightDirection() { return RIGHT_DIRECTION; }
        public Vector2 GetUpDirection() { return UP_DIRECTION; }

        private Vector2 GetVectorDiff(Vector2 target)
        {
            var direction = target - (this.Position - new Vector2(0, scrollRows));
            direction.Normalize();
            return direction;
        }


        public bool IsAlone
        {
            get
            {
                return groupId == 0;
            }
        }
    }

    public class Directions
    {
        public static Vector2 Up = new Vector2(0, -1);
        public static Vector2 Down = new Vector2(0, 1);
        public static Vector2 Left = new Vector2(-1, 0);
        public static Vector2 Right = new Vector2(1, 0);
    }
}
