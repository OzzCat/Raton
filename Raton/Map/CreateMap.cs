using BruTile.MbTiles;
using Mapsui.Tiling.Layers;
using SQLite;
using Color = Mapsui.Styles.Color;

namespace Raton.Map
{
    public static class CreateMap
    {
        public static Mapsui.Map CreateMap3857()
        {
            var map = new Mapsui.Map()
            {
                CRS = "EPSG:3857", // The Map CRS needs to be set
                BackColor = Color.Gray
            };

            return map;
        }

        public static Mapsui.Map CreateOfflineMap(string path)
        {
            var map = new Mapsui.Map()
            {
                CRS = "EPSG:3857", // The Map CRS needs to be set
                BackColor = Color.Gray
            };

            var tileSource = new MbTilesTileSource(new SQLiteConnectionString(path));
            map.Layers.Add(new TileLayer(tileSource) { Name = "MbTiles" });

            return map;
        }
    }
}
