using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using System.Device.Location;

namespace what3pass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NewEntryWindow : Window
    {
        private double _latitude;
        private double _longitude;
        private GeoCoordinateWatcher _geoCoordinateWatcher;
        private GeoCoordinate _geoCoordinate;
        
        public NewEntryWindow()
        {
            InitializeComponent();

            _geoCoordinateWatcher = new GeoCoordinateWatcher();
            _geoCoordinateWatcher.Start();

            _geoCoordinateWatcher.PositionChanged += (sender, e) =>
            {
                _geoCoordinate = e.Position.Location;
                _latitude = _geoCoordinate.Latitude;
                _longitude = _geoCoordinate.Longitude;

                var coordinate = e.Position.Location;
                Console.WriteLine("Lat: {0}, Long: {1}", coordinate.Latitude,
                    coordinate.Longitude);
            };
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            mapView.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 17;
            mapView.Zoom = 2;
            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            mapView.DragButton = MouseButton.Left;
            mapView.CanDragMap = true;
            mapView.Position = new PointLatLng(_latitude, _longitude);
        }
    }
}
