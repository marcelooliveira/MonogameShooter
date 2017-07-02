using BaseVerticalShooter.Core.Input;
using BaseVerticalShooter.GameModel;
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
    public class ViewStatePlaying : ViewStateBase
    {
        Action<EnemyDeathMessage> EnemyDeathMessageAction;
        Action<PlayerDeathMessage> PlayerDeathMessageAction;
        Action<BossDeathMessage> BossDeathMessageAction;
        Action<BonusStateChangedMessage> BonusStateChangedMessageAction;
        Action<WeaponStateChangedMessage> WeaponStateChangedMessageAction;
        Action<ComboMessage> ComboMessageAction;
        Action<EnemyShotMessage> EnemyShotMessageAction;

        Song bossSong;
        Song dyingSong;
        SoundEffectInstance level1Song;
        SoundEffectInstance bonusSoundEffectInstance;
        SoundEffectInstance weaponSoundEffectInstance;
        SoundEffectInstance powerUpSoundEffectInstance;
        SoundState lastPowerUpSoundState = SoundState.Stopped;
        SoundEffectInstance comboSoundEffectInstance;
        SoundEffectInstance pauseSoundEffectInstance;
        SoundEffectInstance unpauseSoundEffectInstance;
        Texture2D blackTexture;

        Boss boss;
        IBasePlayer player;
        BossMovement bossMovement;
        Weapon currentWeapon;

        TimeSpan accumSoundCheckTime = TimeSpan.FromSeconds(0);
        TimeSpan accumLowPriorityElapsedGameTime = TimeSpan.FromSeconds(0);
        int lowPriorityTickInMS = 10;
        Vector2 weaponStartPosition = new Vector2(-15, -15);

        public ViewStatePlaying(GraphicsDeviceManager graphics, ContentManager content, ScreenPad screenPad, BossMovement bossMovement, int levelNumber, string levelName, IJsonMapManager jsonMapManager)
            : base(graphics, content, screenPad, bossMovement, levelNumber, levelName, jsonMapManager)
        {
            this.bossMovement = bossMovement;
        }

        public override void LoadContent(IContentHelper contentHelper)
        {
            SetupGameMap(contentHelper);
            SetupBoss(contentHelper);
            SetupSongs();
            SetupSongEffects();
            SetupPlayer(contentHelper);
            SetupBlackTexture();
            base.LoadContent(contentHelper);
        }

        public override void UnLoadContent()
        {
            base.UnLoadContent();
            gameMap.UnLoadContent();
        }

        public override void RegisterActions()
        {
            base.RegisterActions();
            NewMessenger.Default.Register<EnemyDeathMessage>(this, EnemyDeathMessageAction);
            NewMessenger.Default.Register<PlayerDeathMessage>(this, PlayerDeathMessageAction);
            NewMessenger.Default.Register<BossDeathMessage>(this, BossDeathMessageAction);
            NewMessenger.Default.Register<BonusStateChangedMessage>(this, BonusStateChangedMessageAction);
            NewMessenger.Default.Register<WeaponStateChangedMessage>(this, WeaponStateChangedMessageAction);
            NewMessenger.Default.Register<ComboMessage>(this, ComboMessageAction);
            NewMessenger.Default.Register<EnemyShotMessage>(this, EnemyShotMessageAction);
        }

        public override void UnregisterActions()
        {
            base.UnregisterActions();
            NewMessenger.Default.Unregister<EnemyDeathMessage>(this, EnemyDeathMessageAction);
            NewMessenger.Default.Unregister<PlayerDeathMessage>(this, PlayerDeathMessageAction);
            NewMessenger.Default.Unregister<BossDeathMessage>(this, BossDeathMessageAction);
            NewMessenger.Default.Unregister<BonusStateChangedMessage>(this, BonusStateChangedMessageAction);
            NewMessenger.Default.Unregister<WeaponStateChangedMessage>(this, WeaponStateChangedMessageAction);
            NewMessenger.Default.Unregister<ComboMessage>(this, ComboMessageAction);
            NewMessenger.Default.Unregister<EnemyShotMessage>(this, EnemyShotMessageAction);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateBackgroundMusic(player.Lives);
            UpdateSpecialButtons();
            UpdateGamePad();

            UpdateLowPriorityItems(gameTime);

            AccumulateGameTime(gameTime);
        }

        private void UpdateLowPriorityItems(GameTime gameTime)
        {
            var reachedLowPriorityTime = accumLowPriorityElapsedGameTime >= TimeSpan.FromMilliseconds(lowPriorityTickInMS);
            if (reachedLowPriorityTime)
            {
                accumLowPriorityElapsedGameTime = TimeSpan.FromSeconds(0);

                UpdateGameMap(gameTime);
                CheckCollisions(gameTime);
                UpdatePlayer(gameTime);

                if (gameMap.State == MapState.Scrolling)
                {
                    UpdateBonuses(gameTime);
                    UpdateWeapons(gameTime);
                    UpdatePowerUps(gameTime);
                    UpdateBoss(gameTime);
                }

                UpdateEnemies(gameTime);
                UpdateCurrentWeapon(gameTime);
                CheckSound(gameTime);
            }
            accumLowPriorityElapsedGameTime = accumLowPriorityElapsedGameTime.Add(gameTime.ElapsedGameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            DrawGameMap(spriteBatch, gameTime);
            DrawBonuses(spriteBatch, gameTime);
            DrawWeapons(spriteBatch, gameTime);
            DrawPowerUps(spriteBatch, gameTime);
            DrawPlayer(spriteBatch, gameTime);
            DarwBoss(spriteBatch, gameTime);
            DrawPlayerBullets(spriteBatch, gameTime);
            DrawEnemies(spriteBatch, gameTime);
            DrawScore(spriteBatch, gameTime);
            DrawPause(spriteBatch);
        }

        private void DrawScore(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var textHeight = font.MeasureString("SCORE").Y;
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);

            spriteBatch.Draw(blackTexture, new Rectangle((int)topLeftCorner.X, (int)screenSize.Y - (int)textHeight * 2 - 2, (int)GameSettings.Instance.GameScreenSize.X, GameSettings.Instance.MapTileWidth), Color.Black);
            spriteBatch.DrawString(font, string.Format(GameResources.ScoreHeader), new Vector2(topLeftCorner.X, screenSize.Y - textHeight * 2), Color.White);
            spriteBatch.DrawString(font, string.Format("   {0:d5}    {1:d5}    {2:d2}     {3:d2}  ", score, hiScore, (int)(player.Lives), levelNumber)
                , new Vector2(topLeftCorner.X, screenSize.Y - textHeight), Color.White);
        }

        private void DrawPause(SpriteBatch spriteBatch)
        {
            if (gameMap.State == MapState.Paused)
                DrawStringCentralized(spriteBatch, GameResources.Paused);
        }

        private void DrawEnemies(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //foreach (var enemy in onScreenEnemies
            //    .Where(e => e.Position.Y + e.Size.Y >= gameMap.ScrollRows)
            //    .OrderBy(e => e.Position.Y))
            //{
            //    enemy.DrawShadow(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            //}
 
            foreach (var enemy in onScreenEnemies
                .Where(e => e.Position.Y + e.Size.Y >= gameMap.ScrollRows)
                .OrderBy(e => e.Position.Y))
            {
                    enemy.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void DrawPlayerBullets(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (var i = 0; i < playerBullets.Count; i++)
            {
                var playerBullet = playerBullets[i];
                playerBullet.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void DarwBoss(SpriteBatch spriteBatch, GameTime gameTime)
        {
            boss.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
        }

        private void DrawPlayer(SpriteBatch spriteBatch, GameTime gameTime)
        {
            player.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
        }

        private void DrawPowerUps(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (var i = 0; i < onScreenPowerUps.Count; i++)
            {
                var powerUp = onScreenPowerUps[i];
                powerUp.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void DrawWeapons(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (var i = 0; i < onScreenWeapons.Count; i++)
            {
                var weapon = onScreenWeapons[i];
                weapon.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void DrawBonuses(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (var i = 0; i < onScreenBonuses.Count; i++)
            {
                var bonus = onScreenBonuses[i];
                bonus.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void DrawGameMap(SpriteBatch spriteBatch, GameTime gameTime)
        {
            gameMap.Draw(spriteBatch, gameTime, tickCount, gameMap.ScrollRows);
        }

        public override void InitializeLevel()
        {
            player.Initialize();
            gameMap.Initialize();
            InitializeEnemies();
            base.InitializeLevel();
        }

        protected override void DefineActions()
        {
            EnemyDeathMessageAction = EnemyDeath;
            PlayerDeathMessageAction = PlayerDeath;
            BossDeathMessageAction = BossDeath;
        }

        protected void CheckSound(GameTime gameTime)
        {
            if (accumSoundCheckTime >= TimeSpan.FromMilliseconds(soundCheckMS))
            {
                accumSoundCheckTime = TimeSpan.FromSeconds(0);

                if (lastPowerUpSoundState == SoundState.Playing
                    && powerUpSoundEffectInstance.State == SoundState.Stopped
                    )
                {
                    MediaPlayer.Volume = 1;
                }

                lastPowerUpSoundState = powerUpSoundEffectInstance.State;
            }
            accumSoundCheckTime = accumSoundCheckTime.Add(gameTime.ElapsedGameTime);
        }

        private void UpdateSpecialButtons()
        {
            var inputCode = InputManager.Instance.GetInputCode(screenPad, blinkCount);

            if (inputCode == InputCodes.B)
                ReviewButtonPressed();

            if (inputCode == InputCodes.A)
                PauseButtonPressed(inputCode);

            if (inputCode == InputCodes.X)
                FireButtonPressed(inputCode);
        }

        private void UpdateBackgroundMusic(float previousRemainingLives)
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {
                if (IsInsideBossZone())
                {
                    PlaySong(bossSong,
                            (s, e) =>
                            {
                                ReplayBossSong(previousRemainingLives);
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
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            for (var i = 0; i < onScreenEnemies.Count; i++)
            {
                var enemy = onScreenEnemies[i];

                var enemyIsBeyondLeftMargin = enemy.Position.X < -windowTilesSize.X;
                var enemyIsBeyondRightMargin = enemy.Position.X > GameSettings.Instance.WindowTilesSize.X * 2;
                var enemyIsBeyondBottomMargin = enemy.Position.Y > gameMap.ScrollRows + GameSettings.Instance.WindowTilesSize.Y;

                if (enemyIsBeyondLeftMargin
                    || enemyIsBeyondRightMargin
                    || enemyIsBeyondBottomMargin)
                {
                    RemoveEnemy(enemy);
                }
                else
                {
                    UpdateEnemy(gameTime, enemy);
                    ShootNewEnemyBullet(enemy);
                }
            }
        }

        private void SetupPlayer(IContentHelper contentHelper)
        {
            player = new Player();
            player.LoadContent(contentHelper);
            currentWeapon = new Weapon(weaponStartPosition, player);
            currentWeapon.LoadContent(contentHelper);
        }

        private void SetupBlackTexture()
        {
            blackTexture = new Texture2D(this.graphics.GraphicsDevice, 1, 1);
            blackTexture.SetData(new Color[] { Color.Black });
        }

        private void SetupSongEffects()
        {
            bonusSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Bonus");
            weaponSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Weapon");
            powerUpSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("PowerUp");
            comboSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("Combo");
            pauseSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("pause");
            unpauseSoundEffectInstance = ContentHelper.Instance.GetSoundEffectInstance("unpause");
        }

        private void SetupSongs()
        {
            level1Song = ContentHelper.Instance.GetSoundEffectInstance("Level1");
            dyingSong = ContentHelper.Instance.GetContent<Song>("Death");
        }

        private void SetupBoss(IContentHelper contentHelper)
        {
            boss = new Boss(windowTilesSize, levelNumber, bossMovement);
            boss.LoadContent(contentHelper);
            bossSong = ContentHelper.Instance.GetContent<Song>("BossTheme");
        }

        private void SetupGameMap(IContentHelper contentHelper)
        {
            gameMap = new Map(levelNumber, jsonMapManager);
            gameMap.LoadContent(contentHelper);
            gameMap.Scrolled += gameMap_Scrolled;
        }

        private void EnemyDeath(EnemyDeathMessage message)
        {
            var wasCombo = CheckIfEnemyDeathGeneratesCombo(message);
            if (wasCombo)
            {
                ProcessCombo(message);
            }
            else
            {
                onScreenEnemies.Remove(message.Enemy);
                if (!message.Enemy.IsBullet)
                    enemies.Add(message.Enemy);
            }
        }

        private bool CheckIfEnemyDeathGeneratesCombo(EnemyDeathMessage message)
        {
            bool wasCombo = false;
            var enemyGroupId = message.Enemy.GroupId;
            if (enemyGroupCount.ContainsKey(enemyGroupId))
            {
                enemyGroupCount[enemyGroupId]--;
                wasCombo = enemyGroupCount[enemyGroupId] == 0;
            }
            return wasCombo;
        }

        private void ProcessCombo(EnemyDeathMessage message)
        {
            message.Enemy.State = CharacterState.Combo;
            comboSoundEffectInstance.Play();
            AddToScore((int)Points.Combo);

            NewMessenger.Default.Send(new ComboMessage { Enemy = message.Enemy });
        }

        private void PlayerDeath(PlayerDeathMessage message)
        {
            StopSounds();

            PlaySong(dyingSong, (s, e) =>
            {
                AfterDyingSong();
            });
        }

        private void AfterDyingSong()
        {
            if (currentSong == dyingSong)
            {
                if (MediaPlayer.State == MediaState.Stopped)
                {
                    ShowGameOverOrShowLevel();
                }
            }
        }

        private void ShowGameOverOrShowLevel()
        {
            if (player.Lives <= 0)
            {
                NewMessenger.Default.Send<ViewStateChangedMessage>(new ViewStateChangedMessage { ViewState = ViewState.GameOver });
            }
            else
            {
                NewMessenger.Default.Send<ViewStateChangedMessage>(new ViewStateChangedMessage { ViewState = ViewState.ShowLevel });
                RestoreScrollStartRow();
                RespawnEnemies();
                RestoreCurrentWeaponState();
                RespawnPlayer();
            }
        }

        private void StopSounds()
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
        }

        private void BossDeath(BossDeathMessage message)
        {
            if (this.levelNumber == GameSettings.Instance.GetLevelNames().Count())
            {
                PlaySong(finishLevelSong, (s, e) =>
                {
                    AfterFinishLevelSongAtLastLevel();
                });
            }
            else
            {
                PlaySong(finishLevelSong, (s, e) =>
                {
                    AfterFinishLevelSongBeforeLastLevel();
                });

                levelFinished = true;
                NewMessenger.Default.Send(
                    new PassedLevelMessage { LevelPassed = this.levelNumber });
            }
        }

        private void AfterFinishLevelSongBeforeLastLevel()
        {
            if (currentSong == finishLevelSong
                && MediaPlayer.State == MediaState.Stopped)
            {
                NewMessenger.Default.Send<ViewStateChangedMessage>(
                    new ViewStateChangedMessage { ViewState = ViewState.PassedLevel });
            }
        }

        private void AfterFinishLevelSongAtLastLevel()
        {
            if (currentSong == finishLevelSong
                && MediaPlayer.State == MediaState.Stopped)
            {
                NewMessenger.Default.Send<ViewStateChangedMessage>(
                    new ViewStateChangedMessage { ViewState = ViewState.TheEnd });
                PlaySong(theEndSong, (s, e) =>
                {
                    AfterTheEndSong();
                });
            }
        }

        private void AfterTheEndSong()
        {
            if (currentSong == theEndSong
                && MediaPlayer.State == MediaState.Stopped)
            {
                StopSong(() =>
                {
                    ShowMenuView();
                });
            }
        }

        private void ShowMenuView()
        {
            blinkStart = false;
            currentMenuItem = MenuItems.Start;
            NewMessenger.Default.Send<ViewStateChangedMessage>(
                new ViewStateChangedMessage { ViewState = ViewState.Menu });
            NewMessenger.Default.Send(
                new PassedLevelMessage { LevelPassed = this.levelNumber });
        }

        private void UpdateCurrentWeapon(GameTime gameTime)
        {
            currentWeapon.Update(gameTime, tickCount, gameMap.ScrollRows);
        }

        private void UpdateGamePad()
        {
            if (player.State == CharacterState.Alive)
            {
                if (gameMap.State != MapState.Paused)
                    player.ProcessGamePad(screenPad, (PhysicalObject)gameMap, gameMap.ScrollRows);
            }
        }

        private void AccumulateGameTime(GameTime gameTime)
        {
            if (accumElapsedGameTime >= TimeSpan.FromMilliseconds(tickInMS))
            {
                accumElapsedGameTime = TimeSpan.FromSeconds(0);
                tickCount++;
            }
            accumElapsedGameTime = accumElapsedGameTime.Add(gameTime.ElapsedGameTime);
        }

        private void FireButtonPressed(char inputCode)
        {
            if (gameMap.State != MapState.Paused
                && player.CanShoot
                && player.State == CharacterState.Alive)
            {
                List<PlayerBullet> weaponBullets = currentWeapon.Shoot();

                foreach (var playerBullet in weaponBullets)
                {
                    ShootPlayerBullet(playerBullet);
                }
            }
            inputCode = InputCodes.Empty;
        }

        private void ShootPlayerBullet(PlayerBullet playerBullet)
        {
            playerBullet.Position = GetStartPlayerBulletPosition(playerBullet);

            playerBullet.OffScreen += (spb, epb) =>
            {
                playerBullets.Remove(playerBullet);
            };
            playerBullets.Add(playerBullet);
            playerBullet.Shoot();
        }

        private Vector2 GetStartPlayerBulletPosition(PlayerBullet playerBullet)
        {
            var playerCenter = player.Size / 2;
            var bulletSize = playerBullet.Size / 2;
            return player.Position
                + playerCenter
                - bulletSize
                + player.SpriteDirection;
        }

        private static void ReviewButtonPressed()
        {
            var reviewHelper = BaseResolver.Instance.Resolve<IReviewHelper>();
            reviewHelper.MarketPlaceReviewTask();
        }

        private void PauseButtonPressed(char inputCode)
        {
            inputCode = InputCodes.Empty;
            if (gameMap.State == MapState.Paused)
                Resume();
            else
                Pause();
        }

        private void Pause()
        {
            level1Song.Pause();
            pauseSoundEffectInstance.Play();
            gameMap.State = MapState.Paused;
        }

        private void Resume()
        {
            level1Song.Play();
            gameMap.State = MapState.Scrolling;
            unpauseSoundEffectInstance.Play();
        }

        private void ShootNewBossBullet()
        {
            var bossBullet = boss.GetBullet(player, gameMap);
            bossBullet.LoadContent(contentHelper);
            onScreenEnemies.Add(bossBullet);
        }

        private void ShootNewEnemyBullet(IEnemy enemy)
        {
            var enemyIsOnSight = enemy.OnWindowPosition.HasValue && enemy.OnWindowPosition.Value.Y > 0;
            if (enemyIsOnSight)
            {
                if (enemy.IsAlone
                    && !enemy.IsBullet
                    && enemy.Reloaded)
                {
                    if (enemy.Bullet == null)
                    {
                        var enemyBullet = enemy.GetBullet(player, gameMap);
                        if (enemyBullet != null)
                        {
                            enemyBullet.LoadContent(contentHelper);
                            onScreenEnemies.Add(enemyBullet);
                        }
                    }
                }
            }
        }

        private void UpdateEnemy(GameTime gameTime, IEnemy enemy)
        {
            enemy.UpdateDirection(player, gameMap);
            enemy.Update(gameTime, tickCount, gameMap.ScrollRows, onScreenEnemies, gameMap);
        }

        private void RemoveEnemy(IEnemy enemy)
        {
            onScreenEnemies.Remove(enemy);
            if (!enemy.IsBullet)
                enemies.Add(enemy);
        }

        private void UpdateBoss(GameTime gameTime)
        {
            boss.Update(gameTime, tickCount, gameMap.ScrollRows);
            var playerIsWithinBossShootingRange = gameMap.ScrollRows <= GameSettings.Instance.WindowTilesSize.Y / 2;
            if (playerIsWithinBossShootingRange
                && boss.State == CharacterState.Alive
                && boss.Reloaded)
            {
                ShootNewBossBullet();
            }
        }

        private void UpdatePowerUps(GameTime gameTime)
        {
            for (var i = 0; i < onScreenPowerUps.Count; i++)
            {
                var powerUp = onScreenPowerUps[i];
                powerUp.Update(gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void UpdateWeapons(GameTime gameTime)
        {
            for (var i = 0; i < onScreenWeapons.Count; i++)
            {
                var weapon = onScreenWeapons[i];
                weapon.Update(gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void UpdateBonuses(GameTime gameTime)
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
        }

        private void UpdatePlayerBullets(GameTime gameTime)
        {
            for (var i = 0; i < playerBullets.Count; i++)
            {
                var playerBullet = playerBullets[i];
                playerBullet.Update(gameTime, tickCount, gameMap.ScrollRows);
            }
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime, tickCount, gameMap.ScrollRows);
            UpdatePlayerBullets(gameTime);
        }

        private void UpdateGameMap(GameTime gameTime)
        {
            gameMap.Update(gameTime, tickCount, gameMap.ScrollRows);
        }

        private void ReplayBossSong(float previousRemainingLives)
        {
            if (currentSong == bossSong
                && previousRemainingLives == player.Lives
                && boss.State == CharacterState.Alive)
            {
                if (MediaPlayer.State == MediaState.Stopped)
                    PlaySong(bossSong, null, true);
            }
        }

        private bool IsInsideBossZone()
        {
            return gameMap.ScrollRows < GameSettings.Instance.WindowTilesSize.Y;
        }

        private void CheckCollisions(GameTime gameTime)
        {
            if (accumCollisionTime >= TimeSpan.FromMilliseconds(collisionMS))
            {
                accumCollisionTime = TimeSpan.FromSeconds(0);
                CheckPlayerCollision();
                CheckPlayerBulletsCollision();
            }
            accumCollisionTime = accumCollisionTime.Add(gameTime.ElapsedGameTime);
        }

        private void CheckPlayerCollision()
        {
            CheckPlayerVsBossCollision();
            CheckPlayerVsEnemiesCollision();
            CheckPlayerVsBonusesCollision();
            CheckPlayerVsWeaponsCollision();
            CheckPlayerVsPowerUpsCollision();
            CheckPlayerIsOffScreen();
        }

        private void CheckPlayerIsOffScreen()
        {
            bool playerIsBeyondBottomMargin = player.Position.Y >= GameSettings.Instance.WindowTilesSize.Y;
            if (playerIsBeyondBottomMargin)
            {
                player.ProcessDeath(tickCount, true);
            }
        }

        private void CheckPlayerBulletsCollision()
        {
            for (var i = 0; i < playerBullets.Count; i++)
            {
                var playerBullet = playerBullets[i];
                CheckPlayerBulletVsBossCollision(playerBullet);
                CheckPlayerBulletVsEnemiesCollision(playerBullet);
                CheckPlayerBulletVsBonusesCollision(playerBullet);
                CheckPlayerBulletVsWeaponsCollision(playerBullet);
                CheckPlayerBulletVsPowerUpsCollision(playerBullet);
            }
        }

        private void CheckPlayerBulletVsPowerUpsCollision(BaseBullet playerBullet)
        {
            for (var j = 0; j < onScreenPowerUps.Count; j++)
            {
                var powerUp = (PowerUp)onScreenPowerUps[j];
                CheckPlayerBulletVsPowerUpCollision(playerBullet, powerUp);
            }
        }

        private void CheckPlayerBulletVsPowerUpCollision(BaseBullet playerBullet, PowerUp powerUp)
        {
            var collisionResult = playerBullet.TestCollision(powerUp, powerUp.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
            {
                powerUp.Hit(playerBullet.Position);
                playerBullets.Remove(playerBullet);
            }
        }

        private void CheckPlayerBulletVsWeaponsCollision(BaseBullet playerBullet)
        {
            for (var j = 0; j < onScreenWeapons.Count; j++)
            {
                var weapon = (Weapon)onScreenWeapons[j];
                CheckPlayerBulletVsWeaponCollision(playerBullet, weapon);
            }
        }

        private void CheckPlayerBulletVsWeaponCollision(BaseBullet playerBullet, Weapon weapon)
        {
            var collisionResult = playerBullet.TestCollision(weapon, weapon.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
            {
                weapon.Hit(playerBullet.Position);
                playerBullets.Remove(playerBullet);
            }
        }

        private void CheckPlayerBulletVsBonusesCollision(BaseBullet playerBullet)
        {
            for (var j = 0; j < onScreenBonuses.Count; j++)
            {
                var bonus = (Bonus)onScreenBonuses[j];
                CheckPlayerBulletVsBonusCollision(playerBullet, bonus);
            }
        }

        private void CheckPlayerBulletVsBonusCollision(BaseBullet playerBullet, Bonus bonus)
        {
            if (bonus.State == BonusState.Unknown || bonus.State == BonusState.Barrier)
            {
                var collisionResult = playerBullet.TestCollision(bonus, bonus.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                if (player.State == CharacterState.Alive
                    && collisionResult.CollisionType == CollisionType.Blocked)
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

        private void CheckPlayerBulletVsEnemiesCollision(BaseBullet playerBullet)
        {
            for (var j = 0; j < onScreenEnemies.Count; j++)
            {
                var enemy = (IEnemy)onScreenEnemies[j];
                CheckPlayerBulletVsEnemyCollision(playerBullet, enemy);
            }
        }

        private void CheckPlayerBulletVsEnemyCollision(BaseBullet playerBullet, IEnemy enemy)
        {
            if (enemy.State == CharacterState.Alive)
            {
                var collisionResult = playerBullet.TestCollision(enemy, enemy.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
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

        private void CheckPlayerBulletVsBossCollision(BaseBullet playerBullet)
        {
            var collisionResult = playerBullet.TestCollision(boss, boss.Position - new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
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
        }

        private void CheckPlayerVsPowerUpsCollision()
        {
            for (var i = 0; i < onScreenPowerUps.Count; i++)
            {
                var powerUp = (PowerUp)onScreenPowerUps[i];
                CheckPlayerVsPowerUpCollision(powerUp);
            }
        }

        private void CheckPlayerVsPowerUpCollision(PowerUp powerUp)
        {
            var collisionResult = powerUp.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
            {
                onScreenPowerUps.Remove(powerUp);
                MediaPlayer.Volume = 0;
                accumSoundCheckTime = TimeSpan.FromSeconds(0);
                powerUpSoundEffectInstance.Play();
            }
        }

        private void CheckPlayerVsWeaponsCollision()
        {
            for (var i = 0; i < onScreenWeapons.Count; i++)
            {
                var weapon = (Weapon)onScreenWeapons[i];
                CheckPlayerVsWeaponCollision(weapon);
            }
        }

        private void CheckPlayerVsWeaponCollision(Weapon weapon)
        {
            var collisionResult = weapon.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
            {
                if (weapon.State != WeaponState.OneThousandPoints)
                    currentWeapon = weapon;

                onScreenWeapons.Remove(weapon);
                weaponSoundEffectInstance.Play();
            }
        }

        private void CheckPlayerVsBonusesCollision()
        {
            for (var i = 0; i < onScreenBonuses.Count; i++)
            {
                var bonus = (Bonus)onScreenBonuses[i];
                CheckPlayerVsBonusCollision(bonus);
            }
        }

        private void CheckPlayerVsBonusCollision(Bonus bonus)
        {
            var collisionResult = bonus.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
            {
                switch (bonus.State)
                {
                    case BonusState.FiveHundredPoints:
                        CollectBonusFiveHundredPoints(bonus);
                        break;
                    case BonusState.ExtraLife:
                        CollectBonusExtraLife(bonus);
                        break;
                    case BonusState.KillAllInScreen:
                        CollectBonusKillAllInScreen(bonus);
                        break;
                    case BonusState.Barrier:
                        //isPlayerBlocked = true;
                        break;
                    case BonusState.Freeze:
                        CollectBonusFreeze(bonus);
                        break;
                }
            }
        }

        private void CollectBonusFreeze(Bonus bonus)
        {
            bonus.State = BonusState.Used;
            bonusSoundEffectInstance.Play();
            gameMap.State = MapState.Timer;
            clockSoundEffectInstance.IsLooped = true;
            clockSoundEffectInstance.Play();
        }

        private void CollectBonusKillAllInScreen(Bonus bonus)
        {
            bonus.State = BonusState.Used;
            foreach (var e in onScreenEnemies)
            {
                AddToScore((int)Points.Minimum);
                e.State = CharacterState.Dead;
                e.ProcessDeath(tickCount);
            }
            bonusSoundEffectInstance.Play();
        }

        private void CollectBonusExtraLife(Bonus bonus)
        {
            bonus.State = BonusState.Used;
            player.Lives++;
            bonusSoundEffectInstance.Play();
            gameMap.SaveScrollStartRow();
            player.SavePosition();
        }

        private void CollectBonusFiveHundredPoints(Bonus bonus)
        {
            bonus.State = BonusState.Used;
            AddToScore(500);
            bonusSoundEffectInstance.Play();
        }

        private void CheckPlayerVsEnemiesCollision()
        {
            for (var i = 0; i < onScreenEnemies.Count; i++)
            {
                var enemy = (IEnemy)onScreenEnemies[i];
                CheckPlayerVsEnemyCollision(enemy);
            }
        }

        private void CheckPlayerVsEnemyCollision(IEnemy enemy)
        {
            if (enemy.State == CharacterState.Alive)
            {
                var collisionResult = enemy.TestCollision(player, player.Position + new Vector2(0, gameMap.ScrollRows), gameMap.ScrollRows);
                if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                {
                    player.ProcessDeath(tickCount);
                }
            }
        }

        private void CheckPlayerVsBossCollision()
        {
            var collisionResult = boss.TestCollision(player,
                player.Position + new Vector2(0, gameMap.ScrollRows),
                gameMap.ScrollRows);
            if (player.State == CharacterState.Alive
                && collisionResult.CollisionType == CollisionType.Blocked)
            {
                player.ProcessDeath(tickCount);
            }
        }

        private void AddToScore(int points)
        {
            score += points;
            hiScore = Math.Max(score, hiScore);
        }

        private void RespawnPlayer()
        {
            player.Respawn();
        }

        private void RestoreCurrentWeaponState()
        {
            currentWeapon.State = WeaponState.Arrow;
        }

        private void RestoreScrollStartRow()
        {
            gameMap.RestoreScrollStartRow();
        }

        private void RespawnEnemies()
        {
            foreach (var enemy in onScreenEnemies.Where(enemy => !enemy.IsBullet).ToList())
            {
                enemies.Add(enemy);
            }

            foreach (var enemy in enemies)
            {
                enemy.State = CharacterState.Alive;
                enemy.RestorePosition();
            }
            onScreenEnemies.Clear();
        }

        private void gameMap_Scrolled(object sender, EventArgs e)
        {
            clockSoundEffectInstance.Stop();
            ScrollPlayer();
            ScrollOnScreenEnemies();
            ScrollIncomingEnemies();
            ScrollOutGoingEnemies();
            ScrollOutGoingBonuses();
            ScrollOutGoingWeapons();
            ScrollOutGoingPowerUps();
            PlayBossSongIfEnteredBossZone();

            lastScrollRows = gameMap.ScrollRows;
        }

        private void PlayBossSongIfEnteredBossZone()
        {
            bool justEnteredBossZone = gameMap.ScrollRows
                == GameSettings.Instance.WindowTilesSize.Y
                && lastScrollRows != gameMap.ScrollRows;
            if (justEnteredBossZone)
            {
                MediaPlayer.Volume = 1f;
                level1Song.Stop();
                PlaySong(bossSong, null, true);
            }
        }

        private void ScrollOutGoingPowerUps()
        {
            for (var i = powerUps.Count - 1; i >= 0; i--)
            {
                var powerUp = powerUps[i];
                ScrollOutGoingPowerUp(powerUp);
            }
        }

        private void ScrollOutGoingPowerUp(PowerUp powerUp)
        {
            if (powerUp.Position.Y + powerUp.Size.Y >= gameMap.ScrollRows - GameSettings.Instance.WindowTilesSize.Y)
            {
                powerUps.Remove(powerUp);
                onScreenPowerUps.Add(powerUp);
            }
        }

        private void ScrollOutGoingWeapons()
        {
            for (var i = weapons.Count - 1; i >= 0; i--)
            {
                var weapon = weapons[i];
                ScrollOutGoingWeapon(weapon);
            }
        }

        private void ScrollOutGoingWeapon(Weapon weapon)
        {
            if (weapon.Position.Y + weapon.Size.Y >= gameMap.ScrollRows - GameSettings.Instance.WindowTilesSize.Y)
            {
                weapons.Remove(weapon);
                onScreenWeapons.Add(weapon);
            }
        }

        private void ScrollOutGoingBonuses()
        {
            for (var i = bonuses.Count - 1; i >= 0; i--)
            {
                var bonus = bonuses[i];
                ScrollOutGoingBonus(bonus);
            }
        }

        private void ScrollOutGoingBonus(PhysicalObject bonus)
        {
            var bonusWentOffScreen = bonus.Position.Y + bonus.Size.Y >= gameMap.ScrollRows;
            if (bonusWentOffScreen)
            {
                bonuses.Remove(bonus);
                onScreenBonuses.Add(bonus);
            }
        }

        private void ScrollOutGoingEnemies()
        {
            for (var i = onScreenEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = onScreenEnemies[i];
                ScrollOutGoingEnemy(enemy);
            }
        }

        private void ScrollOutGoingEnemy(IEnemy enemy)
        {
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

        private void ScrollIncomingEnemies()
        {

            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                ScrollIncomingEnemy(enemy);
            }
        }

        private void ScrollIncomingEnemy(IEnemy enemy)
        {
            var enemyBecameEnteredScreen = enemy.Position.Y == gameMap.ScrollRows - (enemy.IsAnchored ? enemy.Size.Y : 0);
            if (enemyBecameEnteredScreen)
            {
                if (enemies.Contains(enemy))
                {
                    enemies.Remove(enemy);
                    onScreenEnemies.Add(enemy);
                }
            }
        }

        private void ScrollOnScreenEnemies()
        {
            foreach (var enemy in onScreenEnemies)
            {
                if (!enemy.IsBullet && !enemy.IsFlying && enemy.State == CharacterState.Alive)
                {
                    enemy.OnWindowPosition += new Vector2(0, .125f) * tileWidth;
                    enemy.Position += new Vector2(0, .125f);
                }
            }
        }

        private void ScrollPlayer()
        {
            var collisionResult = gameMap.TestCollision(player, player.Position, gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType == CollisionType.Blocked)
                player.Position = player.Position + new Vector2(0, .125f);

            collisionResult = gameMap.TestCollision(player, player.Position, gameMap.ScrollRows);
            if (player.State == CharacterState.Alive && collisionResult.CollisionType != CollisionType.None)
            {
                player.ProcessDeath(tickCount);
            }
        }

        private async void InitializeEnemies()
        {
            enemies.Clear();
            var mapLines = await gameMap.GetMapLinesAsync(levelNumber);
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
                        bonus.LoadContent(contentHelper);
                        bonuses.Add(bonus);
                    }
                    else if (c == 'W')
                    {
                        var weapon = new Weapon(pos, player);
                        weapon.LoadContent(contentHelper);
                        weapons.Add(weapon);
                    }

                    x++;
                }
                y++;
            }

            var index = 0;
            foreach (var tileInfo in jsonMap.layers[4].tileIndexes)
            {
                tileInfo.x = index % 16;
                tileInfo.y = (int)(index / 16);
                index++;
                if (tileInfo.ti > 0)
                {
                    var pos = new Vector2(tileInfo.x * 2, tileInfo.y * 2);
                    var enemyCode = Convert.ToChar(((int)'a') + tileInfo.ti).ToString();
                    var enemyGroupId = GetEnemyGroupId(tileInfo.y, tileInfo.x, enemyCode);
                    var enemy = BaseResolver.Instance.ResolveEnemy(tileInfo.ti, pos, enemyGroupId);
                    enemy.LoadContent(contentHelper);
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
    }
}
