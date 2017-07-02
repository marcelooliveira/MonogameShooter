using BaseVerticalShooter.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    public class ViewStateLoading : ViewStateBase
    {
        enum LoadingState
        {
            Fonts = 0,
            Textures = 1,
            Sounds = 2,
            Completed = 3
        }

        LoadingState loadingState = LoadingState.Fonts;

        public ViewStateLoading(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
            :base(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager)
        {
        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            this.contentHelper = contentHelper;
            font = this.contentHelper.GetContent<SpriteFont>("Super-Contra-NES");
        }

        private void LoadFonts()
        {
            //font = this.contentHelper.GetContent<SpriteFont>("Super-Contra-NES");
        }

        private void LoadTextures()
        {
            titleTexture = this.contentHelper.GetContent<Texture2D>("Title");
            loveTexture = this.contentHelper.GetContent<Texture2D>("LoveSpriteSheet");
            cursorTexture = this.contentHelper.GetContent<Texture2D>("CursorSpriteSheet");
        }

        private void LoadSounds()
        {
            finishLevelSong = this.contentHelper.GetContent<Song>("FinishLevel");
            theEndSong = this.contentHelper.GetContent<Song>("TheEnd");
            clockSoundEffectInstance = this.contentHelper.GetSoundEffectInstance("Clock");
        }

        public override void Update(GameTime gameTime)
        {
            switch (loadingState)
            {
                case LoadingState.Fonts:
                    LoadFonts();
                    loadingState = LoadingState.Textures;
                    break;
                case LoadingState.Textures:
                    LoadTextures();
                    loadingState = LoadingState.Sounds;
                    break;
                case LoadingState.Sounds:
                    LoadSounds();
                    loadingState = LoadingState.Completed;
                    break;
                case LoadingState.Completed:
                    NewMessenger.Default.Send<ViewStateChangedMessage>(new ViewStateChangedMessage { ViewState = ViewState.Menu });
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch(loadingState)
            {
                case LoadingState.Fonts:
                    DrawStringCentralized(spriteBatch, "Loading fonts...");
                    break;
                case LoadingState.Sounds:
                    DrawStringCentralized(spriteBatch, "Loading sounds...");
                    break;
                case LoadingState.Textures:
                    DrawStringCentralized(spriteBatch, "Loading sprites...");
                    break;
                case LoadingState.Completed:
                    DrawStringCentralized(spriteBatch, "completed...");
                    break;
            }
        }
    }
}
