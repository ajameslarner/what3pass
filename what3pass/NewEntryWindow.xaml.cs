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
using what3words.dotnet.wrapper;
using what3words.dotnet.wrapper.models;

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
        private Timer _OnTickEvent;
        private bool _gridActive;
        private string _mapProvider;
        private PointLatLng _localLatLong;
        private What3WordsV3 _what3wordsAPIWrapper;
        private string _currentEntryStage;

        public NewEntryWindow()
        {
            InitializeComponent();

            _geoCoordinateWatcher = new GeoCoordinateWatcher();
            _geoCoordinateWatcher.Start();

            _geoCoordinate = new GeoCoordinate();
            
            _geoCoordinateWatcher.PositionChanged += (sender, e) =>
            {
                _geoCoordinate.HorizontalAccuracy = 10.0;
                _geoCoordinate = e.Position.Location;
                _latitude = _geoCoordinate.Latitude;
                _longitude = _geoCoordinate.Longitude;
            };

            grd_gridoverlay.Visibility = Visibility.Hidden;
            grd_newentry_begin.Visibility = Visibility.Visible;
            grd_mappanel.Visibility = Visibility.Visible;
            btn_newentry_back.IsEnabled = false;

            _gridActive = false;
            _mapProvider = "Default";
            _currentEntryStage = "Begin";

            _what3wordsAPIWrapper = new What3WordsV3("E3ZZ4IN2");

            //SetOnTickEvent(500);
        }

        private void SetOnTickEvent(int interval)
        {
            _OnTickEvent = new Timer(interval);
            _OnTickEvent.Elapsed += OnTimedEvent;
            _OnTickEvent.AutoReset = true;
            _OnTickEvent.Enabled = true;
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            mapView.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 21;
            mapView.Zoom = 2;
            mapView.ShowCenter = true;
            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            mapView.DragButton = MouseButton.Left;
            mapView.CanDragMap = true;
        }

        private void btn_location_Click(object sender, RoutedEventArgs e)
        {
            mapView.Position = new PointLatLng(_latitude, _longitude);
            mapView.Zoom = 21;
        }

        private void btn_newentry_next_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentEntryStage)
            {
                case "Begin":
                    grd_newentry_begin.Visibility = Visibility.Hidden;
                    grd_newentry_data.Visibility = Visibility.Visible;
                    grd_newentry_map.Visibility = Visibility.Hidden;
                    grd_newentry_processing.Visibility = Visibility.Hidden;
                    _currentEntryStage = "Data";
                    btn_newentry_back.IsEnabled = true;
                    break;
                case "Data":
                    grd_newentry_begin.Visibility = Visibility.Hidden;
                    grd_newentry_data.Visibility = Visibility.Hidden;
                    grd_newentry_map.Visibility = Visibility.Visible;
                    grd_newentry_processing.Visibility = Visibility.Hidden;
                    _currentEntryStage = "Map";
                    btn_newentry_next.Content = "Finish";
                    btn_gridoverlay.IsEnabled = true;
                    btn_mapsquare.IsEnabled = true;
                    btn_mapcircle.IsEnabled = true;
                    break;
                case "Map":
                    grd_newentry_begin.Visibility = Visibility.Hidden;
                    grd_newentry_data.Visibility = Visibility.Hidden;
                    grd_newentry_map.Visibility = Visibility.Hidden;
                    grd_newentry_processing.Visibility = Visibility.Visible;
                    _currentEntryStage = "Processing";
                    btn_newentry_next.IsEnabled = false;
                    btn_newentry_back.Visibility = Visibility.Hidden;
                    btn_newentry_next.Visibility = Visibility.Hidden;
                    btn_gridoverlay.IsEnabled = false;
                    btn_mapsquare.IsEnabled = false;
                    btn_mapcircle.IsEnabled = false;
                    btn_location.IsEnabled = false;
                    btn_gridoverlay.IsEnabled = false;
                    btn_maptype.IsEnabled = false;
                    btn_mapminus.IsEnabled = false;
                    btn_mapplus.IsEnabled = false;
                    btn_mapsquare.IsEnabled = false;
                    btn_mapcircle.IsEnabled = false;
                    mapView.CanDragMap = false;
                    grd_gridoverlay.IsEnabled = false;
                    break;
                case "Processing":
                    
                    //Start Encryption
                    break;
            }
        }

        private void btn_newentry_back_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentEntryStage)
            {
                case "Begin":
                    break;
                case "Data":
                    grd_newentry_begin.Visibility = Visibility.Visible;
                    grd_newentry_data.Visibility = Visibility.Hidden;
                    grd_newentry_map.Visibility = Visibility.Hidden;
                    grd_newentry_processing.Visibility = Visibility.Hidden;
                    btn_newentry_back.IsEnabled = false;
                    _currentEntryStage = "Begin";
                    break;
                case "Map":
                    grd_newentry_begin.Visibility = Visibility.Hidden;
                    grd_newentry_data.Visibility = Visibility.Visible;
                    grd_newentry_map.Visibility = Visibility.Hidden;
                    grd_newentry_processing.Visibility = Visibility.Hidden;
                    _currentEntryStage = "Data";
                    btn_newentry_next.Content = "Next";
                    btn_gridoverlay.IsEnabled = false;
                    btn_mapsquare.IsEnabled = false;
                    btn_mapcircle.IsEnabled = false;
                    break;
                case "Processing":
                    grd_newentry_begin.Visibility = Visibility.Hidden;
                    grd_newentry_data.Visibility = Visibility.Hidden;
                    grd_newentry_map.Visibility = Visibility.Visible;
                    grd_newentry_processing.Visibility = Visibility.Hidden;
                    _currentEntryStage = "Map";
                    break;
            }
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
        private async void GetWhat3WordsFromPosAsync()
        {
            var indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(_localLatLong.Lat, _localLatLong.Lng)).RequestAsync();
            if (txtbox_gridwords.Text.Contains(indexResult.Data.Words))
            {
                indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(ShiftCoordinates(4.0, _localLatLong.Lat, _localLatLong.Lng, 4.0)).RequestAsync();
            }
            txtbox_gridwords.Text = txtbox_gridwords.Text + indexResult.Data.Words + Environment.NewLine;
        }

        private async void RemoveWhat3WordsFromPosAsync()
        {
            var indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(_localLatLong.Lat, _localLatLong.Lng)).RequestAsync();
            if (txtbox_gridwords.Text.Contains(indexResult.Data.Words))
            {
                string newResult = txtbox_gridwords.Text;
                txtbox_gridwords.Text = newResult.Replace(indexResult.Data.Words + Environment.NewLine, "");
            }
        }

        private Coordinates ShiftCoordinates(double distance, double Lat, double Lng, double angle)
        {
            double distanceNorth = Math.Sin(angle) * distance;
            double distanceEast = Math.Cos(angle) * distance;
            double earthRadius = 6371000;
            double newLat = Lat + (distanceNorth / earthRadius) * 180 / Math.PI;
            double newLon = Lng + (distanceEast / (earthRadius * Math.Cos(newLat * 180 / Math.PI))) * 180 / Math.PI;

            return new Coordinates(newLat, newLon);
        }

        private async void GetPosFromWhat3WordsAsync()
        {
            var indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(_localLatLong.Lat, _localLatLong.Lng)).RequestAsync();
            var indexPosition = await _what3wordsAPIWrapper.ConvertToCoordinates(indexResult.Data.Words).RequestAsync();
            mapView.Position = new PointLatLng(indexPosition.Data.Coordinates.Lat, indexPosition.Data.Coordinates.Lng);
        }
        
        #region GridButtons
        private void btn_00_Click(object sender, RoutedEventArgs e)
        {
            if (btn_00.Background == Brushes.LightBlue)
            {
                btn_00.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();

            }
            else if (btn_00.Background == Brushes.Red)
            {
                btn_00.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_01_Click(object sender, RoutedEventArgs e)
        {
            if (btn_01.Background == Brushes.LightBlue)
            {
                btn_01.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_01.Background == Brushes.Red)
            {
                btn_01.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_02_Click(object sender, RoutedEventArgs e)
        {
            if (btn_02.Background == Brushes.LightBlue)
            {
                btn_02.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_02.Background == Brushes.Red)
            {
                btn_02.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_03_Click(object sender, RoutedEventArgs e)
        {
            if (btn_03.Background == Brushes.LightBlue)
            {
                btn_03.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_03.Background == Brushes.Red)
            {
                btn_03.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_04_Click(object sender, RoutedEventArgs e)
        {
            if (btn_04.Background == Brushes.LightBlue)
            {
                btn_04.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_04.Background == Brushes.Red)
            {
                btn_04.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_10_Click(object sender, RoutedEventArgs e)
        {
            if (btn_10.Background == Brushes.LightBlue)
            {
                btn_10.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_10.Background == Brushes.Red)
            {
                btn_10.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_11_Click(object sender, RoutedEventArgs e)
        {
            if (btn_11.Background == Brushes.LightBlue)
            {
                btn_11.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_11.Background == Brushes.Red)
            {
                btn_11.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_12_Click(object sender, RoutedEventArgs e)
        {
            if (btn_12.Background == Brushes.LightBlue)
            {
                btn_12.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_12.Background == Brushes.Red)
            {
                btn_12.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_13_Click(object sender, RoutedEventArgs e)
        {
            if (btn_13.Background == Brushes.LightBlue)
            {
                btn_13.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_13.Background == Brushes.Red)
            {
                btn_13.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_14_Click(object sender, RoutedEventArgs e)
        {
            if (btn_14.Background == Brushes.LightBlue)
            {
                btn_14.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_14.Background == Brushes.Red)
            {
                btn_14.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_20_Click(object sender, RoutedEventArgs e)
        {
            if (btn_20.Background == Brushes.LightBlue)
            {
                btn_20.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_20.Background == Brushes.Red)
            {
                btn_20.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_21_Click(object sender, RoutedEventArgs e)
        {
            if (btn_21.Background == Brushes.LightBlue)
            {
                btn_21.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_21.Background == Brushes.Red)
            {
                btn_21.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_22_Click(object sender, RoutedEventArgs e)
        {
            if (btn_22.Background == Brushes.LightBlue)
            {
                btn_22.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_22.Background == Brushes.Red)
            {
                btn_22.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_23_Click(object sender, RoutedEventArgs e)
        {
            if (btn_23.Background == Brushes.LightBlue)
            {
                btn_23.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_23.Background == Brushes.Red)
            {
                btn_23.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_24_Click(object sender, RoutedEventArgs e)
        {
            if (btn_24.Background == Brushes.LightBlue)
            {
                btn_24.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_24.Background == Brushes.Red)
            {
                btn_24.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_30_Click(object sender, RoutedEventArgs e)
        {
            if (btn_30.Background == Brushes.LightBlue)
            {
                btn_30.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_30.Background == Brushes.Red)
            {
                btn_30.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_31_Click(object sender, RoutedEventArgs e)
        {
            if (btn_31.Background == Brushes.LightBlue)
            {
                btn_31.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_31.Background == Brushes.Red)
            {
                btn_31.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_32_Click(object sender, RoutedEventArgs e)
        {
            if (btn_32.Background == Brushes.LightBlue)
            {
                btn_32.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_32.Background == Brushes.Red)
            {
                btn_32.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_33_Click(object sender, RoutedEventArgs e)
        {
            if (btn_33.Background == Brushes.LightBlue)
            {
                btn_33.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_33.Background == Brushes.Red)
            {
                btn_33.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_34_Click(object sender, RoutedEventArgs e)
        {
            if (btn_34.Background == Brushes.LightBlue)
            {
                btn_34.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_34.Background == Brushes.Red)
            {
                btn_34.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_40_Click(object sender, RoutedEventArgs e)
        {
            if (btn_40.Background == Brushes.LightBlue)
            {
                btn_40.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_40.Background == Brushes.Red)
            {
                btn_40.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_41_Click(object sender, RoutedEventArgs e)
        {
            if (btn_41.Background == Brushes.LightBlue)
            {
                btn_41.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_41.Background == Brushes.Red)
            {
                btn_41.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_42_Click(object sender, RoutedEventArgs e)
        {
            if (btn_42.Background == Brushes.LightBlue)
            {
                btn_42.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_42.Background == Brushes.Red)
            {
                btn_42.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_43_Click(object sender, RoutedEventArgs e)
        {
            if (btn_43.Background == Brushes.LightBlue)
            {
                btn_43.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_43.Background == Brushes.Red)
            {
                btn_43.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        private void btn_44_Click(object sender, RoutedEventArgs e)
        {
            if (btn_44.Background == Brushes.LightBlue)
            {
                btn_44.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync();
            }
            else if (btn_44.Background == Brushes.Red)
            {
                btn_44.Background = Brushes.LightBlue;
                RemoveWhat3WordsFromPosAsync();
            }
        }

        #endregion

        private void mapView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void LatLongFromLocalEvent(object sender, MouseButtonEventArgs e)
        {
            Point pos = Mouse.GetPosition(Application.Current.MainWindow);
            _localLatLong = mapView.FromLocalToLatLng((int)pos.X, (int)pos.Y);
        }

        private void grd_gridoverlay_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(true))
            {
                AddHandler(MouseDownEvent, new MouseButtonEventHandler(LatLongFromLocalEvent), true);
                //GetPosFromWhat3WordsAsync();
                //_localLatLong = mapView.Position;
            } else if (e.NewValue.Equals(false))
            {
                RemoveHandler(MouseDownEvent, new MouseButtonEventHandler(LatLongFromLocalEvent));
                ClearGridSelections();
            }
        }
        
        private void ClearGridSelections()
        {
            if (btn_00 != null)
            {
                txtbox_gridwords.Text = "";
                btn_00.Background = Brushes.LightBlue;
                btn_01.Background = Brushes.LightBlue;
                btn_02.Background = Brushes.LightBlue;
                btn_03.Background = Brushes.LightBlue;
                btn_04.Background = Brushes.LightBlue;

                btn_10.Background = Brushes.LightBlue;
                btn_11.Background = Brushes.LightBlue;
                btn_12.Background = Brushes.LightBlue;
                btn_13.Background = Brushes.LightBlue;
                btn_14.Background = Brushes.LightBlue;

                btn_20.Background = Brushes.LightBlue;
                btn_21.Background = Brushes.LightBlue;
                btn_22.Background = Brushes.LightBlue;
                btn_23.Background = Brushes.LightBlue;
                btn_24.Background = Brushes.LightBlue;

                btn_30.Background = Brushes.LightBlue;
                btn_31.Background = Brushes.LightBlue;
                btn_32.Background = Brushes.LightBlue;
                btn_33.Background = Brushes.LightBlue;
                btn_34.Background = Brushes.LightBlue;

                btn_40.Background = Brushes.LightBlue;
                btn_41.Background = Brushes.LightBlue;
                btn_42.Background = Brushes.LightBlue;
                btn_43.Background = Brushes.LightBlue;
                btn_44.Background = Brushes.LightBlue;
            }
        }

        private void mapView_OnMapZoomChanged()
        {
            if (btn_gridoverlay.IsEnabled == true)
            {
                grd_gridoverlay.Visibility = Visibility.Hidden;
                _gridActive = false;
                grd_gridoverlay.IsEnabled = false;
            }

            if (mapView.Zoom != 21)
            {
                btn_gridoverlay.IsEnabled = false;
                btn_mapsquare.IsEnabled = false;
                btn_mapcircle.IsEnabled = false;
            }
            else if (_currentEntryStage == "Map" && mapView.Zoom == 21)
            {
                btn_gridoverlay.IsEnabled = true;
                btn_mapsquare.IsEnabled = true;
                btn_mapcircle.IsEnabled = true;
            }

            if (mapView.Zoom < 3)
            {
                btn_mapminus.IsEnabled = false;
            }
            else
            {
                btn_mapminus.IsEnabled = true;
            }

            if (mapView.Zoom == mapView.MaxZoom)
            {
                btn_mapplus.IsEnabled = false;
            }
            else
            {
                btn_mapplus.IsEnabled = true;
            }
        }

        private void btn_mapcircle_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btn_mapsquare_Click(object sender, RoutedEventArgs e)
        {
            btn_11.Background = Brushes.Red;
            btn_12.Background = Brushes.Red;
            btn_13.Background = Brushes.Red;

            btn_21.Background = Brushes.Red;
            btn_22.Background = Brushes.Red;
            btn_23.Background = Brushes.Red;

            btn_31.Background = Brushes.Red;
            btn_32.Background = Brushes.Red;
            btn_33.Background = Brushes.Red;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txt_platform_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txt_username_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
