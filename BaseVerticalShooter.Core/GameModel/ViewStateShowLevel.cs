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
    public class ViewStateShowLevel : ViewStateBase
    {
        Song dyingSong;

        public ViewStateShowLevel(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
            :base(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager)
        {

        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            dyingSong = contentHelper.GetContent<Song>("Death");

            base.LoadContent(contentHelper);
        }

        public override void Update(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Volume = 0;
                PlaySong(dyingSong, (s, e) =>
                {
                    AfterDyingSong();
                });
            }
        }

        private void AfterDyingSong()
        {
            if (currentSong == dyingSong)
            {
                if (MediaPlayer.State == MediaState.Stopped)
                {
                    MediaPlayer.Volume = 1f;
                    NewMessenger.Default.Send<ViewStateChangedMessage>(new ViewStateChangedMessage { ViewState = ViewState.Playing });
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            DrawStringCentralized(spriteBatch, string.Format("LEVEL {0}", levelNumber), levelName);
        }

    }
}
