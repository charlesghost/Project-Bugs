using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ProjectBugs
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Game Switch
        int Menu;

        // Represents the player 
        Player player;

        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        // A movement speed for the player
        float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;

        // Enemies
        Texture2D enemyTexture;
        List<enemy> enemies;


        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;
        Texture2D explosionTexture;
        List<Animation> explosions;

        // The sound that is played when a laser is fired
        SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        SoundEffect explosionSound;

        // The music played during gameplay
        Song gameplayMusic;

        //Number that holds the player score
        int score;

        // The font used to display UI elements
        SpriteFont font;

        // A random number generator
        Random random;

        Texture2D projectileTexture;
        List<projectile> projectiles;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;

        // Screens 
        private Texture2D MainMenu;
        private Texture2D Instructions;
        private Texture2D GameOver;
        private Texture2D Facts;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {

            explosions = new List<Animation>();

            //Set player's score to zero
            score = 0;


            //Initialize the player class
            player = new Player();

            // Set a constant player move speed
            playerMoveSpeed = 8.0f;

            // Initialize the enemies list
            enemies = new List<enemy>();

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            random = new Random();

            projectiles = new List<projectile>();

            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);

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

            // Load the player resources 
            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(Content.Load<Texture2D>("player"), playerPosition);

            MainMenu = Content.Load<Texture2D>("MainMenu2");

            Instructions = Content.Load<Texture2D>("instructions");

            GameOver = Content.Load<Texture2D>("gameoverscreen");

            Facts = Content.Load<Texture2D>("factsscreen");

            mainBackground = Content.Load<Texture2D>("mainbackground");

            enemyTexture = Content.Load<Texture2D>("BugUnused");

            projectileTexture = Content.Load<Texture2D>("bullet");

            explosions = new List<Animation>();

            explosionTexture = Content.Load<Texture2D>("explosion");

            // Load the music
            gameplayMusic = Content.Load<Song>("sound/fear_the_mixup");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("sound/explosion");

            // Load the score font
            font = Content.Load<SpriteFont>("gameFont");

            // Start the music right away
            PlayMusic(gameplayMusic);

        }

        private void PlayMusic(Song song)
        {
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }

        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation BugAnimation = new Animation();

            // Initialize the animation with the correct animation information
            BugAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // Create an enemy
            enemy enemy = new enemy();

            // Initialize the enemy
            enemy.Initialize(BugAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);

                if (enemies[i].Active == false)
                {
                    // If not active and health <= 0
                    if (enemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(enemies[i].Position);

                        // Play the explosion sound
                        explosionSound.Play();

                        //Add to the player's score
                        score += enemies[i].Value;

                    }
                    enemies.RemoveAt(i);
                }
            }
        }

        private void AddProjectile(Vector2 position)
        {
            projectile projectile = new projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
            projectiles.Add(projectile);
        }

        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            switch (Menu)
            {
                case 0:
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                        Menu = 1;
                    if (Keyboard.GetState().IsKeyDown(Keys.Q))
                        Menu = 2;                                       
                    break;
                case 1:
                    if (Keyboard.GetState().IsKeyDown(Keys.Back))
                        Menu = 0;
                    // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
                    previousGamePadState = currentGamePadState;
                    previousKeyboardState = currentKeyboardState;

                    // Read the current state of the keyboard and gamepad and store it
                    currentKeyboardState = Keyboard.GetState();
                    currentGamePadState = GamePad.GetState(PlayerIndex.One);

                    //Update the player
                    UpdatePlayer(gameTime);

                    // Update the enemies
                    UpdateEnemies(gameTime);

                    // Update the collision
                    UpdateCollision();

                    // Update the projectiles
                    UpdateProjectiles();

                    // Update the explosions
                    UpdateExplosions(gameTime);

                    base.Update(gameTime);
                    break;

                case 2:
                    if (Keyboard.GetState().IsKeyDown(Keys.Back))
                        Menu = 0;
                    break;

                case 3:
                    if (Keyboard.GetState().IsKeyDown(Keys.Back))
                        Menu = 0;
                    break;

                case 4:
                    if (Keyboard.GetState().IsKeyDown(Keys.Back))
                        Menu = 0;
                    break;
            }

        }

        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);

            // Use the Keyboard / Dpad
            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
            currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                player.Position.X -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
            currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                player.Position.X += playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
            currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                player.Position.Y -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
            currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                player.Position.Y += playerMoveSpeed;
            }


            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            // Fire only every interval we set as the fireTime
            if (gameTime.TotalGameTime - previousFireTime > fireTime)
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddProjectile(player.Position + new Vector2(player.Width / 2, 0));

                // Play the laser sound
                laserSound.Play();

                // Game over and reset score if player health goes to zero
                if (player.Health <= 0)
                {
                   Menu = 3;
                   player.Health = 100;
                   score = 0;

                }
                // Win Condition
                if (score == 5000)
                {
                    Menu = 4;
                    player.Health = 100;
                    score = 0;

                }

            }

        }

        private void UpdateCollision()
        {
            // Use the Rectangle's built-in intersect functionto 
            // determine if two objects are overlapping
            Rectangle rectangle1;
            Rectangle rectangle2;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int)player.Position.X,
            (int)player.Position.Y,
            player.Width,
            player.Height);

            // Do the collision between the player and the enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                rectangle2 = new Rectangle((int)enemies[i].Position.X,
                (int)enemies[i].Position.Y,
                enemies[i].Width,
                enemies[i].Height);

                // Determine if the two objects collided with each
                // other
                if (rectangle1.Intersects(rectangle2))
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    player.Health -= enemies[i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies[i].Health = 0;

               }

            }

            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)projectiles[i].Position.X -
                    projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
                    (int)enemies[j].Position.Y - enemies[j].Height / 2,
                    enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= projectiles[i].Damage;
                        projectiles[i].Active = false;
                    }
                }
            }
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (Menu)
            {
                case 0:
                    // Start Menu 
                    MediaPlayer.Pause();
                    spriteBatch.Begin();
                    spriteBatch.Draw(MainMenu, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    break;

                case 1:
                    // Start drawing
                    spriteBatch.Begin();

                    // Resume Music
                    MediaPlayer.Resume();

                    // Draw Background
                    spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

                    // Draw the Player
                    player.Draw(spriteBatch);

                    // Draw the Enemies
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        enemies[i].Draw(spriteBatch);
                    }

                    // Draw the explosions
                    for (int i = 0; i < explosions.Count; i++)
                    {
                        explosions[i].Draw(spriteBatch);
                    }

                    // Draw the score
                    spriteBatch.DrawString(font, "score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
                    // Draw the player health
                    spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

                    // Draw the Projectiles
                    for (int i = 0; i < projectiles.Count; i++)
                    {
                        projectiles[i].Draw(spriteBatch);
                    }

                   
                    //Stop drawing
                    spriteBatch.End();

                    base.Draw(gameTime);
                    break;

                case 2:
                    // Show Instructions 
                    spriteBatch.Begin();
                    spriteBatch.Draw(Instructions, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    break;

                case 3:
                    // Game Over
                    MediaPlayer.Pause();
                    spriteBatch.Begin();
                    spriteBatch.Draw(GameOver, Vector2.Zero, Color.White);
                    spriteBatch.End();


                    break;

                case 4:
                    MediaPlayer.Pause();
                    spriteBatch.Begin();
                    spriteBatch.Draw(Facts, Vector2.Zero, Color.White);
                    spriteBatch.End();

                    break;                    

            }


        }

    }


}