using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Mapping;

namespace WB.Infrastructure.Shared.Enumerator.Internals.AreaEditor
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public MapViewModel()
        {
            string pathToSearch = "interviewer/maps/";
            string tpkToSearch = "*.tpk";
            string vtpkToSearch = "*.vtpk";
            
            string searchPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, pathToSearch);
            var tpkFileSearchResult = Directory.GetFiles(searchPath, tpkToSearch);
            var vtpkFileSearchResult = Directory.GetFiles(searchPath, vtpkToSearch);

            var map = new Map();

            if (vtpkFileSearchResult.Length > 0)
            {
                var layer = new ArcGISVectorTiledLayer(new Uri(vtpkFileSearchResult.First()));
                map.Basemap.BaseLayers.Add(layer);
            }

            if (tpkFileSearchResult.Length != 0)
            {
                foreach (var filename in tpkFileSearchResult)
                {
                    TileCache titleCache = new TileCache(filename);
                    var layer = new ArcGISTiledLayer(titleCache);
                    map.Basemap.BaseLayers.Add(layer);
                }
            }

            else
            {
                Console.WriteLine("no files in: " + searchPath);
                map = null;
            }

            //----

            //----
            //var serviceUri = new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");
            //map.Basemap.BaseLayers.Add(new ArcGISTiledLayer(serviceUri));
            //----

            //----
            //map = new Map(Basemap.CreateImagery());
            //-----

            
            //_map = new Map(Basemap.CreateTopographic());

            this.map = map;

        }

        private Map map;

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get { return map; }
            set { map = value; OnPropertyChanged(); }
        }


        /// <summary>
        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = PropertyChanged;
            propertyChangedHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}