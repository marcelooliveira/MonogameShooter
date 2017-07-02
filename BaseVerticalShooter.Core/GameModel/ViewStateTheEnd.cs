using BaseVerticalShooter.GameModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ScreenControlsSample;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel
{
    public class ViewStateTheEnd : ViewStateBase
    {
        IBasePlayer player;

        public ViewStateTheEnd(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
            :base(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager)
        {

        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            player = new Player();
            player.LoadContent(contentHelper);

            base.LoadContent(contentHelper);
        }

        public override void Update(GameTime gameTime)
        {
            player.Update(gameTime, tickCount, gameMap.ScrollRows);
            princess.Update(gameTime, tickCount, gameMap.ScrollRows);

            if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
            {
                accumElapsedGameTime = TimeSpan.FromSeconds(0);
                tickCount++;
            }
            accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);

            var awaitingPrincessPosition = new Vector2(
                GameSettings.Instance.WindowTilesSize.X / 2 - player.Size.X / 2, 
                GameSettings.Instance.WindowTilesSize.Y / 2 + player.Size.Y / 2);
            var deltaPosition = awaitingPrincessPosition - player.Position;

            if (Math.Abs(deltaPosition.Y) > .1)
            {
                player.Position += new Vector2(0, deltaPosition.Y > 0 ? 1 : -1) / 8;
            }
            else if (Math.Abs(deltaPosition.X) > .1)
            {
                player.Position += new Vector2(deltaPosition.X > 0 ? 1 : -1, 0) / 8;
            }
            else
            {
                showLove = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            gameMap.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);

            player.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            player.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);

            princess.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);

            if (showLove)
            {
                DrawLove(spriteBatch);
            }

            var textHeight = font.MeasureString("SCORE").Y;
            spriteBatch.DrawString(font, string.Format(GameResources.ScoreHeader), new Vector2(topLeftCorner.X, screenSize.Y - textHeight * 2), Color.White);
            spriteBatch.DrawString(font, string.Format("   {0:d5}    {1:d5}    {2:d2}     {3:d2}  ", score, hiScore, (int)(player.Lives), levelNumber), new Vector2(topLeftCorner.X, screenSize.Y - textHeight), Color.White);

            if (gameMap.State == MapState.Paused)
                DrawStringCentralized(spriteBatch, GameResources.Paused);
        }

        protected void DrawLove(SpriteBatch spriteBatch)
        {
            Rectangle loveRectangle = new Rectangle(
                loveTexture.Height * ((tickCount / 4) % (loveTexture.Width / loveTexture.Height)),
                0,
                loveTexture.Height,
                loveTexture.Height);
            var frameIndex = (tickCount / 2) % 2;
            
            spriteBatch.Draw(
                loveTexture,
                topLeftCorner + (player.Position - new Vector2(0, player.Size.Y)) * tileWidth,
                loveRectangle,
                Color.White);

            DrawStringCentralized(spriteBatch, "", "", "LOVE IS FOREVER...");
        }
    }
}
