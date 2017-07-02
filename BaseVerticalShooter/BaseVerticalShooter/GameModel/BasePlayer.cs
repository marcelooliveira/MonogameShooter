using BaseVerticalShooter;
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
    public abstract class BasePlayer : PhysicalObject, BaseVerticalShooter.GameModel.IBasePlayer
    {
        Texture2D playerSpriteSheet;
        Texture2D destructionSpriteSheet;
        SoundEffectInstance fireSoundInstance;
        private float lives = 3f;
        public float Lives
        {
            get { return lives; }
            set { lives = value; }
        }

        int respawnTimeOut = 20;
        int blinkTickStart = 0;
        IScreenPad screenPad;

        Vector2 direction = new Vector2(0, 0);
        public Vector2 Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        bool canShoot = true;
        public bool CanShoot
        {
            get { return canShoot; }
            set { canShoot = value; }
        }

        CharacterState state = CharacterState.Alive;
        public CharacterState State
        {
            get { return state; }
            set { state = value; }
        }

        Vector2 savedPosition = new Vector2(10, 10);
        public Vector2 SavedPosition
        {
            get { return savedPosition; }
            set { savedPosition = value; }
        }

        public BasePlayer()
            : base()
        {
            Size = new Vector2(2f, 2f);
            screenPad = Resolver.Instance.Resolve<IScreenPad>();
        }

        public override void LoadContent(ContentManager content)
        {
            playerSpriteSheet = playerSpriteSheet ?? ContentHelper.Instance.GetContent<Texture2D>("PlayerSpriteSheet");
            destructionSpriteSheet = destructionSpriteSheet ?? ContentHelper.Instance.GetContent<Texture2D>("PlayerDestructionSpriteSheet");
            fireSoundInstance = fireSoundInstance ?? ContentHelper.Instance.GetSoundEffectInstance("Fire");
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows)
        {
            if (Position.Y >= GameSettings.Instance.WindowTilesSize.Y)
            {
                ProcessDeath(tickCount, true);
            }

            if (blinkTickStart == 0 && state == CharacterState.Dead)
            {
                blinkTickStart = tickCount;
            }
            else if (state == CharacterState.Dead && tickCount > blinkTickStart + respawnTimeOut)
            {
                State = CharacterState.Alive;
            }
        }

        public void ProcessDeath(int tickCount, bool respawn = false)
        {
             State = CharacterState.Dead;
            CanShoot = true;
            fireSoundInstance.Play();
            Lives--;
            blinkTickStart = tickCount;

            if (respawn)
                Respawn(tickCount);

            NewMessenger.Default.Send(new PlayerDeathMessage { RemainingLives = (int)Lives });
        }

        private void Respawn(int tickCount)
        {
            Position = new Vector2(GameSettings.Instance.WindowTilesSize.X / 2,
            GameSettings.Instance.WindowTilesSize.Y - this.Size.Y - 1);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            Rectangle playerRectangle;
            switch (State)
            {
                case CharacterState.Dead:
                    var destructionStepCount = destructionSpriteSheet.Width / (tileWidth * Size.Y);
                    playerRectangle = new Rectangle((int)(((tickCount - blinkTickStart) % 2) * (int)(Size.X * tileWidth)), 0, (int)(Size.X * tileWidth), (int)(Size.Y * tileWidth));
                    spriteBatch.Draw(destructionSpriteSheet, TopLeftCorner + Position * tileWidth, playerRectangle, Color.White);
                    break;
                default:
                    playerRectangle = GetAlivePlayerRectangle(screenPad, tickCount);

                    spriteBatch.Draw(playerSpriteSheet,
                        TopLeftCorner
                        + Position * tileWidth
                        , playerRectangle, Color.White);
                    break;
            }
        }

        protected virtual Rectangle GetAlivePlayerRectangle(IScreenPad screenPad, int tickCount)
        {
            Rectangle playerRectangle;
            var frameIndex = 0;
            if (screenPad.LeftStick.X + screenPad.LeftStick.Y == 0)
                frameIndex = (tickCount / 2) % 2;
            else
                frameIndex =
                    (frameIndex
                    + (int)(Position.X)
                    + (int)(Position.Y)) % 2;
            playerRectangle = new Rectangle(frameIndex * (int)(Size.X * tileWidth), 0, (int)(Size.X * tileWidth), (int)(Size.Y * tileWidth));
            return playerRectangle;
        }

        public override CollisionResult TestCollision(IPhysicalObject that, Vector2 thatNewPosition, float scrollRows)
        {
            return base.TestCollision(that, thatNewPosition, scrollRows);
        }

        public void ProcessGamePad(IScreenPad screenPad, PhysicalObject gameMap, float scrollRows)
        {
            var candidatePos = this.Position;
            var candidateXPos = this.Position;
            var candidateYPos = this.Position;

#if WINDOWS_PHONE_APP
            var stepLength = .25f;
#endif

#if WINDOWS_APP
            var stepLength = .15f;
#endif

            if (Math.Abs(screenPad.LeftStick.X) + Math.Abs(screenPad.LeftStick.Y) > 0)
            {
                screenPad.LeftStick.Normalize();
                candidatePos = this.Position + new Vector2(screenPad.LeftStick.X, -screenPad.LeftStick.Y) * stepLength;
                candidateXPos = this.Position + new Vector2(screenPad.LeftStick.X, 0) * stepLength;
                candidateYPos = this.Position + new Vector2(0, -screenPad.LeftStick.Y) * stepLength;
            }

            var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                candidatePos -= new Vector2(stepLength, 0);
                candidateXPos = this.Position - new Vector2(stepLength, 0);
            }
            else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                candidatePos += new Vector2(stepLength, 0);
                candidateXPos = this.Position + new Vector2(stepLength, 0);
            }

            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                candidatePos -= new Vector2(0, stepLength);
                candidateYPos = this.Position - new Vector2(0, stepLength);
            }
            else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                candidatePos += new Vector2(0, stepLength);
                candidateYPos = this.Position + new Vector2(0, stepLength);
            }

            direction = candidatePos - this.position;

            direction.Normalize();

            var collisionResult = gameMap.TestCollision(this, candidatePos, scrollRows);
            var collisionXResult = gameMap.TestCollision(this, candidateXPos, scrollRows);
            var collisionYResult = gameMap.TestCollision(this, candidateYPos, scrollRows);
            if (collisionResult.CollisionType == CollisionType.None)
            {
                this.Position = candidatePos;
            }
            else if (collisionXResult.CollisionType == CollisionType.None)
            {
                this.Position = candidateXPos;
            }
            else if (collisionYResult.CollisionType == CollisionType.None)
            {
                this.Position = candidateYPos;
            }
        }

        public void Initialize()
        {
            Respawn();
            Lives = 3f;
            savedPosition =
            Position =
                new Vector2(10, 10);
        }

        public void SavePosition()
        {
            savedPosition = this.Position;
        }

        public void Respawn()
        {
            CanShoot = true;
            State = CharacterState.Alive;
            Position = SavedPosition;
        }

        public int GetSpriteLine()
        {
            int spriteLine = 3;

            if (Direction.Y > 0)
            {
                spriteLine = 0;
            }
            else if (Direction.X < 0 && Math.Abs(Direction.X) > Math.Abs(Direction.Y))
            {
                spriteLine = 1;
            }
            else if (Direction.X > 0 && Math.Abs(Direction.X) > Math.Abs(Direction.Y))
            {
                spriteLine = 2;
            }
            else if (Direction.Y < 0 && Math.Abs(Direction.X) < Math.Abs(Direction.Y))
            {
                spriteLine = 3;
            }

            return spriteLine;
        }

        public Vector2 GetSpriteDirection()
        {
            return new Vector2[] {
                        new Vector2(0, 1),
                        new Vector2(-1, 0),
                        new Vector2(1, 0),
                        new Vector2(0, -1)
                    }[this.GetSpriteLine()];
        }
    }

    public class PlayerDeathMessage
    {
        public int RemainingLives { get; set; }
    }
}
