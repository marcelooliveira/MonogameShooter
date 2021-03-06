﻿using BaseVerticalShooter;
using BaseVerticalShooter.Core;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shooter.GameModel
{
    public class BaseBoss : PhysicalObject
    {
        Texture2D bossSpriteSheet;
        Texture2D bossHitSpriteSheet;
        Texture2D destructionSpriteSheet;
        SoundEffectInstance fireSoundInstance;
        SoundEffectInstance hitSoundEffectInstance;
        SoundEffectInstance bossExplosionSoundEffectInstance;
        public static Vector2 ScreenSize = new Vector2(800f, 480f);
        public static Vector2 GameScreenSize = new Vector2(512, 384);
        public float Lives = 20f;
        int walkDirectionX = 0;
        int respawnTimeOut = 20;
        int blinkTickStart = 0;
        Vector2 windowTilesSize;
        protected int levelNumber = 1;
        TimeSpan accumElapsedGameTime = TimeSpan.FromSeconds(0);
        int hitTimeoutInMS = 500;
        TimeSpan accumHitGameTime = TimeSpan.FromSeconds(1000);
        protected int shotTimeOutMs = 1000;
        TimeSpan accumShotTime = TimeSpan.FromMilliseconds(0);

        protected BossMovement bossMovement = BossMovement.Fixed;
        public BossMovement BossMovement
        {
            get { return bossMovement; }
            set { bossMovement = value; }
        }

        CharacterState state = CharacterState.Alive;
        public CharacterState State
        {
            get { return state; }
            set { state = value; }
        }

        bool reloaded = true;
        public bool Reloaded
        {
            get { return reloaded; }
            set { reloaded = value; }
        }

        public BaseBoss(Vector2 windowTilesSize, int levelNumber, BossMovement bossMovement)
        : base()
        {
            this.windowTilesSize = windowTilesSize;
            this.levelNumber = levelNumber;
            this.bossMovement = bossMovement;

            bossSpriteSheet = bossSpriteSheet ?? GetTexture(GetBossSpriteSheetName(levelNumber));
            bossHitSpriteSheet = bossHitSpriteSheet ?? GetTexture(GetBossHitSpriteSheetName(levelNumber));
            Position = new Vector2(14, 4);
        }

        protected virtual string GetBossHitSpriteSheetName(int levelNumber)
        {
            string bossHitSpriteSheetName;
            bossHitSpriteSheetName = string.Format("Boss{0}HitSpriteSheet", levelNumber);
            return bossHitSpriteSheetName;
        }

        protected virtual string GetBossSpriteSheetName(int levelNumber)
        {
            string bossSpriteSheetName;
            bossSpriteSheetName = string.Format("Boss{0}SpriteSheet", levelNumber);
            return bossSpriteSheetName;
        }

        public override void LoadContent(ContentManager content)
        {
            Size = new Vector2(6, 6);
            destructionSpriteSheet = destructionSpriteSheet ?? ContentHelper.Instance.GetContent<Texture2D>("BossDestructionSpriteSheet");
            hitSoundEffectInstance = hitSoundEffectInstance ?? ContentHelper.Instance.GetSoundEffectInstance(this.GetType().Name + "Hit");
            bossExplosionSoundEffectInstance = bossExplosionSoundEffectInstance ?? ContentHelper.Instance.GetSoundEffectInstance("BossExplosion");
            fireSoundInstance = fireSoundInstance ?? ContentHelper.Instance.GetSoundEffectInstance("Fire");
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows)
        {
            UpdateBlink(tickCount);

            if (state == CharacterState.Alive)
            {
                if (!reloaded && accumShotTime >= TimeSpan.FromMilliseconds(shotTimeOutMs))
                {
                    accumShotTime = TimeSpan.FromSeconds(0);
                    reloaded = true;
                }
                accumShotTime = accumShotTime.Add(gameTime.ElapsedGameTime);

                switch(bossMovement)
                {
                    case BossMovement.WalkHorizontal:
                        WalkHorizontal(gameTime, tickCount);
                        break;
                    case BossMovement.FloatSenoidHorizontal:
                        FloatSenoidHorizontal(gameTime, tickCount);
                        break;
                    default:
                        StayPut(gameTime, tickCount);
                        break;
                }                
            }

            accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);

            if (accumHitGameTime.TotalMilliseconds < hitTimeoutInMS)
                accumHitGameTime = accumHitGameTime.Add(gameTime.ElapsedGameTime);
        }

        private void WalkRandomHorizontal(GameTime gameTime, int tickCount)
        {
            if (tickCount % 20 == 0)
            {
                walkDirectionX = (walkDirectionX + (new Random().Next(10) % 2) + 1) % 3 - 1;
            }

            if (tickCount % 4 == 0)
            {
                var newPosX = Position.X + walkDirectionX;
                if (newPosX > 2 && newPosX < GameSettings.Instance.WindowTilesSize.X - 2 - this.Size.X)
                {
                    Position = new Vector2(newPosX, Position.Y);
                }
                else
                {
                    walkDirectionX *= -1;
                }
            }
        }

        private void WalkHorizontal(GameTime gameTime, int tickCount)
        {
            var x = (float)(-this.Size.X / 2
                + (GameSettings.Instance.WindowTilesSize.X / 2)
                + (GameSettings.Instance.WindowTilesSize.X / 4) * Math.Sin(Math.PI * (accumElapsedGameTime.TotalMilliseconds / 2000f)));

            Position = new Vector2(
                x
                , (float)(this.Size.Y / 2));
        }

        private void FloatSenoidHorizontal(GameTime gameTime, int tickCount)
        {
            var x = (float)(-this.Size.X / 2
                + (GameSettings.Instance.WindowTilesSize.X / 2)
                + (GameSettings.Instance.WindowTilesSize.X / 4) * Math.Sin(Math.PI * (accumElapsedGameTime.TotalMilliseconds/ 2000f)));

            Position = new Vector2(
                x
                , (float)(this.Size.Y / 2 + Math.Sin(2 * Math.PI * (accumElapsedGameTime.TotalMilliseconds / 2000f))));
        }

        private void StayPut(GameTime gameTime, int tickCount)
        {

        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            var bossFrameCount = bossSpriteSheet.Width / (tileWidth * Size.Y);
            var destructionFrameCount = destructionSpriteSheet.Width / destructionSpriteSheet.Height;

            Rectangle bossRectangle = new Rectangle((int)(tickCount % bossFrameCount * Size.X * tileWidth)
                , 0
                , (int)(Size.X * tileWidth)
                , (int)(Size.Y * tileWidth));

            Rectangle destructionRectangle = new Rectangle((int)(tickCount % destructionFrameCount * destructionSpriteSheet.Height)
                , 0
                , (int)(destructionSpriteSheet.Height)
                , (int)(destructionSpriteSheet.Height));

            switch (State)
            {
                case CharacterState.Dead:
                    if (tickCount <= blinkTickStart + respawnTimeOut)
                    {
                        DrawHitBoss(spriteBatch, scrollRows, bossRectangle);

                        spriteBatch.Draw(destructionSpriteSheet
                            , TopLeftCorner
                            + Position * tileWidth
                                - new Vector2(0, scrollRowHeight * scrollRows)
                            , destructionRectangle
                            , Color.White);
                    }
                    break;
                case CharacterState.Alive:
                    if (accumHitGameTime.TotalMilliseconds < hitTimeoutInMS)
                    {
                        DrawHitBoss(spriteBatch, scrollRows, bossRectangle);
                    }
                    else
                    {
                        DrawAliveBoss(spriteBatch, scrollRows, bossRectangle);
                    }
                    break;
            }
        }

        private void DrawAliveBoss(SpriteBatch spriteBatch, float scrollRows, Rectangle bossRectangle)
        {
            spriteBatch.Draw(bossSpriteSheet,
                TopLeftCorner
                + Position * tileWidth
                - new Vector2(0, scrollRowHeight * scrollRows),
                bossRectangle,
                Color.White);
        }

        private void DrawHitBoss(SpriteBatch spriteBatch, float scrollRows, Rectangle bossRectangle)
        {
            if ((int)(accumHitGameTime.TotalMilliseconds / (hitTimeoutInMS / 10)) % 2 == 0)
            {
                spriteBatch.Draw(bossHitSpriteSheet,
                    TopLeftCorner
                    + Position * tileWidth
                    - new Vector2(0, scrollRowHeight * scrollRows),
                    bossRectangle,
                    Color.White);
            }
            else
            {
                DrawAliveBoss(spriteBatch, scrollRows, bossRectangle);
            }
        }

        public void ProcessGamePad(IScreenPad screenPad, PhysicalObject gameMap)
        {
            //var candidatePos = this.Position;
            //var candidateXPos = this.Position;
            //var candidateYPos = this.Position;

            //if (Math.Abs(screenPad.LeftStick.X) + Math.Abs(screenPad.LeftStick.Y) > 0)
            //{
            //    screenPad.LeftStick.Normalize();
            //    candidatePos = this.Position + new Vector2(screenPad.LeftStick.X, -screenPad.LeftStick.Y) * .5f;
            //    candidateXPos = this.Position + new Vector2(screenPad.LeftStick.X, 0) * .5f;
            //    candidateYPos = this.Position + new Vector2(0, -screenPad.LeftStick.Y) * .5f;
            //}

            //var collisionResult = gameMap.TestCollision(this, candidatePos);
            //var collisionXResult = gameMap.TestCollision(this, candidateXPos);
            //var collisionYResult = gameMap.TestCollision(this, candidateYPos);
            //if (collisionResult.CollisionType == CollisionType.None)
            //{
            //    this.Position = candidatePos;
            //}
            //else if (collisionXResult.CollisionType == CollisionType.None)
            //{
            //    this.Position = candidateXPos;
            //}
            //else if (collisionYResult.CollisionType == CollisionType.None)
            //{
            //    this.Position = candidateYPos;
            //}
        }

        public void Hit()
        {
            Lives--;
            if (Lives <= 0)
            {
                State = CharacterState.Dead;
                fireSoundInstance.Play();
            }
            else
            {
                hitSoundEffectInstance.Volume = .4f;
                hitSoundEffectInstance.Play();
                accumHitGameTime = TimeSpan.FromMilliseconds(0);
            }
        }

        protected void UpdateBlink(int tickCount)
        {
            if (blinkTickStart == 0 && state == CharacterState.Dead)
            {
                blinkTickStart = tickCount;
                bossExplosionSoundEffectInstance.Play();
                MediaPlayer.Stop();
                //NewMessenger.Default.Send(new BossDeathMessage { Boss = this });
            }
            else if (state == CharacterState.Dead && tickCount > blinkTickStart + respawnTimeOut)
            {
                state = CharacterState.Terminated;
                //blinkTickStart = 0;
                NewMessenger.Default.Send(new BossDeathMessage { Boss = this });
            }
        }

        public virtual EnemyBullet GetBullet(IBasePlayer player, IBaseMap gameMap)
        {
            var bullet = new EnemyBullet2(this.Position + this.Size / 2, 0);
            var newDirection = new Vector2(player.Position.X - bullet.Position.X, (player.Position.Y + gameMap.ScrollRows) - bullet.Position.Y) + player.Size / 2;
            newDirection.Normalize();
            bullet.Direction = newDirection;
            reloaded = false;
            return bullet;
        }
    }

    public class BossDeathMessage { public BaseBoss Boss { get; set; } }
}
