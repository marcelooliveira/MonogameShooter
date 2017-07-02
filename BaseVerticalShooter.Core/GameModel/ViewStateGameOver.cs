using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ScreenControlsSample;
using Shooter;
using Shooter.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core.GameModel
{
    public class ViewStateGameOver : ViewStateBase
    {
        Song gameOverSong;
        public ViewStateGameOver(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
            :base(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager)
        {

        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            gameOverSong = contentHelper.GetContent<Song>("GameOver");
            base.LoadContent(contentHelper);
        }

        public override void Update(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped && currentSong != gameOverSong)
            {
                PlaySong(gameOverSong, (s, e) =>
                {
                    if (currentSong == gameOverSong && MediaPlayer.State == MediaState.Stopped)
                    {
                        NewMessenger.Default.Send(new ViewStateChangedMessage { ViewState = ViewState.Menu });
                    }
                });
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            DrawStringCentralized(spriteBatch, "GAME OVER");
        }

    }
}
