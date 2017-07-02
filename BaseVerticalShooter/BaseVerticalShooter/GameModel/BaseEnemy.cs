using BaseVerticalShooter.Core;
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
    public class BaseEnemy : PhysicalObject, IEnemy
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
        float scrollRows = 0;
        JsonOpposition Opposition;
        Player player;
        ILineProcessor lineProcessor;
        ScriptProcessor scriptProcessor;
        protected CollisionType lastMapCollisionType = CollisionType.None;

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
                    Position = (onWindowPosition.Value / scrollRowHeight) + new Vector2(0, this.scrollRows);
                }
            }
        }

        protected string code = "a";
        public string Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
            }
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

        public int DirectionIndex { get; set; }

        Vector2 direction = new Vector2(0, 1);
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        Vector2 lastDirection = new Vector2(0, 1);
        public Vector2 LastDirection
        {
            get { return lastDirection; }
            set { lastDirection = value; }
        }

        float speed = 1.5f;
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
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
                    direction = new Vector2(0, -1);
                    speed = 3;
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

            lineProcessor = BaseResolver.Instance.Resolve<ILineProcessor>();
            scriptProcessor = new ScriptProcessor(lineProcessor);
        }

        protected Texture2D enemySpriteSheet;
        public override void LoadContent(ContentManager content)
        {
            enemySpriteSheet = enemySpriteSheet ?? GetTexture(this.GetType().Name + "SpriteSheet");
            destructionSpriteSheet = destructionSpriteSheet ?? GetTexture("DestructionSpriteSheet");
            comboSpriteSheet = comboSpriteSheet ?? GetTexture("ComboSpriteSheet");
            fireSoundInstance = fireSoundInstance ?? ContentHelper.Instance.GetContent<SoundEffect>("Fire").CreateInstance();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows)
        {
        }

        public virtual void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, Map gameMap)
        {
            this.scrollRows = scrollRows;
            UpdateBlink(tickCount);

            if ((State == CharacterState.Alive || State == CharacterState.Combo) && onWindowPosition != null)
            {
                if (!reloaded && accumShotTime >= TimeSpan.FromMilliseconds(shotTimeOutMs))
                {
                    accumShotTime = TimeSpan.FromSeconds(0);
                    reloaded = true;
                }
                accumShotTime = accumShotTime.Add(gameTime.ElapsedGameTime);

                if (onWindowPosition.HasValue)
                {
                    if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
                    {
                        if (gameMap.State == MapState.Scrolling && !isAnchored)
                        {
                            accumElapsedGameTime = TimeSpan.FromSeconds(0);
                            onWindowTicks++;
                            Vector2? candidateWindowPosition;

                            var selectedSpeed = (OnWindowPosition.Value.Y < this.Size.Y * tileWidth * 2) ? 1 : speed;

                            if (State == CharacterState.Combo)
                                candidateWindowPosition = onWindowPosition + speed * new Vector2(0, -1);
                            else
                                candidateWindowPosition = onWindowPosition + speed * direction;

                            var mapCollisionType = CheckMapCollision(gameMap, candidateWindowPosition);

                            if (!(lastMapCollisionType == mapCollisionType
                                && mapCollisionType == CollisionType.Blocked
                                && lastDirection == direction))
                            {
                                if (!isBullet
                                    && state == CharacterState.Alive
                                    && candidateWindowPosition.Value.Y > 0
                                    && State != CharacterState.Combo)
                                {
                                    var collidedWithEnemy = CheckEnemyCollision(onScreenEnemies, candidateWindowPosition);

                                    HandleCollision(onScreenEnemies, gameMap, ref candidateWindowPosition, ref mapCollisionType, ref collidedWithEnemy);

                                    //if (collidedWithEnemy || mapCollisionType == CollisionType.Blocked)
                                    //{
                                    //    candidateWindowPosition = onWindowPosition;
                                    //    while (collidedWithEnemy || mapCollisionType == CollisionType.Blocked)
                                    //    {
                                    //        candidateWindowPosition = candidateWindowPosition - speed * direction;
                                    //        mapCollisionType = CheckMapCollision(gameMap, candidateWindowPosition);
                                    //        collidedWithEnemy = CheckEnemyCollision(onScreenEnemies, candidateWindowPosition);
                                    //    }
                                    //    ChangeDirection(gameMap);
                                    //}   
                                }

                                lastDirection = direction;
                            }

                            onWindowPosition = candidateWindowPosition;
                        }
                    }

                    accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);
                    if (!isAnchored)
                        Position = (onWindowPosition.Value / scrollRowHeight) + new Vector2(0, scrollRows);

                }
            }
        }

        private void HandleCollision(List<IEnemy> onScreenEnemies, Map gameMap, ref Vector2? candidateWindowPosition, ref CollisionType mapCollisionType, ref bool collidedWithEnemy)
        {
            if (collidedWithEnemy || mapCollisionType == CollisionType.Blocked)
            {
                candidateWindowPosition = onWindowPosition;
                while (collidedWithEnemy || mapCollisionType == CollisionType.Blocked)
                {
                    candidateWindowPosition = candidateWindowPosition - speed * direction;
                    mapCollisionType = CheckMapCollision(gameMap, candidateWindowPosition);
                    collidedWithEnemy = CheckEnemyCollision(onScreenEnemies, candidateWindowPosition);
                }
                lastMapCollisionType = mapCollisionType;
                ChangeDirection(gameMap);
            }
        }

        //public void ChangeDirection(Map gameMap)
        //{
        //    if (!this.IsBullet && !this.IsFlying && this.State == PlayerState.Alive)
        //    {
        //        var candidateDirection = direction;
        //        while (candidateDirection == direction)
        //        {
        //            //var directions = new Vector2[] { new Vector2(0, 1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1) };
        //            //candidateDirection = directions[directionIndex];
        //            candidateDirection = GetNewDirection();
        //        }
        //        direction = candidateDirection;
        //    }
        //}

        public void ChangeDirection(Map gameMap)
        {
            if (!this.IsBullet && !this.IsFlying && this.State == CharacterState.Alive)
            {
                //var candidateDirection = direction;
                //while (candidateDirection == direction)
                //{
                //    var directionIndex = (RandomProvider.GetThreadRandom().Next(0, 100)) % 4;
                //    var directions = new Vector2[] { new Vector2(0, 1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1) };
                //    candidateDirection = directions[directionIndex];
                //}
                //direction = candidateDirection;
                this.Direction = GetNewDirection();
                //this.direction = InvertDirection();
            }
        }

        public Vector2 GetNewDirection()
        {
            var script = @"return GetEnemyToPlayerDirection()";

            //var directions = new Vector2[] { new Vector2(0, 1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1) };
            //e.DirectionIndex++;
            //e.DirectionIndex = e.DirectionIndex % 4;
            //e.Direction = directions[DirectionIndex];

            this.direction = (Vector2)(scriptProcessor.ExecScript(this, script));
            return direction;
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
                        var thisRectangle = new Rectangle((int)candidateWindowPosition.Value.X, (int)candidateWindowPosition.Value.Y, (int)Size.X * tileWidth, (int)Size.Y * tileWidth);
                        var thatRectangle = new Rectangle((int)enemy.OnWindowPosition.Value.X, (int)enemy.OnWindowPosition.Value.Y, (int)enemy.Size.X * tileWidth, (int)enemy.Size.Y * tileWidth);
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

        private CollisionType CheckMapCollision(Map gameMap, Vector2? candidateWindowPosition)
        {
            CollisionResult mapCollisionResult = new CollisionResult { CollisionType = CollisionType.None };
            if (!this.isBullet)
            {
                var candidatePosition = (candidateWindowPosition.Value / scrollRowHeight);

                mapCollisionResult = gameMap.TestCollision(this, candidatePosition, gameMap.ScrollRows);
            }
            return mapCollisionResult.CollisionType;
        }

        public virtual void UpdateDirection(Player player, Map gameMap)
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
                onWindowPosition = Position * tileWidth
                - new Vector2(0, scrollRowHeight * scrollRows);
            }

            Rectangle enemyRectangle;
            switch (State)
            {
                case GameModel.CharacterState.Combo:
                    var comboStepCount = comboSpriteSheet.Width / (tileWidth * Size.Y);
                    enemyRectangle = new Rectangle((int)(((tickCount - blinkTickStart) % comboStepCount) * (int)(Size.X * tileWidth)), 0, (int)(Size.X * tileWidth), (int)(Size.Y * tileWidth));
                    spriteBatch.Draw(comboSpriteSheet
                        , DestinationRectangle()
                        , enemyRectangle
                        , Color.White);
                    break;
                case GameModel.CharacterState.Dead:
                    if (tickCount <= blinkTickStart + respawnTimeOut)
                    {
                        var destructionStepCount = destructionSpriteSheet.Width / (tileWidth * Size.Y);
                        enemyRectangle = new Rectangle((int)(((tickCount - blinkTickStart) % destructionStepCount) * (int)(Size.X * tileWidth)), 0, (int)(Size.X * tileWidth), (int)(Size.Y * tileWidth));
                        spriteBatch.Draw(destructionSpriteSheet
                            , DestinationRectangle()
                            , enemyRectangle
                            , Color.White);
                    }
                    break;
                default:
                    DrawAliveEnemy(spriteBatch, gameTime, tickCount, scrollRows);
                    break;
            }
        }

        protected virtual void DrawAliveEnemy(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            if (IsSpriteVisible())
            {
                var spriteCount = enemySpriteSheet.Width / enemySpriteSheet.Height;
                var bulletStep = tickCount % spriteCount;
                spriteBatch.Draw(enemySpriteSheet
                    , DestinationRectangle()
                    , new Rectangle((int)(bulletStep * Size.X * tileWidth), 0, (int)Size.X * tileWidth, (int)Size.Y * tileWidth)
                    , Color.White);
            }
        }

        public bool IsSpriteVisible()
        {
            if (onWindowPosition == null)
                return false;

            var windowRectangle = new Rectangle(
                (int)TopLeftCorner.X
                , (int)TopLeftCorner.Y
                , (int)GameSettings.Instance.WindowTilesSize.X * tileWidth
                , (int)GameSettings.Instance.WindowTilesSize.Y * tileWidth);

            return windowRectangle.Intersects(DestinationRectangle());
        }

        protected virtual Rectangle DestinationRectangle()
        {
            Rectangle destinationRectangle = new Rectangle(0, 0, 0, 0);
            if (isAnchored)
            {
                destinationRectangle = new Rectangle(
                      (int)(TopLeftCorner.X + Position.X * tileWidth)
                    , (int)(TopLeftCorner.Y + (Position.Y - scrollRows) * tileWidth)
                    , (int)(Size.X * tileWidth)
                    , (int)(Size.Y * tileWidth));
            }
            else
            {
                destinationRectangle = new Rectangle(
                        (int)(TopLeftCorner.X + onWindowPosition.Value.X)
                    , (int)(TopLeftCorner.Y + onWindowPosition.Value.Y)
                    , (int)(Size.X * tileWidth)
                    , (int)(Size.Y * tileWidth));
            }

            return destinationRectangle;
        }

        protected Rectangle ShadowDestinationRectangle()
        {
            var destinationRectangle = new Rectangle(
                      (int)(TopLeftCorner.X + onWindowPosition.Value.X)
                    , (int)(TopLeftCorner.Y + onWindowPosition.Value.Y + tileWidth)
                    , (int)(Size.X * tileWidth)
                    , (int)(Size.Y * tileWidth));
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
            direction = new Vector2(0, 1);
            blinkTickStart = 0;
        }

        public virtual EnemyBullet GetBullet(Player player, Map gameMap)
        {
            var bullet = new EnemyBullet(this.Position, 0);
            var script = "return GetCenterAxisDirection()";
            //var jsonEnemy = GameSettings.Instance.Opposition.Enemies.Where(e => e.EnemyClass == this.GetType().Name).SingleOrDefault();

            //if (jsonEnemy != null)
            //    if (!string.IsNullOrEmpty(jsonEnemy.GetBulletDirection))
            //        script = jsonEnemy.GetBulletDirection;

            //bullet.Direction = (Vector2)scriptProcessor.ExecScript(this, script);
            this.Reloaded = false;
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

        public Vector2 GetDownDirection() { return new Vector2(0, 1); }
        public Vector2 GetLeftDirection() { return new Vector2(-1, 0); }
        public Vector2 GetRightDirection() { return new Vector2(1, 0); }
        public Vector2 GetUpDirection() { return new Vector2(0, -1); }

        private Vector2 GetVectorDiff(Vector2 target)
        {
            var direction = target - (this.Position - new Vector2(0, scrollRows));
            direction.Normalize();
            return direction;
        }
    }

}
