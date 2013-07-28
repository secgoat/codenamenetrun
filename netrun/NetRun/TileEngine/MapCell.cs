using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRun.TileEngine
{
    class MapCell
    {
        public int TileID { get; set; }
        public List<int> BaseTiles = new List<int>();


        public MapCell(int tileID)
        {
            TileID = tileID;
        }
    }
}
