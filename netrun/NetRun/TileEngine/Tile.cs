using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NetRun.TileEngine
{
    static class Tile
    {
        static public int TileWidth = 33;
        static public int TileHeight = 27;

        static public int TileStepX = 52;
        static public int TileStepY = 14;
        static public int OddRowOffset = 26;

        static public Texture2D TileSetTexture;

        static public Rectangle GetSourceRectangle(int tileIndex)
        {
            int tileY = tileIndex / (TileSetTexture.Width / TileWidth);
            int tileX = tileIndex % (TileSetTexture.Width / TileWidth);
            return new Rectangle(tileX * TileWidth, tileY * TileHeight, TileWidth, TileHeight);
        }

    }
}
