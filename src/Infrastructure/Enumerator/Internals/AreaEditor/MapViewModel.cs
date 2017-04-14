using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;

namespace WB.Infrastructure.Shared.Enumerator.Internals.AreaEditor
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public MapViewModel()
        {
            
            string filename = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "interviewer/maps/chibombo_b7.tpk");

            if (!File.Exists(filename))
            {
                Console.WriteLine("no file: " + filename);
                _map = null;
            }
            else
            {
                var map = new Map();

                TileCache titleCache = new TileCache(filename);
                var layer = new ArcGISTiledLayer(titleCache);

                //layer.MaxScale = 1;

                /*ServiceFeatureTable tbl =  new ServiceFeatureTable();
                
                var featureTable = new FeatureLayer(tbl);
                map.OperationalLayers.Add(featureTable);*/

                map.Basemap.BaseLayers.Add(layer);

                /*map.MaxScale = 1;
                map.MinScale = 22;*/

                _map = map;
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

        }

        private Map _map;

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
        }


        /// <summary>
        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = PropertyChanged;
            if (propertyChangedHandler != null)
                propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}