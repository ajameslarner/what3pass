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
using System.Diagnostics;

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
        private bool _gridActive;
        private string _mapProvider;
        private PointLatLng _localLatLong;

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

            grd_gridoverlay.Visibility = Visibility.Hidden;
            _gridActive = false;
            _mapProvider = "Default";
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

        private Point downPoint;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            downPoint = e.GetPosition(this);
            Console.WriteLine(downPoint.X);
        }

        private void btn_location_Click(object sender, RoutedEventArgs e)
        {
            mapView.Position = new PointLatLng(_latitude, _longitude);
            mapView.Zoom = 20;

            
        }

        private void btn_newentry_start_Click(object sender, RoutedEventArgs e)
        {
            grd_newentry_begin.Visibility = Visibility.Hidden;
            grd_newentry_data.Visibility = Visibility.Visible;
        }

        private void btn_gridoverlay_Click(object sender, RoutedEventArgs e)
        {
            switch (_gridActive)
            {
                case true:
                    grd_gridoverlay.Visibility = Visibility.Hidden;
                    _gridActive = false;
                    grd_gridoverlay.IsEnabled = false;
                    break;
                case false:
                    grd_gridoverlay.Visibility = Visibility.Visible;
                    _gridActive = true;
                    grd_gridoverlay.IsEnabled = true;
                    break;
            }
        }

        private void btn_maptype_Click(object sender, RoutedEventArgs e)
        {
            switch (_mapProvider)
            {
                case "Default":
                    mapView.MapProvider = GMapProviders.GoogleHybridMap;
                    _mapProvider = "Hybrid";
                    break;
                case "Hybrid":
                    mapView.MapProvider = GMapProviders.GoogleMap;
                    _mapProvider = "Default";
                    break;
            }
        }

        private void btn_mapplus_Click(object sender, RoutedEventArgs e)
        {
            mapView.Zoom++;
        }

        private void btn_mapminus_Click(object sender, RoutedEventArgs e)
        {
            mapView.Zoom--;
        }

        #region GridButtons
        private void btn_00_Click(object sender, RoutedEventArgs e)
        {
            if (btn_00.Background == Brushes.LightBlue)
            {
                btn_00.Background = Brushes.Red;
            }
            else if (btn_00.Background == Brushes.Red)
            {
                btn_00.Background = Brushes.LightBlue;
            }
        }

        private void btn_01_Click(object sender, RoutedEventArgs e)
        {
            if (btn_01.Background == Brushes.LightBlue)
            {
                btn_01.Background = Brushes.Red;
            }
            else if (btn_01.Background == Brushes.Red)
            {
                btn_01.Background = Brushes.LightBlue;
            }
        }

        private void btn_02_Click(object sender, RoutedEventArgs e)
        {
            if (btn_02.Background == Brushes.LightBlue)
            {
                btn_02.Background = Brushes.Red;
            }
            else if (btn_02.Background == Brushes.Red)
            {
                btn_02.Background = Brushes.LightBlue;
            }
        }

        private void btn_03_Click(object sender, RoutedEventArgs e)
        {
            if (btn_03.Background == Brushes.LightBlue)
            {
                btn_03.Background = Brushes.Red;
            }
            else if (btn_03.Background == Brushes.Red)
            {
                btn_03.Background = Brushes.LightBlue;
            }
        }

        private void btn_04_Click(object sender, RoutedEventArgs e)
        {
            if (btn_04.Background == Brushes.LightBlue)
            {
                btn_04.Background = Brushes.Red;
            }
            else if (btn_04.Background == Brushes.Red)
            {
                btn_04.Background = Brushes.LightBlue;
            }
        }

        private void btn_10_Click(object sender, RoutedEventArgs e)
        {
            if (btn_10.Background == Brushes.LightBlue)
            {
                btn_10.Background = Brushes.Red;
            }
            else if (btn_10.Background == Brushes.Red)
            {
                btn_10.Background = Brushes.LightBlue;
            }
        }

        private void btn_11_Click(object sender, RoutedEventArgs e)
        {
            if (btn_11.Background == Brushes.LightBlue)
            {
                btn_11.Background = Brushes.Red;
            }
            else if (btn_11.Background == Brushes.Red)
            {
                btn_11.Background = Brushes.LightBlue;
            }
        }

        private void btn_12_Click(object sender, RoutedEventArgs e)
        {
            if (btn_12.Background == Brushes.LightBlue)
            {
                btn_12.Background = Brushes.Red;
            }
            else if (btn_12.Background == Brushes.Red)
            {
                btn_12.Background = Brushes.LightBlue;
            }
        }

        private void btn_13_Click(object sender, RoutedEventArgs e)
        {
            if (btn_13.Background == Brushes.LightBlue)
            {
                btn_13.Background = Brushes.Red;
            }
            else if (btn_13.Background == Brushes.Red)
            {
                btn_13.Background = Brushes.LightBlue;
            }
        }

        private void btn_14_Click(object sender, RoutedEventArgs e)
        {
            if (btn_14.Background == Brushes.LightBlue)
            {
                btn_14.Background = Brushes.Red;
            }
            else if (btn_14.Background == Brushes.Red)
            {
                btn_14.Background = Brushes.LightBlue;
            }
        }

        private void btn_20_Click(object sender, RoutedEventArgs e)
        {
            if (btn_20.Background == Brushes.LightBlue)
            {
                btn_20.Background = Brushes.Red;
            }
            else if (btn_20.Background == Brushes.Red)
            {
                btn_20.Background = Brushes.LightBlue;
            }
        }

        private void btn_21_Click(object sender, RoutedEventArgs e)
        {
            if (btn_21.Background == Brushes.LightBlue)
            {
                btn_21.Background = Brushes.Red;
            }
            else if (btn_21.Background == Brushes.Red)
            {
                btn_21.Background = Brushes.LightBlue;
            }
        }

        private void btn_22_Click(object sender, RoutedEventArgs e)
        {
            if (btn_22.Background == Brushes.LightBlue)
            {
                btn_22.Background = Brushes.Red;
            }
            else if (btn_22.Background == Brushes.Red)
            {
                btn_22.Background = Brushes.LightBlue;
            }
        }

        private void btn_23_Click(object sender, RoutedEventArgs e)
        {
            if (btn_23.Background == Brushes.LightBlue)
            {
                btn_23.Background = Brushes.Red;
            }
            else if (btn_23.Background == Brushes.Red)
            {
                btn_23.Background = Brushes.LightBlue;
            }
        }

        private void btn_24_Click(object sender, RoutedEventArgs e)
        {
            if (btn_24.Background == Brushes.LightBlue)
            {
                btn_24.Background = Brushes.Red;
            }
            else if (btn_24.Background == Brushes.Red)
            {
                btn_24.Background = Brushes.LightBlue;
            }
        }

        private void btn_30_Click(object sender, RoutedEventArgs e)
        {
            if (btn_30.Background == Brushes.LightBlue)
            {
                btn_30.Background = Brushes.Red;
            }
            else if (btn_30.Background == Brushes.Red)
            {
                btn_30.Background = Brushes.LightBlue;
            }
        }

        private void btn_31_Click(object sender, RoutedEventArgs e)
        {
            if (btn_31.Background == Brushes.LightBlue)
            {
                btn_31.Background = Brushes.Red;
            }
            else if (btn_31.Background == Brushes.Red)
            {
                btn_31.Background = Brushes.LightBlue;
            }
        }

        private void btn_32_Click(object sender, RoutedEventArgs e)
        {
            if (btn_32.Background == Brushes.LightBlue)
            {
                btn_32.Background = Brushes.Red;
            }
            else if (btn_32.Background == Brushes.Red)
            {
                btn_32.Background = Brushes.LightBlue;
            }
        }

        private void btn_33_Click(object sender, RoutedEventArgs e)
        {
            if (btn_33.Background == Brushes.LightBlue)
            {
                btn_33.Background = Brushes.Red;
            }
            else if (btn_33.Background == Brushes.Red)
            {
                btn_33.Background = Brushes.LightBlue;
            }
        }

        private void btn_34_Click(object sender, RoutedEventArgs e)
        {
            if (btn_34.Background == Brushes.LightBlue)
            {
                btn_34.Background = Brushes.Red;
            }
            else if (btn_34.Background == Brushes.Red)
            {
                btn_34.Background = Brushes.LightBlue;
            }
        }

        private void btn_40_Click(object sender, RoutedEventArgs e)
        {
            if (btn_40.Background == Brushes.LightBlue)
            {
                btn_40.Background = Brushes.Red;
            }
            else if (btn_40.Background == Brushes.Red)
            {
                btn_40.Background = Brushes.LightBlue;
            }
        }

        private void btn_41_Click(object sender, RoutedEventArgs e)
        {
            if (btn_41.Background == Brushes.LightBlue)
            {
                btn_41.Background = Brushes.Red;
            }
            else if (btn_41.Background == Brushes.Red)
            {
                btn_41.Background = Brushes.LightBlue;
            }
        }

        private void btn_42_Click(object sender, RoutedEventArgs e)
        {
            if (btn_42.Background == Brushes.LightBlue)
            {
                btn_42.Background = Brushes.Red;
            }
            else if (btn_42.Background == Brushes.Red)
            {
                btn_42.Background = Brushes.LightBlue;
            }
        }

        private void btn_43_Click(object sender, RoutedEventArgs e)
        {
            if (btn_43.Background == Brushes.LightBlue)
            {
                btn_43.Background = Brushes.Red;
            }
            else if (btn_43.Background == Brushes.Red)
            {
                btn_43.Background = Brushes.LightBlue;
            }
        }

        private void btn_44_Click(object sender, RoutedEventArgs e)
        {
            if (btn_44.Background == Brushes.LightBlue)
            {
                btn_44.Background = Brushes.Red;
            }
            else if (btn_44.Background == Brushes.Red)
            {
                btn_44.Background = Brushes.LightBlue;
            }
        }

        #endregion

        private void mapView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void LatLongFromLocalEvent(object sender, MouseButtonEventArgs e)
        {
            Point pos = Mouse.GetPosition(Application.Current.MainWindow);
            Console.WriteLine(pos.X);
            Trace.WriteLine(pos.Y);
            _localLatLong = mapView.FromLocalToLatLng((int)pos.X, (int)pos.Y);
            Console.WriteLine(_localLatLong);
        }

        private void grd_gridoverlay_Loaded(object sender, RoutedEventArgs e)
        {
            AddHandler(MouseDownEvent, new MouseButtonEventHandler(LatLongFromLocalEvent), true);
        }
    }
}
