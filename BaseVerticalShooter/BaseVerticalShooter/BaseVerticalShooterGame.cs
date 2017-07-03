using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ScreenControlsSample;
using Shooter.GameModel;
using Shooter;
using System;
using xInput = Microsoft.Xna.Framework.Input;
using Windows.UI.Xaml;
using Autofac;
using BaseVerticalShooter.Input;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Core;
using Windows.Storage;
using BaseVerticalShooter.JsonModels;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using BaseVerticalShooter.Core.GameModel;
namespace BaseVerticalShooter
{
    public class BaseGame : Game
    {
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BaseVerticalShooterGame : BaseGame, IBaseVerticalShooterGame
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenPad screenPad;
        View currentView;
        int levelIndex = -1;
        Texture2D gameFrameTexture;
        bool buttonBPressed = false;
        ICamera2d camera2d;

        protected BossMovement[] bossMovements = new BossMovement[] { 
                BossMovement.WalkHorizontal,
                BossMovement.FloatSenoidHorizontal,
                BossMovement.WalkHorizontal,
                BossMovement.FloatSenoidHorizontal,
                BossMovement.WalkHorizontal,
                BossMovement.WalkHorizontal,
                BossMovement.WalkHorizontal,
                BossMovement.WalkHorizontal
            };

        public BossMovement[] BossMovements
        {
            get { return bossMovements; }
            set { bossMovements = value; }
        }

        public BaseVerticalShooterGame()
        {
#if WINDOWS_APP || WINDOWS_UAP
            GameSettings.Instance.PlatformType = PlatformType.Windows;
#endif

#if WINDOWS_PHONE_APP
            GameSettings.Instance.PlatformType = PlatformType.WindowsPhone;
#endif

            BaseResolver.Instance.RegisterAll();
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
            graphics.PreferredBackBufferHeight = 480;
            graphics.PreferredBackBufferWidth = 800;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            ContentHelper.Setup(Content);

            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>
                (graphics_PreparingDeviceSettings);

            // Frame rate is 60 fps
            TargetElapsedTime = TimeSpan.FromTicks(166667);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            NewMessenger.Default.Register<PassedLevelMessage>(this, (message) =>
            {
                if (message.LevelPassed == 8)
                    levelIndex = -1;

                PassLevel();
            });
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = PresentInterval.One;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ConfigureScreenPad();

            PassLevel();

            camera2d = Resolver.Instance.Resolve<ICamera2d>();
        }

        private void ConfigureScreenPad()
        {
            gameFrameTexture = ContentHelper.Instance.GetContent<Texture2D>("GameFrame");
            screenPad = new ScreenPad
            (
                this,
                Content
            );

            Resolver.Instance.RegisterGame(Content, screenPad);
        }

        private void PassLevel()
        {
            levelIndex++;

            string[] levelNames;

            levelNames = GameSettings.Instance.GetLevelNames();

            if (currentView != null)
            {
                currentView.UnregisterActions();
                currentView.UnLoadContent();

                Content.RootDirectory = "Content";
                ContentHelper.Setup(Content);
            }

            IJsonMapManager jsonMapManager = new JsonMapManager();
            currentView = new View(graphics, Content, screenPad, bossMovements[levelIndex], levelIndex + 1, levelNames[levelIndex], jsonMapManager);
            //IContentHelper contentHelper;
            currentView.LoadContent(null);
            
            currentView.RegisterActions();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (xInput.GamePad.GetState(PlayerIndex.One).Buttons.Back == xInput.ButtonState.Pressed)
                Application.Current.Exit();

#if WINDOWS_APP
            var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            //if (keyboardState.IsKeyUp())
#endif

#if WINDOWS_PHONE_APP
            if (screenPad.GetState().Buttons.B == ButtonState.Pressed)
            {
                buttonBPressed = true;
            }
            else if (screenPad.GetState().Buttons.B == ButtonState.Released && buttonBPressed)
            {
                buttonBPressed = false;
                var reviewHelper = BaseVerticalShooter.Resolver.Instance.Resolve<IReviewHelper>();
                reviewHelper.MarketPlaceReviewTask();
            }

            screenPad.Update();
#endif

            currentView.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera2d.GetTransformation(GraphicsDevice));
            currentView.Draw(spriteBatch, gameTime);
            spriteBatch.Draw(gameFrameTexture, new Vector2(0, 0), Color.White);
            spriteBatch.End();

#if WINDOWS_PHONE_APP
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            DrawScreenPad(spriteBatch, gameTime);
            spriteBatch.End();
#endif

            base.Draw(gameTime);
        }

        private void DrawScreenPad(SpriteBatch spriteBatch, GameTime gameTime)
        {
            screenPad.Draw(gameTime, spriteBatch);
        }

        private void DrawPlayer(Vector2 deviceScreenSize)
        {
        }
    }

    public class JsonMapManager : IJsonMapManager
    {
        public async Task<JsonMap> GetJsonMap(int levelNumber, JsonMap jsonMap)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(string.Format(@"ms-appx:///Content/Level{0:d2}.txt", levelNumber)));
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                var json = sRead.ReadToEnd();
                var fileMap = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonMap>(json);
                for (var i = 0; i < fileMap.layers.Count; i++)
                {
                    foreach (var tileInfo in fileMap.layers[i].tileIndexes)
                    {
                        jsonMap.layers[i].tileIndexes[tileInfo.i] = tileInfo;
                    }
                }
            }
            return jsonMap;
        }

        public async Task<List<string>> GetMapLinesAsync(int levelNumber)
        {
            var lines = new List<string>();
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(string.Format(@"ms-appx:///Content/Map{0:d2}.txt", levelNumber)));
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                var line = string.Empty;
                while ((line = sRead.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
    }
}
