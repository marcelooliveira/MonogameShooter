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
using BaseVerticalShooter.ScreenInput;
using System.Text.RegularExpressions;
using BaseVerticalShooter.GameModel;
using BaseVerticalShooter.Core;

namespace Shooter.GameModel
{
    public class View
    {
        enum ViewState
        {
            Menu,
            Intro,
            Playing,
            ShowLevel,
            PassedLevel,
            GameOver,
            TheEnd
        }

        ICamera2d camera2d;
        ViewState viewState = ViewState.Intro;
        GraphicsDeviceManager graphics;
        ContentManager content;
        IScreenPad screenPad;
        BossMovement bossMovement;
        int levelNumber;
        string levelName;
        bool buttonAPressed = false;
        bool buttonBPressed = false;
        bool buttonXPressed = false;
        bool buttonDownPressed = false;
        bool buttonUpPressed = false;

        Vector2 screenSize = new Vector2(800f, 480f);
        Vector2 gameScreenSize = new Vector2(512, 480);
        protected Vector2 windowTilesSize = new Vector2(32, 30);
        SpriteFont font;
        int tickInMS = 125;
        int tickCount = 0;
        int tileWidth = 16;

        int score = 0;
        int hiScore = 0;
        int rest = 2;
        int stage = 1;
        float lastScrollRows = -1;
        bool showLove = false;

        TimeSpan accumElapsedGameTime = TimeSpan.FromSeconds(0);
        int collisionMS = 5;
        TimeSpan accumCollisionTime = TimeSpan.FromSeconds(0);
        int soundCheckMS = 50;
        TimeSpan accumSoundCheckTime = TimeSpan.FromSeconds(0);
        TimeSpan accumStartBlinkingTime = TimeSpan.FromSeconds(0);
        bool blinkStart = false;
        int blinkCount = 0;
        Song currentSong;
        Song startSong;
        Song bossSong;
        Song introSong;
        Song dyingSong;
        Song gameOverSong;
        Song finishLevelSong;
        Song theEndSong;
        EventHandler<EventArgs> lastMediaStateChanged;
        SoundEffectInstance level1Song;
        SoundEffectInstance clockSoundEffectInstance;
        SoundEffectInstance clickSoundEffectInstance;
        SoundEffectInstance bonusSoundEffectInstance;
        SoundEffectInstance weaponSoundEffectInstance;
        SoundEffectInstance powerUpSoundEffectInstance;
        SoundState lastPowerUpSoundState = SoundState.Stopped;
        SoundEffectInstance comboSoundEffectInstance;
        SoundEffectInstance pauseSoundEffectInstance;
        SoundEffectInstance unpauseSoundEffectInstance;
        Map gameMap;
        IBasePlayer player;
        Princess princess;
        Boss boss;
        //BaseBullet bossBullet;
        Weapon currentWeapon;
        PowerUp currentPowerUp;
        List<BaseBullet> playerBullets = new List<BaseBullet>();
        List<PhysicalObject> bonuses = new List<PhysicalObject>();
        List<Weapon> weapons = new List<Weapon>();
        List<PowerUp> powerUps = new List<PowerUp>();
        List<PhysicalObject> onScreenBonuses = new List<PhysicalObject>();
        List<Weapon> onScreenWeapons = new List<Weapon>();
        List<PowerUp> onScreenPowerUps = new List<PowerUp>();
        List<IEnemy> enemies = new List<IEnemy>();
        Dictionary<int, int> enemyGroupCount = new Dictionary<int, int>();
        List<IEnemy> onScreenEnemies = new List<IEnemy>();
        public Vector2 topLeftCorner;
        Texture2D titleTexture;
        Texture2D cursorTexture;
        Texture2D loveTexture;
        Action<EnemyDeathMessage> EnemyDeathMessageAction;
        Action<PlayerDeathMessage> PlayerDeathMessageAction;
        Action<BossDeathMessage> BossDeathMessageAction;
        Action<BonusStateChangedMessage> BonusStateChangedMessageAction;
        Action<WeaponStateChangedMessage> WeaponStateChangedMessageAction;
        Action<ComboMessage> ComboMessageAction;
        Action<EnemyShotMessage> EnemyShotMessageAction;
        bool levelFinished = false;
        MenuItems menuItem = MenuItems.Start;
        private FrameCounter _frameCounter = new FrameCounter();

        public View(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName)
        {
            this.content = content;
            this.graphics = graphics;
            this.content = content;
            //this.screenPad = screenPad;
            this.screenPad = BaseVerticalShooter.Resolver.Instance.Resolve<IScreenPad>();
            this.bossMovement = bossMovement;
            this.levelNumber = levelNumber;
            this.levelName = levelName;
            this.topLeftCorner = new Vector2((screenSize.X - gameScreenSize.X) / 2, (screenSize.Y - gameScreenSize.Y) / 2);
            camera2d = BaseVerticalShooter.Resolver.Instance.Resolve<ICamera2d>();
            DefineActions();
        }

        public void RegisterActions()
        {
            NewMessenger.Default.Register<EnemyDeathMessage>(this, EnemyDeathMessageAction);
            NewMessenger.Default.Register<PlayerDeathMessage>(this, PlayerDeathMessageAction);
            NewMessenger.Default.Register<BossDeathMessage>(this, BossDeathMessageAction);
            NewMessenger.Default.Register<BonusStateChangedMessage>(this, BonusStateChangedMessageAction);
            NewMessenger.Default.Register<WeaponStateChangedMessage>(this, WeaponStateChangedMessageAction);
            NewMessenger.Default.Register<ComboMessage>(this, ComboMessageAction);
            NewMessenger.Default.Register<EnemyShotMessage>(this, EnemyShotMessageAction);
        }

        public void UnregisterActions()
        {
            NewMessenger.Default.Unregister<EnemyDeathMessage>(this, EnemyDeathMessageAction);
            NewMessenger.Default.Unregister<PlayerDeathMessage>(this, PlayerDeathMessageAction);
            NewMessenger.Default.Unregister<BossDeathMessage>(this, BossDeathMessageAction);
            NewMessenger.Default.Unregister<BonusStateChangedMessage>(this, BonusStateChangedMessageAction);
            NewMessenger.Default.Unregister<WeaponStateChangedMessage>(this, WeaponStateChangedMessageAction);
            NewMessenger.Default.Unregister<ComboMessage>(this, ComboMessageAction);
            NewMessenger.Default.Unregister<EnemyShotMessage>(this, EnemyShotMessageAction);
        }

        private void DefineActions()
        {
            EnemyDeathMessageAction = (message) =>
            {
                var wasCombo = false;
                var enemyGroupId = message.Enemy.GroupId;
                if (enemyGroupCount.ContainsKey(enemyGroupId))
                {
                    enemyGroupCount[enemyGroupId]--;

                    if (enemyGroupCount[enemyGroupId] == 0)
                    {
                        message.Enemy.State = CharacterState.Combo;
                        comboSoundEffectInstance.Play();
                        AddToScore((int)Points.Combo);

                        wasCombo = true;
                        NewMessenger.Default.Send(new ComboMessage { Enemy = message.Enemy });
                    }
                }

                if (!wasCombo)
                {
                    onScreenEnemies.Remove(message.Enemy);
                    if (!message.Enemy.IsBullet)
                        enemies.Add(message.Enemy);
                }
            };

            PlayerDeathMessageAction = (message) =>
            {
                lastPowerUpSoundState = SoundState.Stopped;
                level1Song.Stop();
                bonusSoundEffectInstance.Stop();
                weaponSoundEffectInstance.Stop();
                powerUpSoundEffectInstance.Stop();
                comboSoundEffectInstance.Stop();
                pauseSoundEffectInstance.Stop();
                unpauseSoundEffectInstance.Stop();
                MediaPlayer.Stop();
                currentWeapon.State = WeaponState.Arrow;

                PlaySong(dyingSong, (s, e) =>
                {
                    if (currentSong == dyingSong)
                    {
                        if (MediaPlayer.State == MediaState.Stopped)
                        {
                            if (player.Lives <= 0)
                            {
                                viewState = ViewState.GameOver;
                            }
                            else
                            {
                                viewState = ViewState.ShowLevel;
                                RespawnPlayerAndEnemies();
                            }
                        }
                    }
                });
            };

            BossDeathMessageAction = (message) =>
            {
                //bossBullet.Position = new Vector2(-10, -10);
                if (this.levelNumber == 8)
                {
                    PlaySong(finishLevelSong, (s, e) =>
                    {
                        if (currentSong == finishLevelSong
                            && MediaPlayer.State == MediaState.Stopped)
                        {
                            viewState = ViewState.TheEnd;
                            PlaySong(theEndSong, (s2, e2) =>
                            {
                                if (currentSong == theEndSong
                                    && MediaPlayer.State == MediaState.Stopped)
                                {
                                    StopSong(() =>
                                        {
                                            blinkStart = false;
                                            menuItem = MenuItems.Start;
                                            viewState = ViewState.Menu;
                                            NewMessenger.Default.Send(new PassedLevelMessage { LevelPassed = this.levelNumber });
                                        });
                                }
                            });
                        }
                    });
                }
                else
                {
                    PlaySong(finishLevelSong, (s, e) =>
                    {
                        if (currentSong == finishLevelSong
                            && MediaPlayer.State == MediaState.Stopped)
                        {
                            viewState = ViewState.PassedLevel;
                        }
                    });

                    levelFinished = true;
                    NewMessenger.Default.Send(new PassedLevelMessage { LevelPassed = this.levelNumber });
                }
            };

            BonusStateChangedMessageAction = (message) =>
            {
                //onScreenBonuses.Remove(message.Bonus);
            };

            WeaponStateChangedMessageAction = (message) =>
            {
                //weaponState = message.Weapon.State;
            };

            ComboMessageAction = (message) =>
            {
            };

            EnemyShotMessageAction = (message) =>
            {
            };
        }

        private void RespawnPlayerAndEnemies()
        {
            gameMap.RestoreScrollStartRow();
            foreach (var enemy in onScreenEnemies.Where(enemy => !enemy.IsBullet).ToList())
            {
                enemy.RestorePosition();
                enemies.Add(enemy);
            }

            foreach (var enemy in enemies)
            {
                enemy.State = CharacterState.Alive;
                enemy.RestorePosition();
            }
            onScreenEnemies.Clear();
            //bossBullet.Position = new Vector2(-10, -10);
            player.Respawn();
        }

        private void PlaySong(Song song, EventHandler<EventArgs> mediaStateChanged, bool loop = false)
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

        private void StopSong(Action afterSongStopped = null)
        {
            currentSong = null;
            if (lastMediaStateChanged != null)
                MediaPlayer.MediaStateChanged -= lastMediaStateChanged;

            lastMediaStateChanged = null;

            if (afterSongStopped != null)
                afterSongStopped();

            level1Song.Stop();
            MediaPlayer.Stop();
        }

        public void LoadContent(ContentManager content)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            startSong = ContentHelper.Instance.GetContent<Song>("Start");
            introSong = ContentHelper.Instance.GetContent<Song>("Intro");
            bossSong = ContentHelper.Instance.GetContent<Song>("BossTheme");
            dyingSong = ContentHelper.Instance.GetContent<Song>("Death");
            gameOverSong = ContentHelper.Instance.GetContent<Song>("GameOver");
            finishLevelSong = ContentHelper.Instance.GetContent<Song>("FinishLevel");
            theEndSong = ContentHelper.Instance.GetContent<Song>("TheEnd");
            font = ContentHelper.Instance.GetContent<SpriteFont>("Super-Contra-NES");
            titleTexture = ContentHelper.Instance.GetContent<Texture2D>("Title");
            loveTexture = ContentHelper.Instance.GetContent<Texture2D>("LoveSpriteSheet");
            cursorTexture = ContentHelper.Instance.GetContent<Texture2D>("CursorSpriteSheet");
            level1Song = ContentHelper.Instance.GetSoundEffectInstance("Level1");
            clockSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Clock");
            clickSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Click");
            bonusSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Bonus");
            weaponSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Weapon");
            powerUpSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("PowerUp");
            comboSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Combo");
            pauseSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("pause");
            unpauseSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("unpause");

            gameMap = new Map(levelNumber);
            gameMap.LoadContent(content);
            gameMap.Scrolled += gameMap_Scrolled;
            player = new Player();
            player.LoadContent(content);
            princess = new Princess();
            princess.LoadContent(content);
            boss = new Boss(windowTilesSize, levelNumber, bossMovement);
            boss.LoadContent(content);
            //bossBullet = new BossBullet(1);
            //bossBullet.LoadContent(content);
            //bossBullet.Position = boss.Position + (boss.Size) / 2 - new Vector2(0, gameMap.ScrollRows);
            //bossBullet.OffScreen += (sb, eb) =>
            //{
            //    if (boss.State == PlayerState.Alive)
            //    {
            //        bossBullet.Position = boss.Position + (boss.Size) / 2 - new Vector2(0, gameMap.ScrollRows);
            //        var newDirection = new Vector2(player.Position.X - bossBullet.Position.X, player.Position.Y - bossBullet.Position.Y) + player.Size / 2;
            //        newDirection.Normalize();
            //        bossBullet.Direction = newDirection;
            //        if (!bossBullet.IsOffScreen())
            //            bossBullet.Shoot();
            //    }
            //};

            currentWeapon = new Weapon(new Vector2(-15, -15), player);

            InitializeLevel();
        }

        public void UnLoadContent()
        {
            gameMap.UnLoadContent();
        }

        private void InitializeLevel()
        {
            showLove = false;
            MediaPlayer.Volume = 1f;
            tickCount = 0;
            player.Initialize();
            score = 0;
            //viewState = (levelNumber == 1 ? ViewState.Menu : ViewState.Intro);
            gameMap.Initialize();
            playerBullets.Clear();
            onScreenBonuses.Clear();
            onScreenWeapons.Clear();
            onScreenPowerUps.Clear();
            onScreenEnemies.Clear();
            InitializeEnemies();
        }

        private async void InitializeEnemies()
        {
            enemies.Clear();
            var mapLines = await gameMap.GetMapLinesAsync();
            var jsonMap = await gameMap.GetLayersAsync();
            var y = 0;
            foreach (var line in mapLines)
            {
                var x = 0;
                foreach (var c in line)
                {
                    var pos = new Vector2(x * 2, y * 2);

                    if ("ABCDE".Contains(c))
                    {
                        var bonus = new Bonus(pos, c);
                        bonus.LoadContent(content);
                        bonuses.Add(bonus);
                    }
                    else if (c == 'W')
                    {
                        var weapon = new Weapon(pos, player);
                        weapon.LoadContent(content);
                        weapons.Add(weapon);
                    }
                    else if (c == 'P')
                    {
                        //var powerUp = new PowerUp(pos, player);
                        //powerUps.Add(powerUp);
                    }
                    //else if (Regex.IsMatch(c.ToString(), @"[a-z]"))
                    //{
                    //    var enemyGroupId = GetEnemyGroupId(y, x, c.ToString());
                    //    var enemy = Resolver.Instance.ResolveEnemy((int)c, pos, enemyGroupId);
                    //    enemy.LoadContent(content);
                    //    enemies.Add(enemy);
                    //}

                    x++;
                }
                y++;
            }

            var index = 0;
            foreach(var tileInfo in jsonMap.layers[4].tileIndexes)
            {
                tileInfo.x = index % 16;
                tileInfo.y = (int)(index / 16);
                index++;
                if (tileInfo.ti > 0)
                {
                    var pos = new Vector2(tileInfo.x * 2, tileInfo.y * 2);
                    var enemyCode = Convert.ToChar(((int)'a') + tileInfo.ti).ToString();
                    var enemyGroupId = GetEnemyGroupId(tileInfo.y, tileInfo.x, enemyCode);
                    var enemy = BaseVerticalShooter.Resolver.Instance.ResolveEnemy(tileInfo.ti, pos, enemyGroupId);
                    enemy.LoadContent(content);
                    enemies.Add(enemy);
                }
            }

            var groupsToRemoveIdList = new List<int>();
            foreach (var enemyGroup in enemyGroupCount)
            {
                if (enemyGroup.Value < 3)
                {
                    groupsToRemoveIdList.Add(enemyGroup.Key);
                }
            }

            foreach (var id in groupsToRemoveIdList)
            {
                enemyGroupCount.Remove(id);
                foreach (var enemy in enemies.Where(e => e.GroupId == id))
                {
                    enemy.GroupId = 0;
                }
            }
            viewState = (levelNumber == 1 ? ViewState.Menu : ViewState.Intro);
        }

        private int GetEnemyGroupId(int y, int x, string enemyCode)
        {
            var enemyGroupId = 0;
            var selectedEnemies =
                enemies.Where(e =>
                (
                    (int)e.Position.Y == (y - 1) * 2
                    && (
                        ((int)e.Position.X >= (x - 1) * 2)
                        && ((int)e.Position.X <= (x + 1) * 2)
                        )
                ) ||
                (
                    (int)e.Position.Y == y * 2
                    && ((int)e.Position.X == (x - 1) * 2)
                )
                ).ToList();

            foreach (var e in selectedEnemies)
            {
                if (enemyGroupId == 0 && e.Code == enemyCode)
                {
                    enemyGroupId = e.GroupId;
                }
            }

            if (enemyGroupId == 0)
                enemyGroupId = enemyGroupCount.LastOrDefault().Key + 1;

            if (enemyGroupCount.ContainsKey(enemyGroupId))
            {
                enemyGroupCount[enemyGroupId]++;
            }
            else
            {
                enemyGroupCount.Add(enemyGroupId, 1);
            }

            return enemyGroupId;
        }

        void gameMap_Scrolled(object sender, EventArgs e)
        {
            if (clockSoundEffectInstance.State == SoundState.Playing)
            {
                clockSoundEffectInstance.Stop();
            }

            var collisionResult = gameMap.TestCollision(player, player.Position, gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                player.Position = player.Position + new Vector2(0, .125f);

            collisionResult = gameMap.TestCollision(player, player.Position, gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType != CollisionType.None)
            {
                player.ProcessDeath(tickCount);
            }

            foreach (var enemy in onScreenEnemies)
            {
                if (!enemy.IsBullet && !enemy.IsFlying && enemy.State == CharacterState.Alive)
                {
                    enemy.OnWindowPosition += new Vector2(0, .125f) * tileWidth;
                    enemy.Position += new Vector2(0, .125f);
                }
            }

            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];

                if (enemy.Position.Y == gameMap.ScrollRows - (enemy.IsAnchored ? enemy.Size.Y : 0))
                {
                    if (enemies.Contains(enemy))
                    {
                        enemies.Remove(enemy);
                        onScreenEnemies.Add(enemy);
                    }
                }
            }

            for (var i = onScreenEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = onScreenEnemies[i];
                if (!enemy.IsBullet && enemy.OnWindowPosition.HasValue && !enemy.IsFlying && !enemy.IsPassingBy)
                {
                    if (enemy.OnWindowPosition.Value.Y > 0)
                    {
                        var enemyPosition = (enemy.OnWindowPosition.Value / tileWidth);
                        var mapCollision = gameMap.TestCollision(enemy, enemyPosition, gameMap.ScrollRows);
                        if (mapCollision.CollisionType == CollisionType.Blocked)
                        {
                            enemy.ProcessDeath(tickCount);
                        }
                    }
                }
            }

            for (var i = bonuses.Count - 1; i >= 0; i--)
            {
                var bonus = bonuses[i];
                if (bonus.Position.Y + bonus.Size.Y >= gameMap.ScrollRows)
                {
                    bonuses.Remove(bonus);
                    onScreenBonuses.Add(bonus);
                }
            }

            for (var i = weapons.Count - 1; i >= 0; i--)
            {
                var weapon = weapons[i];
                if (weapon.Position.Y + weapon.Size.Y >= gameMap.ScrollRows - GameSettings.Instance.WindowTilesSize.Y)
                {
                    weapons.Remove(weapon);
                    onScreenWeapons.Add(weapon);
                }
            }

            for (var i = powerUps.Count - 1; i >= 0; i--)
            {
                var powerUp = powerUps[i];
                if (powerUp.Position.Y + powerUp.Size.Y >= gameMap.ScrollRows - GameSettings.Instance.WindowTilesSize.Y)
                {
                    powerUps.Remove(powerUp);
                    onScreenPowerUps.Add(powerUp);
                }
            }

            if (gameMap.ScrollRows == GameSettings.Instance.WindowTilesSize.Y && lastScrollRows != gameMap.ScrollRows && viewState == ViewState.Playing)
            {
                MediaPlayer.Volume = 1f;
                level1Song.Stop();
                PlaySong(bossSong, null, true);
            }

            lastScrollRows = gameMap.ScrollRows;
        }

        public void Update(GameTime gameTime)
        {
            if (levelFinished)
                return;

            switch (viewState)
            {
                case ViewState.Menu:
                    UpdateViewStateMenu(gameTime);
                    break;
                case ViewState.Intro:
                    UpdateViewStateIntro(gameTime);
                    break;
                case ViewState.ShowLevel:
                    UpdateViewStateShowLevel(gameTime);
                    break;
                case ViewState.Playing:
                    UpdateViewStatePlaying(gameTime);
                    break;
                case ViewState.GameOver:
                    UpdateViewStateGameOver(gameTime);
                    break;
                case ViewState.TheEnd:
                    UpdateViewStateTheEnd(gameTime);
                    break;
            }
        }

        void DrawStringCentralized(SpriteBatch spriteBatch, params string[] lines)
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

        void UpdateViewStateMenu(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped && currentSong != startSong)
            {
                var inputCode = GetInputCode();
                switch (inputCode)
                {
                    case 'X':
                        PlaySong(startSong, (s, e) =>
                        {
                            if (currentSong == startSong)
                            {
                                if (MediaPlayer.State == MediaState.Stopped)
                                {
                                    StopSong();
                                    switch (menuItem)
                                    {
                                        case MenuItems.Start:
                                            blinkStart = false;
                                            viewState = ViewState.Intro;
                                            break;
                                        case MenuItems.ReviewApp:
                                            blinkStart = false;
                                            menuItem = MenuItems.Start;
                                            var reviewHelper = BaseVerticalShooter.Resolver.Instance.Resolve<IReviewHelper>();
                                            reviewHelper.MarketPlaceReviewTask();
                                            break;
                                    }
                                }
                            }
                        });
                        break;
                    case 'U':
                        var i = (int)menuItem - 1;
                        if (i == 0)
                            i = 1;

                        var newMenuItem = (MenuItems)i;
                        if (menuItem != newMenuItem)
                        {
                            clickSoundEffectInstance.Play();
                        }

                        menuItem = newMenuItem;
                        break;
                    case 'D':
                        var i2 = (int)menuItem + 1;
                        if (i2 == 3)
                            i2 = 2;

                        var newMenuItem2 = (MenuItems)i2;
                        if (menuItem != newMenuItem2)
                        {
                            clickSoundEffectInstance.Play();
                        }

                        menuItem = newMenuItem2;
                        break;
                }
            }
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

        void UpdateViewStateIntro(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped && currentSong != introSong)
            {
                PlaySong(introSong, (s, e) =>
                {
                    if (currentSong == introSong)
                    {
                        if (MediaPlayer.State == MediaState.Stopped)
                        {
                            //camera2d.Zoom = 2;
                            viewState = ViewState.Playing;
                        }
                    }
                });
            }
        }

        void UpdateViewStateShowLevel(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Volume = 0;
                PlaySong(dyingSong, (s, e) =>
                {
                    if (currentSong == dyingSong && viewState == ViewState.ShowLevel)
                    {
                        if (MediaPlayer.State == MediaState.Stopped)
                        {
                            MediaPlayer.Volume = 1f;
                            //camera2d.Zoom = 2;
                            viewState = ViewState.Playing;
                        }
                    }
                });
            }

            //if (viewState == ViewState.ShowLevel && currentSong == GetLevelSong() && MediaPlayer.State == MediaState.Playing)
            //{
            //    MediaPlayer.Stop();
            //}
        }

        private void UpdateViewStateGameOver(GameTime gameTime)
        {
            if (MediaPlayer.State == MediaState.Stopped && currentSong != gameOverSong)
            {
                PlaySong(gameOverSong, (s, e) =>
                {
                    if (currentSong == gameOverSong && MediaPlayer.State == MediaState.Stopped)
                    {
                        InitializeLevel();
                    }
                });
            }
        }

        private void UpdateViewStatePlaying(GameTime gameTime)
        {
            var previousRemainingLives = player.Lives;

            if (MediaPlayer.State == MediaState.Stopped)
            {
                if (gameMap.ScrollRows < GameSettings.Instance.WindowTilesSize.Y)
                {
                    PlaySong(bossSong,
                            (s, e) =>
                            {
                                if (currentSong == bossSong 
                                    && previousRemainingLives == player.Lives 
                                    && viewState == ViewState.Playing 
                                    && boss.State == CharacterState.Alive)
                                {
                                    if (MediaPlayer.State == MediaState.Stopped && viewState == ViewState.Playing)
                                        PlaySong(bossSong, null, true);
                                }
                            }, true
                        );
                }
                else
                {
                    if (level1Song.State == SoundState.Stopped)
                    {
                        level1Song.IsLooped = true;
                        level1Song.Play();
                    }
                }
            }

            gameMap.Update(gameTime, tickCount, gameMap.ScrollRows);

            bool isPlayerBlocked = CheckCollisions(gameTime);

            player.Update(gameTime, tickCount, gameMap.ScrollRows);

            for (var i = 0; i < playerBullets.Count; i++)
            {
                var playerBullet = playerBullets[i];
                playerBullet.Update(gameTime, tickCount, gameMap.ScrollRows);
            }

            if (gameMap.State == MapState.Scrolling)
            {
                for (var i = 0; i < onScreenBonuses.Count; i++)
                {
                    var bonus = onScreenBonuses[i];
                    if (bonus.Position.Y > gameMap.ScrollRows + GameSettings.Instance.WindowTilesSize.Y)
                    {
                        onScreenBonuses.Remove(bonus);
                    }
                    else
                    {
                        bonus.Update(gameTime, tickCount, gameMap.ScrollRows);
                    }
                }

                for (var i = 0; i < onScreenWeapons.Count; i++)
                {
                    var weapon = onScreenWeapons[i];
                    weapon.Update(gameTime, tickCount, gameMap.ScrollRows);
                }

                for (var i = 0; i < onScreenPowerUps.Count; i++)
                {
                    var powerUp = onScreenPowerUps[i];
                    powerUp.Update(gameTime, tickCount, gameMap.ScrollRows);
                }

                boss.Update(gameTime, tickCount, gameMap.ScrollRows);
                //if (boss.State == PlayerState.Alive)
                //    bossBullet.Update(gameTime, tickCount, gameMap.ScrollRows);
            }

            for (var i = 0; i < onScreenEnemies.Count; i++)
            {
                var enemy = onScreenEnemies[i];

                if (enemy.Position.X < -windowTilesSize.X
                    || enemy.Position.X > GameSettings.Instance.WindowTilesSize.X * 2
                    || enemy.Position.Y > gameMap.ScrollRows + GameSettings.Instance.WindowTilesSize.Y)
                {
                     onScreenEnemies.Remove(enemy);
                    if (!enemy.IsBullet)
                        enemies.Add(enemy);
                }
                else
                {
                    enemy.UpdateDirection(player, gameMap);
                    enemy.Update(gameTime, tickCount, gameMap.ScrollRows, onScreenEnemies, gameMap);

                    if (enemy.OnWindowPosition.HasValue && enemy.OnWindowPosition.Value.Y > 0)
                    {
                        if (enemy.GroupId == 0 && !enemy.IsBullet && enemy.Reloaded)
                        {
                            var enemyBullet = enemy.GetBullet(player, gameMap);
                            enemyBullet.LoadContent(content);
                            onScreenEnemies.Add(enemyBullet);
                        }
                    }

                    foreach (var other in onScreenEnemies)
                    {
                        if (other.TestCollision(enemy, enemy.Position, gameMap.ScrollRows).CollisionType == CollisionType.Blocked)
                        {
                            //other.OnWindowPosition = other.OnWindowPosition + new Vector2(other.Size.X, 0);
                        }
                    }
                }
            }

            if (gameMap.ScrollRows <= GameSettings.Instance.WindowTilesSize.Y / 2
                && boss.State == CharacterState.Alive 
                && boss.Reloaded)
            {
                var bossBullet = boss.GetBullet(player, gameMap);
                bossBullet.LoadContent(content);
                onScreenEnemies.Add(bossBullet);
            }

            var inputCode = GetInputCode();

            if (inputCode == 'B')
            {
                buttonBPressed = false;
                var reviewHelper = BaseVerticalShooter.Resolver.Instance.Resolve<IReviewHelper>();
                reviewHelper.MarketPlaceReviewTask();
            }

            if (inputCode == 'A')
            {
                inputCode = ' ';
                buttonAPressed = false;
                if (gameMap.State == MapState.Paused)
                {
                    //MediaPlayer.Volume = 1;
                    level1Song.Play();
                    gameMap.State = MapState.Scrolling;
                    unpauseSoundEffectInstance.Play();
                }
                else
                {
                    //MediaPlayer.Volume = 0;
                    level1Song.Pause();
                    pauseSoundEffectInstance.Play();
                    gameMap.State = MapState.Paused;
                }
            }

            if (inputCode == 'X')
            {
                if (gameMap.State != MapState.Paused && player.CanShoot && player.State == CharacterState.Alive)
                {
                    List<PlayerBullet> weaponBullets = currentWeapon.Shoot();

                    foreach (var playerBullet in weaponBullets)
                    {
                        playerBullet.Position = 
                            player.Position 
                            + player.Size / 2
                            - playerBullet.Size / 2
                            + player.GetSpriteDirection();

                        playerBullet.OffScreen += (spb, epb) =>
                        {
                            playerBullets.Remove(playerBullet);
                        };
                        playerBullets.Add(playerBullet);
                        playerBullet.Shoot();
                    }
                }
                inputCode = ' ';
            }

            if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
            {
                accumElapsedGameTime = TimeSpan.FromSeconds(0);
                tickCount++;
            }
            accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);

            if (player.State == CharacterState.Alive)
            {
                if (!isPlayerBlocked && gameMap.State != MapState.Paused)
                    player.ProcessGamePad(screenPad, gameMap, gameMap.ScrollRows);
            }

            currentWeapon.Update(gameTime, tickCount, gameMap.ScrollRows);

            CheckSound(gameTime);
        }

        private char GetInputCode()
        {
            var inputCode = ' ';

#if WINDOWS_PHONE_APP
            if (screenPad.GetState().Buttons.B == ButtonState.Pressed)
            {
                buttonBPressed = true;
            }
            else if (screenPad.GetState().Buttons.B == ButtonState.Released && buttonBPressed)
            {
                buttonBPressed = false;
                inputCode = 'B';
            }
            if (screenPad.GetState().Buttons.A == ButtonState.Pressed)
            {
                buttonAPressed = true;
            }
            else if (screenPad.GetState().Buttons.A == ButtonState.Released && buttonAPressed)
            {
                buttonAPressed = false;
                inputCode = 'A';
            }
            if (screenPad.GetState().Buttons.X == ButtonState.Pressed)
            {
                buttonXPressed = true;
                inputCode = 'X';
            }
            else if (screenPad.GetState().Buttons.X == ButtonState.Released && buttonXPressed)
            {
                buttonXPressed = false;
            }

            if (screenPad.LeftStick.Y > .75f && (inputCode != 'U' || blinkCount % 4 == 0))
            {
                inputCode = 'U';
            }

            if (screenPad.LeftStick.Y < -.75f && (inputCode != 'D' || blinkCount % 4 == 0))
            {
                inputCode = 'D';
            }
#endif

#if WINDOWS_APP
            var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            if (keyboardState.IsKeyDown(xInput.Keys.A))
            {
                buttonAPressed = true;
            }
            else if (keyboardState.IsKeyUp(xInput.Keys.A) && buttonAPressed)
            {
                buttonAPressed = false;
                inputCode = 'A';
            }

            if (keyboardState.IsKeyDown(xInput.Keys.B))
            {
                buttonBPressed = true;
            }
            else if (keyboardState.IsKeyUp(xInput.Keys.B) && buttonBPressed)
            {
                buttonBPressed = false;
                inputCode = 'B';
            }

            if (keyboardState.IsKeyDown(xInput.Keys.Space))
            {
                buttonXPressed = true;
                inputCode = 'X';
            }

            if (keyboardState.IsKeyDown(xInput.Keys.Up))
            {
                buttonUpPressed = true;
            }
            else if (keyboardState.IsKeyUp(xInput.Keys.Up) && buttonUpPressed)
            {
                buttonUpPressed = false;
                inputCode = 'U';
            }

            if (keyboardState.IsKeyDown(xInput.Keys.Down))
            {
                buttonDownPressed = true;
            }
            else if (keyboardState.IsKeyUp(xInput.Keys.Down) && buttonDownPressed)
            {
                buttonDownPressed = false;
                inputCode = 'D';
            }
#endif

            return inputCode;
        }

        private void UpdateViewStateTheEnd(GameTime gameTime)
        {
            bool isPlayerBlocked = CheckCollisions(gameTime);

            player.Update(gameTime, tickCount, gameMap.ScrollRows);
            princess.Update(gameTime, tickCount, gameMap.ScrollRows);

            if (screenPad.GetState().Buttons.B == ButtonState.Pressed)
            {
                var reviewHelper = BaseVerticalShooter.Resolver.Instance.Resolve<IReviewHelper>();
                reviewHelper.MarketPlaceReviewTask();
            }
            else if (screenPad.GetState().Buttons.A == ButtonState.Pressed)
            {
                buttonAPressed = true;
            }
            else if (screenPad.GetState().Buttons.A == ButtonState.Released && buttonAPressed)
            {
                buttonAPressed = false;
                if (gameMap.State == MapState.Paused)
                {
                    MediaPlayer.Volume = 1;
                    gameMap.State = MapState.Scrolling;
                    unpauseSoundEffectInstance.Play();
                }
                else
                {
                    MediaPlayer.Volume = 0;
                    pauseSoundEffectInstance.Play();
                    gameMap.State = MapState.Paused;
                }
            }

            if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
            {
                accumElapsedGameTime = TimeSpan.FromSeconds(0);
                tickCount++;
            }
            accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);

            var awaitingPrincessPosition = new Vector2(GameSettings.Instance.WindowTilesSize.X / 2 - player.Size.X / 2, GameSettings.Instance.WindowTilesSize.Y / 2 + player.Size.Y / 2);
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

            CheckSound(gameTime);
        }

        //private Song GetLevelSong()
        //{
        //    Song levelSong;
        //    levelSong = (levelNumber % 2 == 1 ? level1Song : level1Song);
        //    return levelSong;
        //}

        private void CheckSound(GameTime gameTime)
        {
            if (accumSoundCheckTime >= TimeSpan.FromMilliseconds(soundCheckMS))
            {
                accumSoundCheckTime = TimeSpan.FromSeconds(0);

                if (lastPowerUpSoundState == SoundState.Playing && powerUpSoundEffectInstance.State == SoundState.Stopped && viewState == ViewState.Playing)
                {
                    //PlaySong(GetLevelSong(), null, true);
                    MediaPlayer.Volume = 1;
                }

                //if (level1Song.State == SoundState.Stopped)
                //{
                //    level1Song.IsLooped = true;
                //    level1Song.Play();
                //}

                lastPowerUpSoundState = powerUpSoundEffectInstance.State;
            }
            accumSoundCheckTime = accumSoundCheckTime.Add(gameTime.ElapsedGameTime);
        }

        private bool CheckCollisions(GameTime gameTime)
        {
            bool isPlayerBlocked = false;
            if (accumCollisionTime >= TimeSpan.FromMilliseconds(collisionMS))
            {
                accumCollisionTime = TimeSpan.FromSeconds(0);

                //if (camera2d.Zoom > 1)
                //{
                //    camera2d.Zoom -= .01f;
                //}

                var collisionResult = boss.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                {
                    player.ProcessDeath(tickCount);
                }

                //collisionResult = bossBullet.TestCollision(player, player.Position, gameMap.ScrollRows);
                //if (player.State == PlayerState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                //{
                //    player.ProcessDeath(tickCount);
                //}

                for (var i = 0; i < onScreenEnemies.Count; i++)
                {
                    var enemy = (IEnemy)onScreenEnemies[i];
                    if (enemy.State == CharacterState.Alive)
                    {
                        collisionResult = enemy.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                        if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                        {
                            player.ProcessDeath(tickCount);
                        }
                    }
                }

                for (var i = 0; i < onScreenBonuses.Count; i++)
                {
                    var bonus = (Bonus)onScreenBonuses[i];
                    collisionResult = bonus.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                    if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                    {
                        switch (bonus.State)
                        {
                            case BonusState.FiveHundredPoints:
                                bonus.State = BonusState.Used;
                                AddToScore(500);
                                bonusSoundEffectInstance.Play();
                                break;
                            case BonusState.ExtraLife:
                                bonus.State = BonusState.Used;
                                player.Lives++;
                                bonusSoundEffectInstance.Play();
                                gameMap.SaveScrollStartRow();
                                player.SavePosition();
                                break;
                            case BonusState.KillAllInScreen:
                                bonus.State = BonusState.Used;
                                foreach (var e in onScreenEnemies)
                                {
                                    AddToScore((int)Points.Minimum);
                                    e.State = CharacterState.Dead;
                                    e.ProcessDeath(tickCount);
                                }
                                bonusSoundEffectInstance.Play();
                                break;
                            case BonusState.Barrier:
                                isPlayerBlocked = true;
                                break;
                            case BonusState.Freeze:
                                bonus.State = BonusState.Used;
                                bonusSoundEffectInstance.Play();
                                gameMap.State = MapState.Timer;
                                clockSoundEffectInstance.IsLooped = true;
                                clockSoundEffectInstance.Play();
                                break;
                        }
                    }
                }

                for (var i = 0; i < onScreenWeapons.Count; i++)
                {
                    var weapon = (Weapon)onScreenWeapons[i];
                    collisionResult = weapon.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                    if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                    {
                        if (weapon.State != WeaponState.OneThousandPoints)
                            currentWeapon = weapon;

                        onScreenWeapons.Remove(weapon);

                        weaponSoundEffectInstance.Play();
                    }

                    //using (MidiOut midiOut = new MidiOut(0))
                    //{
                    //    midiOut.Volume = 65535;
                    //    midiOut.Send(MidiMessage.StartNote(60, 127, 0).RawData);
                    //}
                }

                for (var i = 0; i < onScreenPowerUps.Count; i++)
                {
                    var powerUp = (PowerUp)onScreenPowerUps[i];
                    collisionResult = powerUp.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                    if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                    {
                        onScreenPowerUps.Remove(powerUp);
                        MediaPlayer.Volume = 0;
                        accumSoundCheckTime = TimeSpan.FromSeconds(0);
                        powerUpSoundEffectInstance.Play();
                    }
                }

                for (var i = 0; i < playerBullets.Count; i++)
                {
                    var playerBullet = playerBullets[i];
                    collisionResult = playerBullet.TestCollision(boss, boss.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                    if (boss.State == CharacterState.Alive && player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                    {
                        AddToScore((int)Points.BossHit);
                        playerBullets.Remove(playerBullet);
                        boss.Hit();
                        if (boss.State == CharacterState.Dead)
                        {
                            MediaPlayer.Volume = 0;
                            StopSong();
                        }
                    }

                    for (var j = 0; j < onScreenEnemies.Count; j++)
                    {
                        var enemy = (IEnemy)onScreenEnemies[j];
                        if (enemy.State == CharacterState.Alive)
                        {
                            collisionResult = playerBullet.TestCollision(enemy, enemy.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                            {
                                enemy.Hit(playerBullet, tickCount);
                                if (enemy.State == CharacterState.Dead)
                                {
                                    AddToScore((int)Points.Minimum);
                                }
                                if (playerBullet.Damage <= 0)
                                {
                                    playerBullets.Remove(playerBullet);
                                }
                            }
                        }
                    }

                    for (var j = 0; j < onScreenBonuses.Count; j++)
                    {
                        var bonus = (Bonus)onScreenBonuses[j];
                        if (bonus.State == BonusState.Unknown || bonus.State == BonusState.Barrier)
                        {
                            collisionResult = playerBullet.TestCollision(bonus, bonus.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                            {
                                bonus.Hit(playerBullet);
                                if (bonus.State != BonusState.Unknown)
                                {
                                    AddToScore((int)Points.Minimum);
                                    playerBullets.Remove(playerBullet);
                                }
                                if (playerBullet.Damage <= 0)
                                {
                                    playerBullets.Remove(playerBullet);
                                }
                            }
                        }
                    }

                    for (var j = 0; j < onScreenWeapons.Count; j++)
                    {
                        var weapon = (Weapon)onScreenWeapons[j];
                        collisionResult = playerBullet.TestCollision(weapon, weapon.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                        if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                        {
                            weapon.Hit(playerBullet.Position);
                            playerBullets.Remove(playerBullet);
                        }
                    }

                    for (var j = 0; j < onScreenPowerUps.Count; j++)
                    {
                        var powerUp = (PowerUp)onScreenPowerUps[j];
                        collisionResult = playerBullet.TestCollision(powerUp, powerUp.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                        if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                        {
                            powerUp.Hit(playerBullet.Position);
                            playerBullets.Remove(playerBullet);
                        }
                    }
                }
            }
            accumCollisionTime = accumCollisionTime.Add(gameTime.ElapsedGameTime);
            return isPlayerBlocked;
        }

        private void AddToScore(int points)
        {
            score += points;
            hiScore = Math.Max(score, hiScore);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (levelFinished)
                return;

            switch (viewState)
            {
                case ViewState.Menu:
                    DrawViewStateMenu(spriteBatch, gameTime);
                    break;
                case ViewState.Intro:
                    if (currentSong == finishLevelSong)
                        DrawViewStateFinishLevel(spriteBatch, gameTime, levelName);
                    else
                        DrawViewStateIntro(spriteBatch, gameTime, levelName);
                    break;
                case ViewState.ShowLevel:
                    DrawViewStateLevel(spriteBatch, gameTime, levelName);
                    break;
                case ViewState.Playing:
                    DrawViewStatePlaying(spriteBatch, gameTime);
                    break;
                case ViewState.GameOver:
                    DrawViewStateGameOver(spriteBatch, gameTime);
                    break;
                case ViewState.TheEnd:
                    DrawViewStateTheEnd(spriteBatch, gameTime);
                    break;
            }
        }

        void DrawViewStateMenu(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(titleTexture, topLeftCorner, Color.White);
            var itemSelected = currentSong == startSong;

            DrawStringCentralized(spriteBatch, "",
                (menuItem == MenuItems.Start ? ">" : " ") +
                    ((itemSelected && menuItem == MenuItems.Start && blinkStart) ? "" : "START GAME"),
                (menuItem == MenuItems.ReviewApp ? ">" : " ") +
                    ((itemSelected && menuItem == MenuItems.ReviewApp && blinkStart) ? "" : "REVIEW APP"));
        }

        void DrawViewStateIntro(SpriteBatch spriteBatch, GameTime gameTime, string levelName)
        {
            DrawStringCentralized(spriteBatch, string.Format("LEVEL {0}", levelNumber), levelName);
        }

        void DrawViewStateFinishLevel(SpriteBatch spriteBatch, GameTime gameTime, string levelName)
        {
            DrawStringCentralized(spriteBatch, string.Format("CONGRATULATIONS!", "YOU HAVE DEFEATED {0}!", levelName));
        }

        void DrawViewStateLevel(SpriteBatch spriteBatch, GameTime gameTime, string levelName)
        {
            DrawStringCentralized(spriteBatch, string.Format("LEVEL {0}", levelNumber), levelName);
        }

        private void DrawViewStateGameOver(SpriteBatch spriteBatch, GameTime gameTime)
        {
            DrawStringCentralized(spriteBatch, "GAME OVER");
        }

        private void DrawViewStatePlaying(SpriteBatch spriteBatch, GameTime gameTime)
        {
            gameMap.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);

            for (var i = 0; i < onScreenBonuses.Count; i++)
            {
                var bonus = onScreenBonuses[i];
                bonus.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }

            for (var i = 0; i < onScreenWeapons.Count; i++)
            {
                var weapon = onScreenWeapons[i];
                weapon.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }

            for (var i = 0; i < onScreenPowerUps.Count; i++)
            {
                var powerUp = onScreenPowerUps[i];
                powerUp.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }

            player.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            boss.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            //if (boss.State == PlayerState.Alive)
            //    bossBullet.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            for (var i = 0; i < playerBullets.Count; i++)
            {
                var playerBullet = playerBullets[i];
                playerBullet.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }

            for (var i = 0; i < onScreenEnemies.Count; i++)
            {
                var enemy = onScreenEnemies[i];
                if (enemy.Position.Y + enemy.Size.Y >= gameMap.ScrollRows)
                {
                    enemy.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
                }
            }

            var textHeight = font.MeasureString("SCORE").Y;

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);

            spriteBatch.DrawString(font, string.Format("   FPS    Y         LIVES LEVEL  "), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight * 2), Color.White);
            spriteBatch.DrawString(font, string.Format("   {0:d5}    {1:d5}    {2:d2}     {3:d2}  ", (int)_frameCounter.AverageFramesPerSecond, (int)player.Position.Y, (int)(player.Lives), levelNumber), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight), Color.White);

            //spriteBatch.DrawString(font, string.Format("   X      Y         LIVES LEVEL  "), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight * 2), Color.White);
            //spriteBatch.DrawString(font, string.Format("   {0:d5}    {1:d5}    {2:d2}     {3:d2}  ", (int)player.Position.X, (int)player.Position.Y, (int)(player.Lives), levelNumber), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight), Color.White);

            //spriteBatch.DrawString(font, string.Format("   SCORE  HISCORE  LIVES LEVEL  "), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight * 2), Color.White);
            //spriteBatch.DrawString(font, string.Format("   {0:d5}    {1:d5}    {2:d2}     {3:d2}  ", score, hiScore, (int)(player.Lives), levelNumber), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight), Color.White);

            //spriteBatch.DrawString(font, string.Format("   BONUS  ENEMIES  WEAP"), new Vector2((screenSize.X - gameScreenSize.X) / 2, 401), Color.White);
            //spriteBatch.DrawString(font, string.Format("   {0:d5}    {1:d5}    {2:d2}", onScreenBonuses.Count(), onScreenEnemies.Where(e => e.State == PlayerState.Alive).Count(), onScreenWeapons.Count()), new Vector2((screenSize.X - gameScreenSize.X) / 2, 417), Color.White);

            //if (currentSong != null)
            //{
            //    var m1 = MediaPlayer.PlayPosition.Minutes;
            //    var s1 = MediaPlayer.PlayPosition.Seconds;
            //    var m2 = currentSong.Duration.Minutes;
            //    var s2 = currentSong.Duration.Seconds;

            //    spriteBatch.DrawString(font, string.Format("TIME  DURATION"), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight * 2), Color.White);
            //    spriteBatch.DrawString(font, string.Format("{0:d2} {1:d2} {2:d2} {3:d2}", m1, s1, m2, s2), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight), Color.White);
            //}

            if (gameMap.State == MapState.Paused)
                DrawStringCentralized(spriteBatch, "PAUSED");
        }

        private void DrawViewStateTheEnd(SpriteBatch spriteBatch, GameTime gameTime)
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
            spriteBatch.DrawString(font, string.Format("   SCORE  HISCORE  REST  LEVEL  "), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight * 2), Color.White);
            spriteBatch.DrawString(font, string.Format("   {0:d5}    {1:d5}    {2:d2}     {3:d2}  ", score, hiScore, (int)(player.Lives), levelNumber), new Vector2((screenSize.X - gameScreenSize.X) / 2, screenSize.Y - textHeight), Color.White);

            if (gameMap.State == MapState.Paused)
                DrawStringCentralized(spriteBatch, "PAUSED");
        }

        private void DrawLove(SpriteBatch spriteBatch)
        {
            Rectangle loveRectangle = new Rectangle(
                loveTexture.Height * ((tickCount / 4) % (loveTexture.Width / loveTexture.Height)),
                0,
                loveTexture.Height,
                loveTexture.Height);
            var frameIndex = (tickCount / 2) % 2;
            this.topLeftCorner = new Vector2((screenSize.X - gameScreenSize.X) / 2, (screenSize.Y - gameScreenSize.Y) / 2);
            spriteBatch.Draw(
                loveTexture,
                topLeftCorner + (player.Position - new Vector2(0, player.Size.Y)) * tileWidth,
                loveRectangle,
                Color.White);

            DrawStringCentralized(spriteBatch, "", "", "LOVE IS FOREVER...");
        }

        private void DrawScreenPad(SpriteBatch spriteBatch, GameTime gameTime)
        {
            screenPad.Draw(gameTime, spriteBatch);
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

    public enum MenuItems
    {
        None = 0,
        Start = 1,
        ReviewApp = 2
    }
}
