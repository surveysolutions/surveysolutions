using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Mapping;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public MapViewModel(string pathToMap)
        {
            if (pathToMap != null)
            {
                var map = new Map();
                TileCache titleCache = new TileCache(pathToMap);
                var layer = new ArcGISTiledLayer(titleCache);
                map.Basemap.BaseLayers.Add(layer);
                
                this.map = map;
            }
            else
            {
                this.map = null;
            }
            
            //var serviceUri = new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");
            //map.Basemap.BaseLayers.Add(new ArcGISTiledLayer(serviceUri));
            //----

            //----
            //map = new Map(Basemap.CreateImagery());
            //-----


            //_map = new Map(Basemap.CreateTopographic());
        }

        private Map map;

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get { return this.map; }
            set { this.map = value; this.OnPropertyChanged(); }
        }


        private Dictionary<string, string> mapList = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Dictionary<string, string> MapList
        {
            get { return this.mapList; }
            set { this.mapList = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = this.PropertyChanged;
            propertyChangedHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}