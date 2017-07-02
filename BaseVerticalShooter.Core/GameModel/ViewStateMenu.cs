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
    public enum MenuItems
    {
        None = 0,
        Start = 1,
        ReviewApp = 2
    }

    public class ViewStateMenu : ViewStateBase
    {
        Song startSong;
        SoundEffectInstance clickSoundEffectInstance;
        TimeSpan accumStartBlinkingTime = TimeSpan.FromSeconds(0);

        public ViewStateMenu(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
            :base(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager)
        {

        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            base.LoadContent(contentHelper);
            startSong = contentHelper.GetContent<Song>("Start");
            clickSoundEffectInstance = contentHelper.GetSoundEffectInstance("Click");
        }

        public override void Update(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {
                var inputCode = InputManager.Instance.GetInputCode(screenPad, blinkCount);
                switch (inputCode)
                {
                    case InputCodes.X:
                        OnStartButtonClick();
                        break;
                    case InputCodes.U:
                        OnUpButtonClick();
                        break;
                    case InputCodes.D:
                        OnDownButtonClick();
                        break;
                }
            }
            UpdateBlinking(gameTime);
        }

        void UpdateBlinking(GameTime gameTime)
        {
            if (accumStartBlinkingTime.TotalMilliseconds > soundCheckMS * 2)
            {
                accumStartBlinkingTime = TimeSpan.FromMilliseconds(0);
                blinkStart = !blinkStart;
                blinkCount++;
            }
            else
            {
                accumStartBlinkingTime = accumStartBlinkingTime.Add(gameTime.ElapsedGameTime);
            }
        }

        void OnDownButtonClick()
        {
            var index = (int)currentMenuItem + 1;
            if (index == (int)MenuItems.ReviewApp + 1)
                index = (int)MenuItems.ReviewApp;

            SetNewMenuItem(index);
        }

        void OnUpButtonClick()
        {
            var index = (int)currentMenuItem - 1;
            if (index == (int)MenuItems.None)
                index = (int)MenuItems.Start;

            SetNewMenuItem(index);
        }

        void SetNewMenuItem(int index)
        {
            var newMenuItem = (MenuItems)index;
            if (currentMenuItem != newMenuItem)
                clickSoundEffectInstance.Play();

            currentMenuItem = newMenuItem;
        }

        void OnStartButtonClick()
        {
            PlaySong(startSong, (s, e) =>
            {
                if (currentSong == startSong)
                {
                    AfterStartSong();
                }
            });
        }

        private void AfterStartSong()
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {
                StopSong();
                switch (currentMenuItem)
                {
                    case MenuItems.Start:
                        StartGame();
                        break;
                    case MenuItems.ReviewApp:
                        ReviewApp();
                        break;
                }
            }
        }

        private void ReviewApp()
        {
            blinkStart = false;
            currentMenuItem = MenuItems.Start;
            var reviewHelper = BaseResolver.Instance.Resolve<IReviewHelper>();
            reviewHelper.MarketPlaceReviewTask();
        }

        private void StartGame()
        {
            blinkStart = false;
            NewMessenger.Default.Send<ViewStateChangedMessage>(new ViewStateChangedMessage { ViewState = ViewState.Intro });
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(titleTexture, topLeftCorner, Color.White);
            var itemSelected = currentSong == startSong;

            DrawStringCentralized(spriteBatch, "",
                (currentMenuItem == MenuItems.Start ? ">" : " ") +
                    ((itemSelected && currentMenuItem == MenuItems.Start && blinkStart) ? "" : "START GAME"),
                (currentMenuItem == MenuItems.ReviewApp ? ">" : " ") +
                    ((itemSelected && currentMenuItem == MenuItems.ReviewApp && blinkStart) ? "" : "REVIEW APP"));
        }

    }
}
