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

namespace Byakuya_Test
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ByakuyaSprite byakuya = new ByakuyaSprite(400, 400);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            // TODO: use this.Content to load your game content here
            Texture2D texStanding = Content.Load<Texture2D>("standing");
            Texture2D texRunning = Content.Load<Texture2D>("running");
            Texture2D texJumping = Content.Load<Texture2D>("jumping");
            Texture2D texDashUp = Content.Load<Texture2D>("dash_up");
            Texture2D texDashRight = Content.Load<Texture2D>("dash_right");
            byakuya.LoadSprites(texStanding, texRunning, texJumping, texDashUp, texDashRight);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            GetKeyboardState(gameTime);
            byakuya.Update(gameTime);
            
            base.Update(gameTime);
        }

        private void GetKeyboardState(GameTime gameTime)
        {
            bool isStanding = true;
            KeyboardState keybstate = Keyboard.GetState();
            /*
            if (keybstate.IsKeyDown(Keys.Up))
            {
                byakuya.RunUp(gameTime);
                isStanding = false;
            }
            */
            if (keybstate.IsKeyDown(Keys.Down))
            {
                byakuya.Duck();
                isStanding = false;
            }
            else
            {
                if (keybstate.IsKeyDown(Keys.Left))
                {
                    byakuya.RunLeft(gameTime);
                    isStanding = false;
                }
                if (keybstate.IsKeyDown(Keys.Right))
                {
                    byakuya.RunRight(gameTime);
                    isStanding = false;
                }
                if (keybstate.IsKeyDown(Keys.A))
                {
                    byakuya.Jump();
                    isStanding = false;
                }
                if (keybstate.IsKeyDown(Keys.S) && keybstate.IsKeyDown(Keys.Up))
                {
                    byakuya.DashUp();
                    isStanding = false;
                }
                if (keybstate.IsKeyDown(Keys.S) && keybstate.IsKeyDown(Keys.Right))
                {
                    byakuya.DashRight();
                    isStanding = false;
                }
                if (keybstate.IsKeyDown(Keys.S) && keybstate.IsKeyDown(Keys.Left))
                {
                    byakuya.DashLeft();
                    isStanding = false;
                }

                // Default action when no other actions taken
                if (isStanding)
                {
                    byakuya.Stand();
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            byakuya.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
