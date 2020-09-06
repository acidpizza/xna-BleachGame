using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BleachGame
{
    enum TileCollision
    {
        Passable = 0,       // Does not block player
        Impassable = 1,     // Blocks player from all directions
        platform = 2        // Blocks player only from the top
    }

    struct Tile
    {
        public Texture2D texture;
        public TileCollision collision;

        public const int Width = 40;
        public const int Height = 30;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        public Tile(Texture2D texture, TileCollision collision)
        {
            this.texture = texture;
            this.collision = collision;
        }
    }
}
