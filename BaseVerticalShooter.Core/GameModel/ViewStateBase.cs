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
    public enum ViewState
    {
        Menu,
        Intro,
        Playing,
        ShowLevel,
        PassedLevel,
        GameOver,
        TheEnd
    }

    public class InputCodes
    {
        public const char Empty = ' ';
        public const char B = 'B';
        public const char A = 'A';
        public const char X = 'X';
        public const char U = 'U';
        public const char D = 'D';
    }

    public abstract class ViewStateBase
    {
        protected ICamera2d camera2d;
        protected GraphicsDeviceManager graphics;
        protected ContentManager content;
        protected IScreenPad screenPad;
        protected int levelNumber;
        protected string levelName;

        protected Vector2 screenSize = new Vector2(800f, 480f);
        protected Vector2 gameScreenSize = new Vector2(512, 480);
        protected Vector2 windowTilesSize = new Vector2(32, 30);
        protected SpriteFont font;
        protected int tickInMS = 125;
        protected int tickCount = 0;
        protected int tileWidth = 16;

        protected int score = 0;
        protected int hiScore = 0;
        protected int rest = 2;
        protected int stage = 1;
        protected float lastScrollRows = -1;
        protected bool showLove = false;

        protected TimeSpan accumElapsedGameTime = TimeSpan.FromSeconds(0);
        protected int collisionMS = 5;
        protected TimeSpan accumCollisionTime = TimeSpan.FromSeconds(0);
        protected int soundCheckMS = 50;
        protected int blinkCount = 0;
        protected bool blinkStart = false;
        protected Song currentSong;

        protected Song finishLevelSong;
        protected Song theEndSong;
        protected EventHandler<EventArgs> lastMediaStateChanged;
        protected SoundEffectInstance clockSoundEffectInstance;
        protected Princess princess;
        protected PowerUp currentPowerUp;
        protected List<BaseBullet> playerBullets = new List<BaseBullet>();
        protected List<PhysicalObject> bonuses = new List<PhysicalObject>();
        protected List<Weapon> weapons = new List<Weapon>();
        protected List<PowerUp> powerUps = new List<PowerUp>();
        protected List<PhysicalObject> onScreenBonuses = new List<PhysicalObject>();
        protected List<Weapon> onScreenWeapons = new List<Weapon>();
        protected List<PowerUp> onScreenPowerUps = new List<PowerUp>();
        protected List<IEnemy> enemies = new List<IEnemy>();
        protected Dictionary<int, int> enemyGroupCount = new Dictionary<int, int>();
        protected List<IEnemy> onScreenEnemies = new List<IEnemy>();
        public Vector2 topLeftCorner;
        protected Texture2D titleTexture;
        protected Texture2D cursorTexture;
        protected Texture2D loveTexture;
        
        public bool levelFinished = false;
        protected FrameCounter _frameCounter = new FrameCounter();
        protected IJsonMapManager jsonMapManager;
        protected MenuItems currentMenuItem = MenuItems.Start;
        protected IContentHelper contentHelper;

        protected IMap gameMap;

        public ViewStateBase(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
        {
            this.content = content;
            this.graphics = graphics;
            this.content = content;
            this.screenPad = BaseResolver.Instance.Resolve<IScreenPad>();
            this.levelNumber = levelNumber;
            this.levelName = levelName;
            this.topLeftCorner = new Vector2((screenSize.X - gameScreenSize.X) / 2, (screenSize.Y - gameScreenSize.Y) / 2);
            this.jsonMapManager = jsonMapManager;
            camera2d = BaseResolver.Instance.Resolve<ICamera2d>();
            DefineActions();
        }

        public virtual void RegisterActions()
        {
        }

        public virtual void UnregisterActions()
        {
        }

        protected virtual void DefineActions()
        {
        }

        protected void PlaySong(Song song, EventHandler<EventArgs> mediaStateChanged, bool loop = false)
        {
            MediaPlayer.Stop();
            if (lastMediaStateChanged != null)
                MediaPlayer.MediaStateChanged -= lastMediaStateChanged;

            currentSong = null;
            lastMediaStateChanged = mediaStateChanged;
            MediaPlayer.IsRepeating = loop;
            currentSong = song;
            MediaPlayer.Play(song);
            if (mediaStateChanged != null)
                MediaPlayer.MediaStateChanged += mediaStateChanged;
        }

        protected void StopSong(Action afterSongStopped = null)
        {
            currentSong = null;
            if (lastMediaStateChanged != null)
                MediaPlayer.MediaStateChanged -= lastMediaStateChanged;

            lastMediaStateChanged = null;

            if (afterSongStopped != null)
                afterSongStopped();

            //level1Song.Stop();
            MediaPlayer.Stop();
        }

        public virtual void LoadContent(IContentHelper contentHelper)
        {
            this.contentHelper = contentHelper;
            finishLevelSong = contentHelper.GetContent<Song>("FinishLevel");
            theEndSong = contentHelper.GetContent<Song>("TheEnd");
            font = contentHelper.GetContent<SpriteFont>("Super-Contra-NES");
            titleTexture = contentHelper.GetContent<Texture2D>("Title");
            loveTexture = contentHelper.GetContent<Texture2D>("LoveSpriteSheet");
            cursorTexture = contentHelper.GetContent<Texture2D>("CursorSpriteSheet");
            clockSoundEffectInstance = contentHelper.GetSoundEffectInstance("Clock");

            //princess = new Princess();
            //princess.LoadContent(contentHelper);

            InitializeLevel();
        }

        public virtual void UnLoadContent()
        {
        }

        public virtual void InitializeLevel()
        {
            showLove = false;
            MediaPlayer.Volume = 1f;
            tickCount = 0;
            score = 0;
            playerBullets.Clear();
            onScreenBonuses.Clear();
            onScreenWeapons.Clear();
            onScreenPowerUps.Clear();
            onScreenEnemies.Clear();
        }

        public virtual void Update(GameTime gameTime)
        {
            if (levelFinished)
                return;
        }

        protected void DrawStringCentralized(SpriteBatch spriteBatch, params string[] lines)
        {
            var offsetY = 0f;
            var maxTextLength = 0;
            foreach (var line in lines)
            {
                var text = line.Replace(">", " ");
                if (text.Length > maxTextLength)
                {
                    maxTextLength = text.Length;
                }
            }

            foreach (var line in lines)
            {
                var text = line.Replace(">", " ");
                var position = topLeftCorner + new Vector2((GameSettings.Instance.WindowTilesSize.X - maxTextLength) / 2
                    , (GameSettings.Instance.WindowTilesSize.Y - lines.Length) / 2) * tileWidth + new Vector2(0, offsetY);

                for (var y = -2; y <= 2; y++)
                {
                    for (var x = -2; x <= 2; x++)
                    {
                        spriteBatch.DrawString(font, string.Format(text), position + new Vector2(x, y), Color.Black);
                    }
                }
                if (line.StartsWith(">"))
                {
                    spriteBatch.Draw(cursorTexture, position - new Vector2(tileWidth * 1.5f, tileWidth * .75f), new Rectangle(0, 0, tileWidth * 2, tileWidth * 2), Color.White);
                }
                spriteBatch.DrawString(font, string.Format(text), position, Color.White);

                offsetY += 32;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (levelFinished)
                return;
        }

        protected void DrawScreenPad(SpriteBatch spriteBatch, GameTime gameTime)
        {
            screenPad.Draw(gameTime, spriteBatch);
        }
    }

    public class ViewStateChangedMessage { public ViewState ViewState { get; set; } }

}
