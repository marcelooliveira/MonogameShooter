using Shooter.GameModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ScreenControlsSample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xInput = Microsoft.Xna.Framework.Input;
using BaseVerticalShooter;
using BaseVerticalShooter.Input;
using System.Text.RegularExpressions;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Core;
using BaseVerticalShooter.Core.GameModel;
using Shooter;

namespace BaseVerticalShooter.Core.GameModel
{
    public class View
    {
        Dictionary<ViewState, ViewStateBase> viewStatesDic = new Dictionary<ViewState, ViewStateBase>();
        ViewStateBase currentViewState = null;

        public View(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
        {
            viewStatesDic.Add(ViewState.Intro, new ViewStateIntro(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager));
            viewStatesDic.Add(ViewState.Menu, new ViewStateMenu(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager));
            viewStatesDic.Add(ViewState.ShowLevel, new ViewStateShowLevel(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager));
            viewStatesDic.Add(ViewState.Playing, new ViewStatePlaying(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager));
            viewStatesDic.Add(ViewState.TheEnd, new ViewStateTheEnd(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager));
            viewStatesDic.Add(ViewState.GameOver, new ViewStateGameOver(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager));

            currentViewState = viewStatesDic[ViewState.Menu];

            NewMessenger.Default.Register<ViewStateChangedMessage>(this, (message) =>
            {
                foreach (var viewState in viewStatesDic.Values)
                {
                    viewState.UnregisterActions();
                }

                currentViewState = viewStatesDic[message.ViewState];
                currentViewState.RegisterActions();

                if (message.ViewState == ViewState.Intro)
                {
                    currentViewState.InitializeLevel();
                }
            });
        }

        public void Update(GameTime gameTime)
        {
            if (currentViewState.levelFinished)
                return;

            currentViewState.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (currentViewState.levelFinished)
                return;

            currentViewState.Draw(spriteBatch, gameTime);
        }

        public void UnregisterActions() 
        {
            foreach (var viewState in viewStatesDic.Values)
            {
                viewState.UnregisterActions();
            }
        }

        public void UnLoadContent() 
        {
            foreach (var viewState in viewStatesDic.Values)
            {
                viewState.UnLoadContent();
            }
        }

        public void LoadContent(IContentHelper contentHelper) 
        {
            contentHelper = ContentHelper.Instance;
            foreach (var viewState in viewStatesDic.Values)
            {
                viewState.LoadContent(contentHelper);
            }

            contentHelper.GetContent<Song>("FinishLevel");
            contentHelper.GetContent<Song>("TheEnd");
            contentHelper.GetContent<SpriteFont>("Shooter");
            contentHelper.GetContent<Texture2D>("Title");
            contentHelper.GetContent<Texture2D>("LoveSpriteSheet");
            contentHelper.GetContent<Texture2D>("CursorSpriteSheet");
            contentHelper.GetContent<Texture2D>("Boss1SpriteSheet");
            contentHelper.GetContent<Texture2D>("Boss1HitSpriteSheet");
            contentHelper.GetContent<Texture2D>("BossDestructionSpriteSheet");
            contentHelper.GetContent<Texture2D>("BonusSpriteSheet");
            contentHelper.GetContent<Texture2D>("EnemySpriteSheet");
            contentHelper.GetContent<Texture2D>("DestructionSpriteSheet");
            contentHelper.GetContent<Texture2D>("ComboSpriteSheet");
            contentHelper.GetContent<Texture2D>("EnemySpriteSheet");
            contentHelper.GetContent<Texture2D>("EnemyBullet3SpriteSheet");
            contentHelper.GetContent<Texture2D>("EnemyBulletSpriteSheet");
            contentHelper.GetContent<Texture2D>("PlayerBullet1SpriteSheet");
            contentHelper.GetContent<Texture2D>("EnemyBullet2SpriteSheet");
            contentHelper.GetContent<Texture2D>("PlayerBullet1SpriteSheet");
            //contentHelper.GetSoundEffectInstance("PlayerBullet1Shooting");
            contentHelper.GetSoundEffectInstance("Fire");
            contentHelper.GetSoundEffectInstance("Clock");
        }
        
        public void RegisterActions() 
        {
            //NewMessenger.Default.Register<ViewStateChangedMessage>(this, (message) =>
            //    {
            //        foreach (var viewState in viewStatesDic.Values)
            //        {
            //            viewState.UnregisterActions();
            //        }

            //        currentViewState = viewStatesDic[message.ViewState];
            //        currentViewState.RegisterActions();
            //    });

            //foreach (var viewState in viewStatesDic.Values)
            //{
            //    viewState.RegisterActions();
            //}
        }
    }

    public class PassedLevelMessage { public int LevelPassed { get; set; } }

    public class ComboMessage { public IEnemy Enemy { get; set; } }

    public enum Points
    {
        Minimum = 10,
        BossHit = 15,
        Combo = 50
    }
}
