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
    public class BaseEnemy2 : Enemy
    {
        Vector2? onWindowStartPosition;
        float angle = 0f;
        protected float rangeWidth = .25f;
        protected float ticksToYRate = 20;

        public BaseEnemy2(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "b";
            Speed = 2f;
            Position = position;
            IsFlying = true;
            IsPassingBy = true;
        }

        Texture2D shadowSpriteSheet;
        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
            shadowSpriteSheet = GetTexture("ShadowSpriteSheet");
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, Map gameMap)
        {
            UpdateBlink(tickCount);

            if ((State == CharacterState.Alive || State == CharacterState.Combo) && onWindowPosition != null)
            {
                if (onWindowStartPosition == null)
                    onWindowStartPosition = onWindowPosition;

                if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS / 20f))
                {
                    if (gameMap.State == MapState.Scrolling && !isAnchored)
                    {
                        if (State == CharacterState.Combo)
                        {
                            onWindowPosition = onWindowPosition + Speed * new Vector2(0, -1);
                        }
                        else
                        {
                            var rad = ((onWindowTicks % 360) / 360f) * 2 * Math.PI;

                            onWindowPosition +=
                                new Vector2((float)Math.Cos(rad), .5f);

                        }
                        accumElapsedGameTime = TimeSpan.FromSeconds(0);
                        onWindowTicks++;
                    }
                }
                accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);

                Position = (onWindowPosition.Value / scrollRowHeight) + new Vector2(0, (int)(scrollRows));
            }
        }

        protected override void DrawAliveEnemy(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            base.DrawAliveEnemy(spriteBatch, gameTime, tickCount, scrollRows);
            if (IsSpriteVisible())
            {
                spriteBatch.Draw(shadowSpriteSheet
                    , ShadowDestinationRectangle()
                    , new Rectangle(0, 0, (int)Size.X * tileWidth, (int)Size.Y * tileWidth)
                    , Color.White);
            }
        }

        public override EnemyBullet GetBullet(Player player, Map gameMap)
        {
            var bullet = new EnemyBullet2(this.Position, 0);
            var newDirection = new Vector2(player.Position.X - this.Position.X, (player.Position.Y + gameMap.ScrollRows) - this.Position.Y) + player.Size / 2;
            newDirection.Normalize();
            bullet.Direction = newDirection;
            this.Reloaded = false;
            return bullet;
        }

        public override void RestorePosition()
        {
            base.RestorePosition();
            onWindowTicks = 0;
            angle = 0f;
        }
    }

    public class BaseEnemy3 : Enemy
    {
        public BaseEnemy3(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "c";
            Speed = 3f;
            Position = position;
            IsPassingBy = true;
        }

        public override EnemyBullet GetBullet(Player player, Map gameMap)
        {
            var bullet = new EnemyBullet3(this.Position, 0);
            var newDirection = new Vector2(player.Position.X - this.Position.X, (player.Position.Y + gameMap.ScrollRows) - this.Position.Y) + player.Size / 2;
            newDirection.Normalize();
            bullet.Direction = newDirection;
            this.Reloaded = false;
            return bullet;
        }
    }

    public class BaseEnemy4 : Enemy2
    {
        public BaseEnemy4(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "d";
            Speed = 3f;
            Position = position;
            //rangeWidth = .125f;
            //ticksToYRate = 10;
        }
    }

    public class BaseEnemy5 : Enemy
    {
        bool isDisassembling = false;
        bool isAssembling = false;
        float assemblyTimeInMS = 500;
        float disassemblyTimeInMS = 500;
        float maxSplitDistance = 32;
        float splitDistance = 0;
        Texture2D splitSpriteSheet;

        public BaseEnemy5(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "e";
            Speed = 2f;
            Position = position;
            IsPassingBy = false;
            Lives = 3;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
            splitSpriteSheet = GetTexture("splitSpriteSheet");
        }

        public override void UpdateDirection(Player player, Map gameMap)
        {
            if ((this.Position.X / 2) == (int)(this.Position.X / 2)
                || (this.Position.Y / 2) == (int)(this.Position.Y / 2))
            {
                var newDirection = new Vector2(player.Position.X - this.Position.X, (player.Position.Y + gameMap.ScrollRows) - this.Position.Y) + player.Size / 2;
                if (onWindowTicks / (changeDirectionInMS / tickInMS) % 2 == 0)
                    newDirection = new Vector2(newDirection.X, 0);
                else
                    newDirection = new Vector2(0, newDirection.Y);

                newDirection.Normalize();
                this.Direction = newDirection;
            }
        }

        public override void Hit(BaseBullet bullet, int tickCount)
        {
            if (!(isDisassembling || isAssembling))
            {
                base.Hit(bullet, tickCount);

                if (this.Lives > 0)
                    isDisassembling = true;
            }
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, Map gameMap)
        {
            if (isDisassembling)
            {
                if (gameMap.State == MapState.Scrolling && !isAnchored)
                {
                    if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
                    {
                        splitDistance = splitDistance * 1.05f + 1;
                    }

                    if (splitDistance >= maxSplitDistance)
                    {
                        accumElapsedGameTime = TimeSpan.FromSeconds(0);
                        isDisassembling = false;
                        isAssembling = true;
                    }
                    else
                        accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);
                }
            }
            else if (isAssembling)
            {
                if (gameMap.State == MapState.Scrolling && !isAnchored)
                {
                    if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
                    {
                        splitDistance = splitDistance / 1.05f - 1;
                    }

                    if (splitDistance <= 0)
                    {
                        accumElapsedGameTime = TimeSpan.FromSeconds(0);
                        isDisassembling = false;
                        isAssembling = false;
                    }
                    else
                        accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);
                }
            }
            else
            {
                base.Update(gameTime, tickCount, scrollRows, onScreenEnemies, gameMap);
            }
        }

        protected override void DrawAliveEnemy(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            if (isDisassembling || isAssembling)
            {
                if (IsSpriteVisible())
                {
                    var timeInMS = isDisassembling ? disassemblyTimeInMS : assemblyTimeInMS;

                    spriteBatch.Draw(splitSpriteSheet
                        , new Rectangle((int)(TopLeftCorner.X + onWindowPosition.Value.X
                                            - splitDistance)
                                        , (int)(TopLeftCorner.Y + onWindowPosition.Value.Y)
                                        , (int)Size.X * tileWidth
                                        , (int)Size.Y * tileWidth)
                        , new Rectangle(
                            0
                            , 0
                            , (int)Size.X * tileWidth
                            , (int)Size.Y * tileWidth)
                        , Color.White
                        , - (float)((splitDistance / maxSplitDistance) * Math.PI / 6)
                        , new Vector2(tileWidth * Size.X / 2, 0)
                        , SpriteEffects.None
                        , 0);

                    spriteBatch.Draw(splitSpriteSheet
                        , new Rectangle((int)(TopLeftCorner.X + onWindowPosition.Value.X + (Size.X * tileWidth) / 2
                                            + splitDistance)
                                        , (int)(TopLeftCorner.Y + onWindowPosition.Value.Y)
                                        , (int)Size.X * tileWidth
                                        , (int)Size.Y * tileWidth)
                        , new Rectangle(
                            (int)Size.X * tileWidth
                            , 0
                            , (int)Size.X * tileWidth
                            , (int)Size.Y * tileWidth)
                        , Color.White
                        , (float)((splitDistance / maxSplitDistance) * Math.PI / 6)
                        , new Vector2(tileWidth * Size.X / 2, 0)
                        , SpriteEffects.None
                        , 0);
                }
            }
            else
            {
                base.DrawAliveEnemy(spriteBatch, gameTime, tickCount, scrollRows);
            }
        }

        public override void RestorePosition()
        {
            base.RestorePosition();
            Lives = 3;
            isDisassembling = false;
            isAssembling = false;
            splitDistance = 0;
            Speed = 2f;
            IsPassingBy = false;
            Lives = 3;
        }
    }

    public class BaseEnemy6 : Enemy
    {
        public BaseEnemy6(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "f";
            Speed = 3f;
            Position = position;
        }
    }

    public class BaseEnemy7 : Enemy
    {
        public BaseEnemy7(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "g";
            Speed = 2f;
            Position = position;
            IsPassingBy = false;
        }

        public override void UpdateDirection(Player player, Map gameMap)
        {
            if ((this.Position.X / 2) == (int)(this.Position.X / 2)
                || (this.Position.Y / 2) == (int)(this.Position.Y / 2))
            {
                var newDirection = new Vector2(player.Position.X - this.Position.X, (player.Position.Y + gameMap.ScrollRows) - this.Position.Y) + player.Size / 2;
                if (onWindowTicks / (changeDirectionInMS / tickInMS) % 2 == 0)
                    newDirection = new Vector2(newDirection.X, 0);
                else
                    newDirection = new Vector2(0, newDirection.Y);

                newDirection.Normalize();
                this.Direction = newDirection;
            }
        }
    }

    public class BaseEnemy8 : Enemy
    {
        public BaseEnemy8(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "h";
            Speed = 3f;
            Position = position;
        }
    }

    public class BaseEnemy9 : Enemy
    {
        public BaseEnemy9(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "i";
            Speed = 3f;
            Position = position;
        }
    }

    public class BaseEnemy10 : Enemy
    {
        public BaseEnemy10(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "j";
            Speed = 3f;
            Position = position;
        }
    }

    public class BaseEnemy11 : Enemy3
    {
        public BaseEnemy11(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "k";
            Speed = 3f;
            Position = position;
            IsPassingBy = false;
        }

        public override void UpdateDirection(Player player, Map gameMap)
        {
            if (State == CharacterState.Alive
                && gameMap.State == MapState.Scrolling
                && ((this.Position.X / 2) == (int)(this.Position.X / 2)
                    || (this.Position.Y / 2) == (int)(this.Position.Y / 2)))
            {
                var target = new Vector2(player.Position.X >= GameSettings.Instance.WindowTilesSize.X / 2 ? 0 : GameSettings.Instance.WindowTilesSize.X, (player.Position.Y + gameMap.ScrollRows));

                var newDirection = new Vector2(target.X - this.Position.X, target.Y - this.Position.Y) + this.Size / 2;
                if (onWindowTicks / (changeDirectionInMS / tickInMS) % 2 == 0)
                    newDirection = new Vector2(newDirection.X, 0);
                else
                    newDirection = new Vector2(0, newDirection.Y);

                newDirection.Normalize();
                this.Direction = newDirection;
            }
        }
    }

    public class BaseEnemy12 : Enemy
    {
        bool wasHit = false;
        bool isDescending = true;

        public BaseEnemy12(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "l";
            Speed = 1f;
            Position = position;
            Lives = 1000;
            IsPassingBy = true;
            IsFlying = true;
            Direction = new Vector2(position.X < GameSettings.Instance.WindowTilesSize.X / 2 ? -1 : 1, 0);
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows, List<IEnemy> onScreenEnemies, Map gameMap)
        {
            base.Update(gameTime, tickCount, scrollRows, onScreenEnemies, gameMap);

            if (onWindowPosition.HasValue)
            {
                if (!isDescending != onWindowPosition.Value.Y >= this.Size.Y * 2)
                {
                    isDescending = false;
                    Direction = new Vector2(1, 0);
                }
            }
        }

        public override void UpdateDirection(Player player, Map gameMap)
        {
            if (wasHit)
            {
                Direction = new Vector2(0, 1);
                Speed = 10;
            }
            else
            {
                if (isDescending)
                {
                    Direction = new Vector2(0, 1);
                    Speed = 1;
                }
                else
                {
                    if (Position.X <= GameSettings.Instance.WindowTilesSize.X / 4)
                        Direction = new Vector2(1, 0);
                    else if (Position.X >= (GameSettings.Instance.WindowTilesSize.X - GameSettings.Instance.WindowTilesSize.X / 4))
                        Direction = new Vector2(-1, 0);
                }
            }
        }

        public override void Hit(BaseBullet bullet, int tickCount)
        {
            if (!isDescending)
            {
                base.Hit(bullet, tickCount);

                wasHit = true;
            }
        }

        public override void RestorePosition()
        {
            base.RestorePosition();
            Lives = 1000;
            wasHit = false;
            isDescending = false;
        }
    }

    public class BaseEnemy13 : Enemy
    {
        public BaseEnemy13(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "m";
            Speed = 3f;
            Position = position;
            isAnchored = true;
        }
    }

    public class BaseEnemy14 : Enemy
    {
        public BaseEnemy14(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "n";
            Speed = 3f;
            Position = position;
            isAnchored = true;
        }
    }

    public class BaseEnemy15 : Enemy
    {
        public BaseEnemy15(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "o";
            Speed = 3f;
            Position = position;
            isAnchored = true;
        }
    }

    public class BaseEnemy16 : Enemy
    {
        public BaseEnemy16(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "p";
            Speed = 3f;
            Position = position;
            isAnchored = true;
        }
    }

    public class BaseEnemy17 : Enemy
    {
        public BaseEnemy17(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "q";
            Speed = 3f;
            Position = position;
            isAnchored = true;
        }
    }

    public class BaseEnemy18 : Enemy
    {
        public BaseEnemy18(Vector2 position, int groupId)
            : base(position, groupId)
        {
            code = "r";
            Speed = 3f;
            Position = position;
            isAnchored = true;
        }
    }

    public class EnemyDeathMessage { public BaseEnemy Enemy { get; set; } }
    public class EnemyShotMessage { public BaseEnemy Enemy { get; set; } }
}
