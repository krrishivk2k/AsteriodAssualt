using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteriod_Bel_Assault
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //
        SpriteFont pericles14;
        private float playerDeathDelayTime = 10f;
        private float playerDeathTimer = 0f;
        private float titleScreenTimer = 0f;
        private float titleScreenDelayTime = 1f;
        private int playerStartingLives = 3;
        private Vector2 playerStartLocation = new Vector2(390, 550);
        private Vector2 scoreLocation = new Vector2(20, 10);
        private Vector2 livesLocation = new Vector2(20, 25);

        enum GameStates { TitleScreen, Playing, PlayerDead, GameOver };
        GameStates gameState = GameStates.TitleScreen;
        Texture2D titleScreen;
        Texture2D spriteSheet;
        StarField starField;
        AsteroidManager asteroidManager;
        PlayerManager playerManager;
        EnemyManager enemyManager;
        ExplosionManager explosionManager;
        CollisionManager collisionManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            titleScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");
            spriteSheet = Content.Load<Texture2D>(@"Textures\spriteSheet");

            starField = new StarField(
            this.Window.ClientBounds.Width,
            this.Window.ClientBounds.Height,
            200,
            new Vector2(0, 30f),
            spriteSheet,
            new Rectangle(0, 450, 2, 2));

            asteroidManager = new AsteroidManager(
            10,
            spriteSheet,
            new Rectangle(0, 0, 50, 50),
            20,
            this.Window.ClientBounds.Width,
            this.Window.ClientBounds.Height);

            playerManager = new PlayerManager(
            spriteSheet,
            new Rectangle(0, 150, 50, 50),
            3,
            new Rectangle(
            0,
            0,
            this.Window.ClientBounds.Width,
            this.Window.ClientBounds.Height));

            enemyManager = new EnemyManager(
            spriteSheet,
            new Rectangle(0, 200, 50, 50),
            6,
            playerManager,
            new Rectangle(
            0,
            0,
            this.Window.ClientBounds.Width,
            this.Window.ClientBounds.Height));

            explosionManager = new ExplosionManager(
            spriteSheet,
            new Rectangle(0, 100, 50, 50),
            3,
            new Rectangle(0, 450, 2, 2));

            collisionManager = new CollisionManager(
            asteroidManager,
            playerManager,
            enemyManager,
            explosionManager);

            SoundManager.Initialize(Content);

            pericles14 = Content.Load<SpriteFont>(@"Fonts\Pericles14");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (gameState)
            {
                case GameStates.TitleScreen:
                    titleScreenTimer +=
                    (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (titleScreenTimer >= titleScreenDelayTime)
                    {
                        if ((Keyboard.GetState().IsKeyDown(Keys.Space)) ||
                        (GamePad.GetState(PlayerIndex.One).Buttons.A ==
                        ButtonState.Pressed))
                        {
                            playerManager.LivesRemaining = playerStartingLives;
                            playerManager.PlayerScore = 0;
                            resetGame();
                            gameState = GameStates.Playing;
                        }
                    }
                    break;
                case GameStates.Playing:
                    starField.Update(gameTime);
                    asteroidManager.Update(gameTime);
                    playerManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    explosionManager.Update(gameTime);
                    collisionManager.CheckCollisions();
                    if (playerManager.Destroyed)
                    {
                        playerDeathTimer = 0f;
                        enemyManager.Active = false;
                        playerManager.LivesRemaining--;
                        if (playerManager.LivesRemaining < 0)
                        {
                            gameState = GameStates.GameOver;
                        }
                        else
                        {
                            gameState = GameStates.PlayerDead;
                        }
                    }
                    break;
                case GameStates.PlayerDead:
                    playerDeathTimer +=
                    (float)gameTime.ElapsedGameTime.TotalSeconds;
                    starField.Update(gameTime);
                    asteroidManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    playerManager.PlayerShotManager.Update(gameTime);
                    explosionManager.Update(gameTime);
                    if (playerDeathTimer >= playerDeathDelayTime)
                    {
                        resetGame();
                        gameState = GameStates.Playing;
                    }
                    break;
                case GameStates.GameOver:
                    playerDeathTimer +=
                    (float)gameTime.ElapsedGameTime.TotalSeconds;
                    starField.Update(gameTime);
                    asteroidManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    playerManager.PlayerShotManager.Update(gameTime);
                    explosionManager.Update(gameTime);
                    if (playerDeathTimer >= playerDeathDelayTime)
                    {
                        gameState = GameStates.TitleScreen;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            if (gameState == GameStates.TitleScreen)
            {
                spriteBatch.Draw(titleScreen,
                new Rectangle(0, 0, this.Window.ClientBounds.Width,
                this.Window.ClientBounds.Height),
                Color.White);
            }
            if ((gameState == GameStates.Playing) ||
            (gameState == GameStates.PlayerDead) ||
            (gameState == GameStates.GameOver))
            {
                starField.Draw(spriteBatch);
                asteroidManager.Draw(spriteBatch);
                playerManager.Draw(spriteBatch);
                enemyManager.Draw(spriteBatch);
                explosionManager.Draw(spriteBatch);
                spriteBatch.DrawString(
                pericles14,
                "Score: " + playerManager.PlayerScore.ToString(),
                scoreLocation,
                Color.White);
                if (playerManager.LivesRemaining >= 0)
                {
                    spriteBatch.DrawString(
                    pericles14,
                    "Ships Remaining: " +
                    playerManager.LivesRemaining.ToString(),
                    livesLocation,
                    Color.White);
                }
            }
            if ((gameState == GameStates.GameOver))
            {
                spriteBatch.DrawString(
                pericles14,
                "G A M E O V E R !",
                new Vector2(
                this.Window.ClientBounds.Width / 2 -
                pericles14.MeasureString
                ("G A M E O V E R !").X / 2,
                50),
                Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        //Reset Game Function
        private void resetGame()
        {
            playerManager.playerSprite.Location = playerStartLocation;
            foreach (Sprite asteroid in asteroidManager.Asteroids)
            {
                asteroid.Location = new Vector2(-500, -500);
            }
            enemyManager.Enemies.Clear();
            enemyManager.Active = true;
            playerManager.PlayerShotManager.Shots.Clear();
            enemyManager.EnemyShotManager.Shots.Clear();
            playerManager.Destroyed = false;
        }
    }
}
