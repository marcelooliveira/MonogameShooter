using BaseVerticalShooter;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.GameModel;
using BaseVerticalShooter.Core.Input;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Input;
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
        const float STEP_LENGTH_WINDOWSPHONE = .25f;
        const float STEP_LENGTH_WINDOWS = .15f;
        SpriteLine lastSpriteLine = SpriteLine.FaceUp;
        float lastScrollRows = 0;

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

        Vector2 spriteDirection = Vector2.Zero;
        public Vector2 SpriteDirection
        {
            get { return spriteDirection; }
        }

        Vector2 lastDirection = new Vector2(0, -1);
        public Vector2 LastDirection
        {
            get
            {
                return lastDirection;
            }
            set
            {
                lastDirection = value;
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

        Vector2 savedPosition = GameSettings.Instance.WindowTilesSize / 2;
        public Vector2 SavedPosition
        {
            get { return savedPosition; }
            set { savedPosition = value; }
        }

        public BasePlayer()
            : base()
        {
            Size = new Vector2(2f, 2f);
            screenPad = BaseResolver.Instance.Resolve<IScreenPad>();
        }

        public override void Update(GameTime gameTime, int tickCount, float scrollRows)
        {
            ScrollPlayer(scrollRows);
            var shouldStartBlinking = blinkTickStart == 0 && state == CharacterState.Dead;
            var shouldStopBlinking = state == CharacterState.Dead && tickCount > blinkTickStart + respawnTimeOut;
            if (shouldStartBlinking)
            {
                blinkTickStart = tickCount;
            }
            else if (shouldStopBlinking)
            {
                State = CharacterState.Alive;
            }
            spriteDirection = GetSpriteDirection();
        }

        private void ScrollPlayer(float scrollRows)
        {
            if (lastScrollRows > 0)
            {
                if (scrollRows > lastScrollRows)
                    lastScrollRows = scrollRows;

                var deltaScrollRows = lastScrollRows - scrollRows;
                var candidatePosition = this.position + new Vector2(0, deltaScrollRows);
                if (candidatePosition.Y + this.Size.Y < GameSettings.Instance.WindowTilesSize.Y)
                    this.position = candidatePosition;
            }
            lastScrollRows = scrollRows;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, int tickCount, float scrollRows)
        {
            switch (State)
            {
                case CharacterState.Dead:
                    DrawDead(spriteBatch, tickCount);
                    break;
                default:
                    DrawAlive(spriteBatch, tickCount);
                    break;
            }
        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            playerSpriteSheet = playerSpriteSheet ?? contentHelper.GetContent<Texture2D>("PlayerSpriteSheet");
            destructionSpriteSheet = destructionSpriteSheet ?? contentHelper.GetContent<Texture2D>("PlayerDestructionSpriteSheet");
            fireSoundInstance = fireSoundInstance ?? contentHelper.GetSoundEffectInstance("Fire");
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

        private void DrawAlive(SpriteBatch spriteBatch, int tickCount)
        {
            Rectangle playerRectangle = GetAlivePlayerRectangle(screenPad, tickCount);
            spriteBatch.Draw(playerSpriteSheet,
                TopLeftCorner
                + Position * TileWidth
                , playerRectangle, Color.White);
        }

        private void DrawDead(SpriteBatch spriteBatch, int tickCount)
        {
            var destructionStepCount = destructionSpriteSheet.Width / (TileWidth * Size.Y);
            Rectangle playerRectangle = new Rectangle((int)(((tickCount - blinkTickStart) % destructionStepCount) * (int)(Size.X * TileWidth)), 0, (int)(Size.X * TileWidth), (int)(Size.Y * TileWidth));
            spriteBatch.Draw(destructionSpriteSheet, TopLeftCorner + Position * TileWidth, playerRectangle, Color.White);
        }

        protected virtual Rectangle GetAlivePlayerRectangle(IScreenPad screenPad, int tickCount)
        {
            var frameIndex = 0;
            var frameIndexWhenIdle = (tickCount / 2) % 2;
            var frameIndexWhenWalking = (frameIndex
                    + (int)(Position.X)
                    + (int)(Position.Y)) % 2;

            if (InputManager.Instance.IsIdle(screenPad))
                frameIndex = frameIndexWhenIdle;
            else
                frameIndex = frameIndexWhenWalking;
                    
            var playerRectangle = new Rectangle(
                frameIndex * (int)(Size.X * TileWidth)
                , 0
                , (int)(Size.X * TileWidth)
                , (int)(Size.Y * TileWidth));
            return playerRectangle;
        }

        public override CollisionResult TestCollision(IPhysicalObject that, Vector2 thatNewPosition, float scrollRows)
        {
            return base.TestCollision(that, thatNewPosition, scrollRows);
        }

        public void ProcessGamePad(IScreenPad screenPad, PhysicalObject gameMap, float scrollRows)
        {
            float stepLength = GetStepLength();

            var candidatePositions = SetupScreenPadCandidatePositions(screenPad, stepLength);
            candidatePositions = SetupKeyboardCandidatePositions(stepLength, candidatePositions);
            NormalizeDirection(candidatePositions);
            SetNewPosition(gameMap, scrollRows, candidatePositions);
        }

        private void NormalizeDirection(CandidatePositions candidatePositions)
        {
            direction = candidatePositions.CandidatePos - this.position;
            direction.Normalize();
            //if (direction != Vector2.Zero
            //    && !float.IsNaN(direction.X) && !float.IsNaN(direction.Y))
            //    lastDirection = direction;
        }

        private void SetNewPosition(PhysicalObject gameMap, float scrollRows, CandidatePositions candidatePositions)
        {
            var collisionResult = gameMap.TestCollision(this, candidatePositions.CandidatePos, scrollRows);
            var collisionXResult = gameMap.TestCollision(this, candidatePositions.CandidateXPos, scrollRows);
            var collisionYResult = gameMap.TestCollision(this, candidatePositions.CandidateYPos, scrollRows);

            if (collisionResult.CollisionType == CollisionType.None)
                this.Position = candidatePositions.CandidatePos;
            else if (collisionXResult.CollisionType == CollisionType.None)
                this.Position = candidatePositions.CandidateXPos;
            else if (collisionYResult.CollisionType == CollisionType.None)
                this.Position = candidatePositions.CandidateYPos;
        }

        private CandidatePositions SetupKeyboardCandidatePositions(float stepLength, CandidatePositions candidatePositions)
        {
            var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                candidatePositions.CandidatePos -= new Vector2(stepLength, 0);
                candidatePositions.CandidateXPos = this.Position - new Vector2(stepLength, 0);
            }
            else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                candidatePositions.CandidatePos += new Vector2(stepLength, 0);
                candidatePositions.CandidateXPos = this.Position + new Vector2(stepLength, 0);
            }

            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                candidatePositions.CandidatePos -= new Vector2(0, stepLength);
                candidatePositions.CandidateYPos = this.Position - new Vector2(0, stepLength);
            }
            else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                candidatePositions.CandidatePos += new Vector2(0, stepLength);
                candidatePositions.CandidateYPos = this.Position + new Vector2(0, stepLength);
            }
            return candidatePositions;
        }

        private CandidatePositions SetupScreenPadCandidatePositions(IScreenPad screenPad, float stepLength)
        {
            var candidatePositions = new CandidatePositions(this.Position);

            if (!screenPad.LeftStickIsIdle)
            {
                screenPad.LeftStick.Normalize();
                candidatePositions.CandidatePos = this.Position + new Vector2(screenPad.LeftStick.X, -screenPad.LeftStick.Y) * stepLength;
                candidatePositions.CandidateXPos = this.Position + new Vector2(screenPad.LeftStick.X, 0) * stepLength;
                candidatePositions.CandidateYPos = this.Position + new Vector2(0, -screenPad.LeftStick.Y) * stepLength;
            }
            return candidatePositions;
        }

        private static float GetStepLength()
        {
            float stepLength = 0;
            if (GameSettings.Instance.PlatformType == PlatformType.WindowsPhone)
                stepLength = STEP_LENGTH_WINDOWSPHONE;

            if (GameSettings.Instance.PlatformType == PlatformType.Windows)
                stepLength = STEP_LENGTH_WINDOWS;
            return stepLength;
        }

        public void Initialize()
        {
            Respawn();
            Lives = 3f;
            savedPosition =
            Position =
                new Vector2(10, 14);
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

        public SpriteLine GetSpriteLine()
        {
            SpriteLine spriteLine = lastSpriteLine;

            var isGoingDown = Direction.Y > 0;
            var isGoingMostlyLeft = Direction.X < 0 && Math.Abs(Direction.X) > Math.Abs(Direction.Y);
            var isGoingMostlyRight = Direction.X > 0 && Math.Abs(Direction.X) > Math.Abs(Direction.Y);
            var isGoingMostlyUp = Direction.Y < 0 && Math.Abs(Direction.X) < Math.Abs(Direction.Y);

            if (isGoingDown)
                spriteLine = SpriteLine.FaceDown;
            else if (isGoingMostlyLeft)
                spriteLine = SpriteLine.FaceLeft;
            else if (isGoingMostlyRight)
                spriteLine = SpriteLine.FaceRight;
            else if (isGoingMostlyUp)
                spriteLine = SpriteLine.FaceUp;

            lastSpriteLine = spriteLine;

            return spriteLine;
        }

        public Vector2 GetSpriteDirection()
        {
            return new Vector2[] {
                        new Vector2(0, 1),
                        new Vector2(-1, 0),
                        new Vector2(1, 0),
                        new Vector2(0, -1)
                    }[(int)this.GetSpriteLine()];
        }
    }

    public class CandidatePositions
    {
        public CandidatePositions(Vector2 StartPosition)
        {
            CandidatePos = 
            CandidateXPos = 
            CandidateYPos = StartPosition;
        }

        public Vector2 CandidatePos { get; set;}
        public Vector2 CandidateXPos { get; set; }
        public Vector2 CandidateYPos { get; set; }
    }

    public class PlayerDeathMessage
    {
        public int RemainingLives { get; set; }
    }

    public enum SpriteLine
    {
        FaceDown = 0,
        FaceLeft = 1,
        FaceRight = 2,
        FaceUp = 3
    }
}
