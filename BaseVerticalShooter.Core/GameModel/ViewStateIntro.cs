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
    public class ViewStateIntro : ViewStateBase
    {
        Song introSong;

        public ViewStateIntro(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
            :base(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager)
        {

        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            base.LoadContent(contentHelper);
            introSong = contentHelper.GetContent<Song>("Intro");
        }

        public override void Update(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped && currentSong != introSong)
            {
                PlaySong(introSong, (s, e) =>
                {
                    AfterIntroSong();
                });
            }
        }

        private void AfterIntroSong()
        {
            if (currentSong == introSong)
            {
                if (MediaPlayer.State == MediaState.Stopped)
                {
                    NewMessenger.Default.Send<ViewStateChangedMessage>(new ViewStateChangedMessage { ViewState = ViewState.Playing });
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (currentSong == finishLevelSong)
                DrawViewStateFinishLevel(spriteBatch, gameTime, levelName);
            else
                DrawViewStateIntro(spriteBatch, gameTime, levelName);
        }

        void DrawViewStateIntro(SpriteBatch spriteBatch, GameTime gameTime, string levelName)
        {
            DrawStringCentralized(spriteBatch, string.Format("LEVEL {0}", levelNumber), levelName);
        }

        void DrawViewStateFinishLevel(SpriteBatch spriteBatch, GameTime gameTime, string levelName)
        {
            DrawStringCentralized(spriteBatch, string.Format("CONGRATULATIONS!", "YOU HAVE DEFEATED {0}!", levelName));
        }

    }
}
