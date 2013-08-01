using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRun.TileEngine
{
    class MapCell
    {
        public int TileID 
        { 
            get { return BaseTiles.Count > 0 ? BaseTiles[0] : 0; } 
            set {
                if (BaseTiles.Count > 0)
                    BaseTiles[0] = value;
                else
                    AddBaseTile(value);
                } 
        }

        public List<int> BaseTiles = new List<int>();

        public void AddBaseTile(int tileID)
        {
            BaseTiles.Add(tileID);
        }

        public MapCell(int tileID)
        {
            TileID = tileID;
        }
    }
}
