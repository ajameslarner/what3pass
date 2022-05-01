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
using System.Device.Location;
using System.Timers;
//Map Package
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;

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
        private Timer _GPSTimer;

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
            };
        }
        private void SetGPSUpdateTimer(int interval)
        {
            _GPSTimer = new Timer(interval);
            _GPSTimer.Elapsed += OnTimedEvent;
            _GPSTimer.AutoReset = true;
            _GPSTimer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                              e.SignalTime);
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            mapView.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 20;
            mapView.Zoom = 2;
            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            mapView.DragButton = MouseButton.Left;
            mapView.CanDragMap = true;
        }

        private void btn_location_Click(object sender, RoutedEventArgs e)
        {
            mapView.Position = new PointLatLng(_latitude, _longitude);
            mapView.Zoom = 20;
        }
    }
}
