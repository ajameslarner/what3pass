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
using GMap.NET.WindowsPresentation;
using GMap.NET.ObjectModel;
using System.Diagnostics;
using what3words.dotnet.wrapper;
using what3words.dotnet.wrapper.models;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Web.Security;
using System.IO.Ports;

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
        private System.Timers.Timer _OnTickEvent;
        private bool _gridActive;
        private string _mapProvider;
        private PointLatLng _localLatLong;
        private What3WordsV3 _what3wordsAPIWrapper;
        private string _currentEntryStage;
        private bool _GridSnap;
        private bool _GridDrawing;

        private List<Button> _ActiveButtons;
        private List<GMapMarker> _ActiveMarkers;

        private string _connectionString;

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

            _connectionString = ConfigurationManager.ConnectionStrings["W3PDB"].ConnectionString;

            _GridDrawing = false;

            _ActiveButtons = new List<Button>();
            FillButtonList();

            _ActiveMarkers = new List<GMapMarker>();
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            mapView.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 20;
            mapView.Zoom = 2;
            mapView.ShowCenter = false;
            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            mapView.DragButton = MouseButton.Left;
            mapView.CanDragMap = true;
        }

        private void btn_location_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow._GPSReceiver.IsOpen)
            {
                if (MainWindow._GPS_DATA[0] != 0 && MainWindow._GPS_DATA[1] != 0)
                {
                    mapView.Position = new PointLatLng(MainWindow._GPS_DATA[0], MainWindow._GPS_DATA[1]);
                    mapView.Zoom = 20;
                    lbl_gpsdeviceconnected.Visibility = Visibility.Visible;
                    lbl_gpsdeviceconnected.Content = "GPS Device Detected";
                    SetOnTickDeviceWarningEvent(3000);
                }
            }
            else
            {
                mapView.Position = new PointLatLng(_latitude, _longitude);
                mapView.Zoom = 20;
            }
        }

        private void SetOnTickDeviceWarningEvent(int interval)
        {
            _OnTickEvent = new System.Timers.Timer(interval);
            _OnTickEvent.Elapsed += OnTimedEventDeviceWarning;
            _OnTickEvent.AutoReset = false;
            _OnTickEvent.Enabled = true;
        }

        private void OnTimedEventDeviceWarning(object source, ElapsedEventArgs e)
        {
            lbl_gpsdeviceconnected.Dispatcher.Invoke(new Action(() =>
            {
                lbl_gpsdeviceconnected.Visibility = Visibility.Hidden;
            }));
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
                    if (MainWindow._GPSReceiver.IsOpen)
                    {
                        if (MainWindow._GPS_DATA[0] != 0 && MainWindow._GPS_DATA[1] != 0)
                        {
                            mapView.Position = new PointLatLng(MainWindow._GPS_DATA[0], MainWindow._GPS_DATA[1]);
                            mapView.Zoom = 20;
                            lbl_gpsdeviceconnected.Visibility = Visibility.Visible;
                            lbl_gpsdeviceconnected.Content = "GPS Device Detected";
                            SetOnTickDeviceWarningEvent(3000);
                        }
                    }
                    else
                    {
                        mapView.Position = new PointLatLng(_latitude, _longitude);
                        mapView.Zoom = 20;
                    }
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
                    btn_mapminus.IsEnabled = false;
                    btn_mapplus.IsEnabled = false;
                    btn_mapsquare.IsEnabled = false;
                    btn_mapcircle.IsEnabled = false;
                    mapView.CanDragMap = true;
                    grd_gridoverlay.IsEnabled = false;
                    grd_gridoverlay.Visibility = Visibility.Hidden;

                    ExecuteW3PEncryptionProcess();
                    break;
                case "Processing":
                    break;
            }
        }

        private async void ExecuteW3PEncryptionProcess()
        {
            byte[] encrypted = { (byte)0, (byte)0 };

            List<UserEntry> dataOut = new List<UserEntry>();

            string[] W3PHashes = txtbox_gridwords.Text.Split('$');
            for(int i = 1; i < W3PHashes.Length; i++)
            {
                var preProcessBytes = Encoding.ASCII.GetBytes(W3PHashes[i].Trim());

                List<byte> processingBytes = preProcessBytes.ToList();

                while (processingBytes.Count < 32)
                {
                    //Padding
                    processingBytes.Insert(0,(byte)3);
                }

                byte[] W3PKey = processingBytes.ToArray();
       
                encrypted = EncryptStringToBytes_Aes(txt_password.Password, W3PKey);
                string chiperText = Convert.ToBase64String(encrypted);

                dataOut.Add(new UserEntry(txt_platform.Text, txt_username.Text, chiperText));

            }
            await InsertUserEntryInto_DB(dataOut, MainWindow.s_currentUser.Id);
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key)
        {
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                Byte[] IVKey = Encoding.ASCII.GetBytes("COLLABORATIONISM");
                aesAlg.IV = IVKey;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        private async Task InsertUserEntryInto_DB(List<UserEntry> entryData, int userId)
        {
            foreach (var userEntry in entryData)
            {
                using (var sqlCon = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"INSERT INTO[UserEntries](platform, username, password, user_id) VALUES(@Platform, @Username, @Password, @User_id)", sqlCon))
                {
                    cmd.Parameters.Add(new SqlParameter("Platform", userEntry.Platform));
                    cmd.Parameters.Add(new SqlParameter("Username", userEntry.Username));
                    cmd.Parameters.Add(new SqlParameter("Password", userEntry.Password));
                    cmd.Parameters.Add(new SqlParameter("User_id", userId));

                    await sqlCon.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    sqlCon.Close();
                }
            }

            grd_newentry_begin.Visibility = Visibility.Hidden;
            grd_newentry_data.Visibility = Visibility.Hidden;
            grd_newentry_map.Visibility = Visibility.Hidden;
            grd_newentry_processing.Visibility = Visibility.Hidden;
            grd_newentry_processed.Visibility = Visibility.Visible;
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
        private async void GetWhat3WordsFromPosAsync(Button b)
        {
            Point relativePoint = b.TransformToAncestor(this).Transform(new Point(0, 0));
            _localLatLong = mapView.FromLocalToLatLng((int)relativePoint.X -512, (int)relativePoint.Y-25);

            var indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(_localLatLong.Lat, _localLatLong.Lng)).RequestAsync();
            
            var result = await _what3wordsAPIWrapper.ConvertToCoordinates(indexResult.Data.Words).RequestAsync();

            var marker = new GMapMarker(new PointLatLng(result.Data.Coordinates.Lat, result.Data.Coordinates.Lng));

            //ClearGridSelections();

            //if (btn_44.Background == Brushes.LightBlue)
            //{
            //    mapView.Position = new PointLatLng(result.Data.Coordinates.Lat, result.Data.Coordinates.Lng);
            //    b.Background = Brushes.LightBlue;
            //    btn_44.Background = Brushes.Red;
            //}

            txtbox_gridwords.Text = $"{txtbox_gridwords.Text}${indexResult.Data.Words}";

            if (mapView.Zoom == 20)
            {
                Rectangle recShape1 = new Rectangle
                {
                    Width = 32,
                    Height = 32,
                    Fill = Brushes.Red,
                    Opacity = 0.35
                };

                marker.Shape = recShape1;
            }

            if (mapView.Zoom == 19)
            {
                Rectangle recShape2 = new Rectangle
                {
                    Width = 15,
                    Height = 15,
                    Fill = Brushes.Red,
                    Opacity = 0.35
                };

                marker.Shape = recShape2;
            }

            marker.Tag = indexResult.Data.Words;
            //marker.Offset = new Point((-recShape.Width / 2)-1, (-recShape.Height / 2)-5);
            mapView.Markers.Add(marker);

            _ActiveMarkers.Add(marker);
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
        
        private void btn_gridClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Background == Brushes.LightBlue)
            {
                button.Background = Brushes.Red;
                GetWhat3WordsFromPosAsync((Button)sender);

            }
            //else if (btn_00.Background == Brushes.Red)
            //{
            //    btn_00.Background = Brushes.LightBlue;
            //    RemoveWhat3WordsFromPosAsync((Button)sender);
            //}
        }
        private void mapView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void LatLongFromLocalEvent(object sender, MouseButtonEventArgs e)
        {
            //Point pos = Mouse.GetPosition(Application.Current.MainWindow);
            //_localLatLong = mapView.FromLocalToLatLng((int)pos.X-200, (int)pos.Y+50);

            //GetWhat3WordsFromPosAsync();

            //var marker = new GMapMarker(new PointLatLng(_localLatLong.Lat, _localLatLong.Lng));

            //Rectangle recShape = new Rectangle
            //{
            //    Width = 50,
            //    Height = 50,
            //    Fill = Brushes.Red,
            //    Opacity = 0.35

            //};
            //marker.Shape = recShape;
            //marker.Tag = "PolyDot";
            //marker.Offset = new Point(-recShape.Width / 2, -recShape.Height / 2);
            //mapView.Markers.Add(marker);
        }

        private void grd_gridoverlay_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (e.NewValue.Equals(true))
            //{
            //    AddHandler(MouseDownEvent, new MouseButtonEventHandler(LatLongFromLocalEvent), true);
            //    //GetPosFromWhat3WordsAsync();
            //    //_localLatLong = mapView.Position;
            //}
            //} else if (e.NewValue.Equals(false))
            //{
            //    RemoveHandler(MouseDownEvent, new MouseButtonEventHandler(LatLongFromLocalEvent));
            //    ClearGridSelections();
            //}
        }
        
        private void ClearGridSelections()
        {
            btn_00.Background = Brushes.LightBlue;
            btn_01.Background = Brushes.LightBlue;
            btn_02.Background = Brushes.LightBlue;
            btn_03.Background = Brushes.LightBlue;
            btn_04.Background = Brushes.LightBlue;
            btn_05.Background = Brushes.LightBlue;
            btn_06.Background = Brushes.LightBlue;
            btn_07.Background = Brushes.LightBlue;
            btn_08.Background = Brushes.LightBlue;

            btn_10.Background = Brushes.LightBlue;
            btn_11.Background = Brushes.LightBlue;
            btn_12.Background = Brushes.LightBlue;
            btn_13.Background = Brushes.LightBlue;
            btn_14.Background = Brushes.LightBlue;
            btn_15.Background = Brushes.LightBlue;
            btn_16.Background = Brushes.LightBlue;
            btn_17.Background = Brushes.LightBlue;
            btn_18.Background = Brushes.LightBlue;

            btn_20.Background = Brushes.LightBlue;
            btn_21.Background = Brushes.LightBlue;
            btn_22.Background = Brushes.LightBlue;
            btn_23.Background = Brushes.LightBlue;
            btn_24.Background = Brushes.LightBlue;
            btn_25.Background = Brushes.LightBlue;
            btn_26.Background = Brushes.LightBlue;
            btn_27.Background = Brushes.LightBlue;
            btn_28.Background = Brushes.LightBlue;

            btn_30.Background = Brushes.LightBlue;
            btn_31.Background = Brushes.LightBlue;
            btn_32.Background = Brushes.LightBlue;
            btn_33.Background = Brushes.LightBlue;
            btn_34.Background = Brushes.LightBlue;
            btn_35.Background = Brushes.LightBlue;
            btn_36.Background = Brushes.LightBlue;
            btn_37.Background = Brushes.LightBlue;
            btn_38.Background = Brushes.LightBlue;

            btn_40.Background = Brushes.LightBlue;
            btn_41.Background = Brushes.LightBlue;
            btn_42.Background = Brushes.LightBlue;
            btn_43.Background = Brushes.LightBlue;
            btn_44.Background = Brushes.LightBlue;
            btn_45.Background = Brushes.LightBlue;
            btn_46.Background = Brushes.LightBlue;
            btn_47.Background = Brushes.LightBlue;
            btn_48.Background = Brushes.LightBlue;

            btn_50.Background = Brushes.LightBlue;
            btn_51.Background = Brushes.LightBlue;
            btn_52.Background = Brushes.LightBlue;
            btn_53.Background = Brushes.LightBlue;
            btn_54.Background = Brushes.LightBlue;
            btn_55.Background = Brushes.LightBlue;
            btn_56.Background = Brushes.LightBlue;
            btn_57.Background = Brushes.LightBlue;
            btn_58.Background = Brushes.LightBlue;

            btn_60.Background = Brushes.LightBlue;
            btn_61.Background = Brushes.LightBlue;
            btn_62.Background = Brushes.LightBlue;
            btn_63.Background = Brushes.LightBlue;
            btn_64.Background = Brushes.LightBlue;
            btn_65.Background = Brushes.LightBlue;
            btn_66.Background = Brushes.LightBlue;
            btn_67.Background = Brushes.LightBlue;
            btn_68.Background = Brushes.LightBlue;

            btn_70.Background = Brushes.LightBlue;
            btn_71.Background = Brushes.LightBlue;
            btn_72.Background = Brushes.LightBlue;
            btn_73.Background = Brushes.LightBlue;
            btn_74.Background = Brushes.LightBlue;
            btn_75.Background = Brushes.LightBlue;
            btn_76.Background = Brushes.LightBlue;
            btn_77.Background = Brushes.LightBlue;
            btn_78.Background = Brushes.LightBlue;

            btn_80.Background = Brushes.LightBlue;
            btn_81.Background = Brushes.LightBlue;
            btn_82.Background = Brushes.LightBlue;
            btn_83.Background = Brushes.LightBlue;
            btn_84.Background = Brushes.LightBlue;
            btn_85.Background = Brushes.LightBlue;
            btn_86.Background = Brushes.LightBlue;
            btn_87.Background = Brushes.LightBlue;
            btn_88.Background = Brushes.LightBlue;
        }

        private void FillButtonList()
        {
            _ActiveButtons.Add(btn_00);
            _ActiveButtons.Add(btn_01);
            _ActiveButtons.Add(btn_02);
            _ActiveButtons.Add(btn_03);
            _ActiveButtons.Add(btn_04);
            _ActiveButtons.Add(btn_05);
            _ActiveButtons.Add(btn_06);
            _ActiveButtons.Add(btn_07);
            _ActiveButtons.Add(btn_08);
            _ActiveButtons.Add(btn_10);
            _ActiveButtons.Add(btn_11);
            _ActiveButtons.Add(btn_12);
            _ActiveButtons.Add(btn_13);
            _ActiveButtons.Add(btn_14);
            _ActiveButtons.Add(btn_15);
            _ActiveButtons.Add(btn_16);
            _ActiveButtons.Add(btn_17);
            _ActiveButtons.Add(btn_18);
            _ActiveButtons.Add(btn_20);
            _ActiveButtons.Add(btn_21);
            _ActiveButtons.Add(btn_22);
            _ActiveButtons.Add(btn_23);
            _ActiveButtons.Add(btn_24);
            _ActiveButtons.Add(btn_25);
            _ActiveButtons.Add(btn_26);
            _ActiveButtons.Add(btn_27);
            _ActiveButtons.Add(btn_28);
            _ActiveButtons.Add(btn_30);
            _ActiveButtons.Add(btn_31);
            _ActiveButtons.Add(btn_32);
            _ActiveButtons.Add(btn_33);
            _ActiveButtons.Add(btn_34);
            _ActiveButtons.Add(btn_35);
            _ActiveButtons.Add(btn_36);
            _ActiveButtons.Add(btn_37);
            _ActiveButtons.Add(btn_38);
            _ActiveButtons.Add(btn_40);
            _ActiveButtons.Add(btn_41);
            _ActiveButtons.Add(btn_42);
            _ActiveButtons.Add(btn_43);
            _ActiveButtons.Add(btn_44);
            _ActiveButtons.Add(btn_45);
            _ActiveButtons.Add(btn_46);
            _ActiveButtons.Add(btn_47);
            _ActiveButtons.Add(btn_48);
            _ActiveButtons.Add(btn_50);
            _ActiveButtons.Add(btn_51);
            _ActiveButtons.Add(btn_52);
            _ActiveButtons.Add(btn_53);
            _ActiveButtons.Add(btn_54);
            _ActiveButtons.Add(btn_55);
            _ActiveButtons.Add(btn_56);
            _ActiveButtons.Add(btn_57);
            _ActiveButtons.Add(btn_58);
            _ActiveButtons.Add(btn_60);
            _ActiveButtons.Add(btn_61);
            _ActiveButtons.Add(btn_62);
            _ActiveButtons.Add(btn_63);
            _ActiveButtons.Add(btn_64);
            _ActiveButtons.Add(btn_65);
            _ActiveButtons.Add(btn_66);
            _ActiveButtons.Add(btn_67);
            _ActiveButtons.Add(btn_68);
            _ActiveButtons.Add(btn_70);
            _ActiveButtons.Add(btn_71);
            _ActiveButtons.Add(btn_72);
            _ActiveButtons.Add(btn_73);
            _ActiveButtons.Add(btn_74);
            _ActiveButtons.Add(btn_75);
            _ActiveButtons.Add(btn_76);
            _ActiveButtons.Add(btn_77);
            _ActiveButtons.Add(btn_78);
            _ActiveButtons.Add(btn_80);
            _ActiveButtons.Add(btn_81);
            _ActiveButtons.Add(btn_82);
            _ActiveButtons.Add(btn_83);
            _ActiveButtons.Add(btn_84);
            _ActiveButtons.Add(btn_85);
            _ActiveButtons.Add(btn_86);
            _ActiveButtons.Add(btn_87);
            _ActiveButtons.Add(btn_88);
        }

        private void mapView_OnMapZoomChanged()
        {
            //if (btn_gridoverlay.IsEnabled == true)
            //{
            //    grd_gridoverlay.Visibility = Visibility.Hidden;
            //    _gridActive = false;
            //    grd_gridoverlay.IsEnabled = false;
            //}

            if (mapView.Zoom == 20)
            {
                foreach (var marker in _ActiveMarkers)
                {
                    Rectangle recShape = new Rectangle
                    {
                        Width = 31,
                        Height = 31,
                        Fill = Brushes.Red,
                        Opacity = 0.35
                    };

                    marker.Shape = recShape;
                }

                grd_gridoverlay.Width = 310;
                grd_gridoverlay.Height = 310;
            }

            if (mapView.Zoom == 19)
            {
                foreach (var marker in _ActiveMarkers)
                {
                    Rectangle recShape = new Rectangle
                    {
                        Width = 15,
                        Height = 15,
                        Fill = Brushes.Red,
                        Opacity = 0.35
                    };

                    marker.Shape = recShape;
                }

                grd_gridoverlay.Width = 160;
                grd_gridoverlay.Height = 160;
            }

            if (mapView.Zoom < 20)
            {
                grd_gridoverlay.Visibility = Visibility.Hidden;
                btn_gridoverlay.IsEnabled = false;
                btn_mapsquare.IsEnabled = false;
                btn_mapcircle.IsEnabled = true;
            }
            else if (_currentEntryStage == "Map" && mapView.Zoom > 19)
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
            ClearGridSelections();
            txtbox_gridwords.Text = "";
            mapView.Markers.Clear();

            lbl_gpsdeviceconnected.Content = "Map Cleared";
            lbl_gpsdeviceconnected.Visibility = Visibility.Visible;
            SetOnTickDeviceWarningEvent(3000);
        }

        private async void btn_mapsquare_Click(object sender, RoutedEventArgs e)
        {
            _GridDrawing = !_GridDrawing;
            if (_GridDrawing)
            {
                lbl_gpsdeviceconnected.Content = "Grid Drawing Enabled";
                lbl_gpsdeviceconnected.Foreground = Brushes.LightGreen;
                lbl_gpsdeviceconnected.Visibility = Visibility.Visible;
                SetOnTickDeviceWarningEvent(3000);
            }
            else
            {
                lbl_gpsdeviceconnected.Content = "Grid Drawing Disabled";
                lbl_gpsdeviceconnected.Foreground = Brushes.White;
                lbl_gpsdeviceconnected.Visibility = Visibility.Visible;
                SetOnTickDeviceWarningEvent(3000);
            }
            
            //double[] incPos = IncrementMapPosition(1.5);
            //double[] decPos = DecrementMapPosition(1.5);
            //var result = await _what3wordsAPIWrapper.GridSection(new Coordinates(incPos[0], incPos[1]), new Coordinates(decPos[0], decPos[1])).RequestAsync();

            //var marker = new GMapMarker(new PointLatLng(_latitude, _longitude));

            //Rectangle recShape = new Rectangle
            //{
            //    Width = 100,
            //    Height = 100,
            //    Fill = Brushes.Red

            //};
            //marker.Shape = recShape;
            //marker.Tag = "PolyDot";
            //marker.Offset = new Point(-recShape.Width / 2, -recShape.Height / 2);
            //mapView.Markers.Add(marker);



            //Declare List for pointlatlang
            //List<PointLatLng> pointlatlang = new List<PointLatLng>();
            //pointlatlang.Add(new PointLatLng(incPos[0], incPos[1]));
            //pointlatlang.Add(new PointLatLng(decPos[0], decPos[1]));

            ////Declare polygon in gmap
            //GMapPolygon polygon = new GMapPolygon(pointlatlang);
            //mapView.RegenerateShape(polygon);
            ////setting line style
            //(polygon.Shape as System.Windows.Shapes.Path).Stroke = Brushes.DarkBlue;
            //(polygon.Shape as System.Windows.Shapes.Path).StrokeThickness = 1.5;
            //(polygon.Shape as System.Windows.Shapes.Path).Effect = null;
            ////To add polygon in gmap
            //mapView.Markers.Add(polygon);
        }

        private double[] IncrementMapPosition(double meters)
        {
            double[] result = new double[2];
            double coef = meters * 0.0000089;

            result[0] = _latitude + coef;
            result[1] = _longitude - coef / Math.Cos(_latitude * 0.018);

            return result;
        }

        private double[] DecrementMapPosition(double meters)
        {
            double[] result = new double[2];
            double coef = meters * 0.0000089;

            result[0] = _latitude - coef;
            result[1] = _longitude + coef / Math.Cos(_latitude * 0.018);

            return result;
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

        private void btn_generatepass_Click(object sender, RoutedEventArgs e)
        {
            string genPassword = Membership.GeneratePassword(15, 2);
            txt_password.Password = genPassword;
            lbl_genpassword.Visibility = Visibility.Visible;
            Clipboard.SetText(genPassword);
            lbl_genpassword.Content = "Copied to clipboard!";
        }

        private void txt_platform_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_platform.Text == "Platform...")
            {
                txt_platform.Text = "";
                txt_platform.Foreground = Brushes.Black;
            }
        }

        private void txt_platform_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_platform.Text))
            {
                txt_platform.Text = "Platform...";
                txt_platform.Foreground = Brushes.Gray;
            }
        }

        private void txt_username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_username.Text == "Username...")
            {
                txt_username.Text = "";
                txt_username.Foreground = Brushes.Black;
            }
        }

        private void txt_username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_username.Text))
            {
                txt_username.Text = "Username...";
                txt_username.Foreground = Brushes.Gray;
            }
        }

        private void txt_password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_password.Password == "Password...")
            {
                txt_password.Password = "";
                txt_password.Foreground = Brushes.Black;
            }
        }

        private void txt_password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_password.Password))
            {
                txt_password.Password = "Password...";
                txt_password.Foreground = Brushes.Gray;
            }
        }

        private void btn_newentry_process_Click(object sender, RoutedEventArgs e)
        {
            grd_newentry_begin.Visibility = Visibility.Visible;
            grd_newentry_data.Visibility = Visibility.Hidden;
            grd_newentry_map.Visibility = Visibility.Hidden;
            grd_newentry_processing.Visibility = Visibility.Hidden;
            grd_newentry_processed.Visibility = Visibility.Hidden;
            btn_newentry_back.IsEnabled = false;
            _currentEntryStage = "Begin";

            txt_username.Text = "Username...";
            txt_password.Password = "Password...";
            txt_platform.Text = "Platform...";

            txtbox_gridwords.Text = "";

            btn_newentry_next.IsEnabled = true;
            btn_newentry_next.Content = "Next";
            btn_newentry_back.Visibility = Visibility.Visible;
            btn_newentry_next.Visibility = Visibility.Visible;

            lbl_genpassword.Visibility = Visibility.Hidden;

            ClearGridSelections();
            txtbox_gridwords.Text = "";
            mapView.Markers.Clear();

            lbl_gpsdeviceconnected.Content = "Map Cleared";
            lbl_gpsdeviceconnected.Visibility = Visibility.Visible;
            SetOnTickDeviceWarningEvent(3000);

            grd_gridoverlay.Visibility = Visibility.Hidden;
            mapView.Zoom = 2;

            btn_gridoverlay.IsEnabled = false;
            btn_mapsquare.IsEnabled = false;
            btn_mapcircle.IsEnabled = false;
            btn_gridoverlay.IsEnabled = false;
            grd_gridoverlay.IsEnabled = false;
        }

        private void mapView_OnMapDrag()
        {
            ClearGridSelections();
            _GridSnap = true;
        }

        private async void mapView_OnPositionChanged(PointLatLng point)
        {

        }

        private async void mapView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (grd_gridoverlay.Visibility == Visibility.Visible)
            {
                Point relativePoint = btn_44.TransformToAncestor(this).Transform(new Point(0, 0));
                _localLatLong = mapView.FromLocalToLatLng((int)relativePoint.X - 512, (int)relativePoint.Y - 25);

                var indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(_localLatLong.Lat, _localLatLong.Lng)).RequestAsync();

                var result = await _what3wordsAPIWrapper.ConvertToCoordinates(indexResult.Data.Words).RequestAsync();

                mapView.Position = new PointLatLng(result.Data.Coordinates.Lat, result.Data.Coordinates.Lng);

                await GetAddressesParallelAsync();
            }
        }

        private async Task GetAddressesParallelAsync()
        {
            List<Task<what3words.dotnet.wrapper.response.APIResponse<Address>>> tasks = new List<Task<what3words.dotnet.wrapper.response.APIResponse<Address>>>();

            if (_ActiveButtons != null)
            {
                foreach (var btn in _ActiveButtons)
                {
                    btn.IsEnabled = false;
                }

                foreach (var btn in _ActiveButtons)
                {
                    Point point = btn.TransformToAncestor(this).Transform(new Point(0, 0));
                    var localLatLng = mapView.FromLocalToLatLng((int)point.X - 512, (int)point.Y - 25);

                    var result = await Task.Run(() => GetAddressesAsync(new Coordinates(localLatLng.Lat, localLatLng.Lng)));

                    if (txtbox_gridwords.Text.Contains(result.Data.Words))
                    {
                        btn.Background = Brushes.Red;
                    }
                    btn.IsEnabled = true;
                }
            }
        }

        private async Task<what3words.dotnet.wrapper.response.APIResponse<Address>> GetAddressesAsync(Coordinates coords)
        {
            return await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(coords.Lat, coords.Lng)).RequestAsync();
        }

        private void btn_MouseEnterGlobal(object sender, MouseEventArgs e)
        {
            if (_GridDrawing)
            {
                Button button = (Button)sender;
                if (button.Background == Brushes.LightBlue)
                {
                    button.Background = Brushes.Red;
                    GetWhat3WordsFromPosAsync((Button)sender);

                }
            }
        }
    }
}
