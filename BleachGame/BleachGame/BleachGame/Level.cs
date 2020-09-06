using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Input;


namespace BleachGame
{
    class Level
    {
        // Physical structure of level
        Tile[,] tiles;
        Layer[] layers; // layers[0] contains furthest background image
        
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities
        Character Player;
        
        // Store all Content for the level
        public ContentManager content;
        public ContentManager Content { get { return content; } }
        public GraphicsDevice graphicsDevice;

        // Store Spawn Point
        Vector2 start;

        // Camera
        float cameraPosition;


        #region Loading

        // Random object for creating random tiles
        private Random random = new Random(354668); // Arbitrary, but constant seed

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        /// /// <param name="levelIndex">
        /// The level to load
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex, GraphicsDevice graphicsDevice)
        {
            content = new ContentManager(serviceProvider, "Content");
            this.graphicsDevice = graphicsDevice;


            // Create the tile 2D array and populate it.
            LoadTiles(fileStream);

            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);

            /*
            // Load the background layers for non-scrolling background.
            backgroundLayers = new Texture2D[3];
            for (int i = 0; i < backgroundLayers.Length; ++i)
            {
                backgroundLayers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + levelIndex);
            }
            */

        }
        
        /// <summary>
        /// Create the 2D array from a file and populate it.
        /// </summary>
        void LoadTiles(Stream fileStream)
        {
            // Check input file has same number of characters per line. 
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                // Use 1st line as a reference to how many characters per line.
                string line = reader.ReadLine();
                width = line.Length;
                
                // Iterate through file lines and check length.
                while (line != null)
                {
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    
                    lines.Add(line);
                    line = reader.ReadLine();
                }
            }

            // Create the space for the tile grid
            tiles = new Tile[width, lines.Count];

            // Populate the tile grid
            for (int y = 0; y < LevelHeight; ++y)
            {
                for (int x = 0; x < LevelWidth; ++x)
                {
                    // for each character in all lines, populate the tile grid.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that Player has been created (must have a player tile).
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
        }

        /// <summary>
        /// Maps a character to a TileType for a specific Tile in the 2D Tile grid
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetTileBounds(x, y));
            Player = new Byakuya(this, start, graphicsDevice);
            

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Width of the level in Tiles
        /// </summary>
        public int LevelWidth { get { return tiles.GetLength(0); } }

        /// <summary>
        /// Height of the level in tiles.
        /// </summary>
        public int LevelHeight { get { return tiles.GetLength(1); } }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetTileBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= LevelWidth)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= LevelHeight)
                return TileCollision.Passable;

            return tiles[x, y].collision;
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            Player.Update(gameTime, keyboardState);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // ==== Draw background layers ====
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            //foreach (Enemy enemy in enemies)
            //    enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // ==== Draw foreground layers (will hide entities) ====
            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, LevelWidth - 1);

            // For each tile position
            for (int y = 0; y < LevelHeight; ++y)
            {
                //for (int x = 0; x < LevelWidth; ++x)
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.position.X < marginLeft)
                cameraMovement = Player.position.X - marginLeft;
            else if (Player.position.X > marginRight)
                cameraMovement = Player.position.X - marginRight;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = Tile.Width * LevelWidth - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }

        #endregion


    }
}
