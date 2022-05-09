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
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Markup;
using System.Security.Cryptography;
using System.IO;
using what3words.dotnet.wrapper;
using what3words.dotnet.wrapper.models;

namespace what3pass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ViewEntryWindow : Window
    {
        private SqlConnection _connection;
        private Dictionary<string, Dictionary<string, string>> _EntryData;
        private What3WordsV3 _what3wordsAPIWrapper;

        private double _latitude;
        private double _longitude;
        private PointLatLng _localLatLong;
        private GeoCoordinateWatcher _geoCoordinateWatcher;
        private GeoCoordinate _geoCoordinate;

        private string _what3wordsGridPosition;

        private Timer _OnTickEvent;

        public ViewEntryWindow()
        {
            InitializeComponent();

            var connectionString = ConfigurationManager.ConnectionStrings["W3PDB"].ConnectionString;

            _connection = new SqlConnection(connectionString: connectionString);

            _EntryData = new Dictionary<string, Dictionary<string, string>>();

            _what3wordsAPIWrapper = new What3WordsV3("E3ZZ4IN2");

            _geoCoordinateWatcher = new GeoCoordinateWatcher();
            _geoCoordinateWatcher.Start();

            _geoCoordinate = new GeoCoordinate();

            _geoCoordinateWatcher.PositionChanged += async (sender, e) =>
            {
                _geoCoordinate.HorizontalAccuracy = 10.0;
                _geoCoordinate = e.Position.Location;
                _latitude = _geoCoordinate.Latitude;
                _longitude = _geoCoordinate.Longitude;
                lbl_what3words_position.Content = $@"\\\{await GetWhat3WordsFromPosAsync(_latitude, _longitude)}";
            };
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            gif_loading.Visibility = Visibility.Visible;
            pnl_parentdataviewer.Margin = new Thickness(0, 0, 0, 50);
            PopulateDataEntries_DB();
        }

        private async void PopulateDataEntries_DB()
        {
            int id = -1;
            string platform = "";
            string username = "";
            string password = "";

            SqlCommand cmd = new SqlCommand("SELECT * FROM [UserEntries] WHERE user_id = @user_id;", _connection);
            cmd.Parameters.Add(new SqlParameter("user_id", MainWindow.s_currentUser.Id));

            await _connection.OpenAsync();

            Dictionary<string, string> items;

            SqlDataReader dataReader = await cmd.ExecuteReaderAsync();

            while (dataReader.Read())
            {
                id = (int)dataReader["Id"];
                platform = (string)dataReader["platform"];
                username = (string)dataReader["username"];
                password = (string)dataReader["password"];

                if (_EntryData.TryGetValue(platform, out items))
                {
                    //Need to find a way to concatenate the passwords associated with that user inside the dictionary
                    //localPassword = items[username];
                    string localPassword = "";
                    bool isDataPresent = items.TryGetValue(username, out localPassword);
                    if (isDataPresent)
                    {
                        items[username] = $"{localPassword}${password}";
                    }
                    else
                    {
                        items[username] = password;
                    }

                }
                else
                {
                    _EntryData[platform] = new Dictionary<string, string> { [username] = password };
                }
            }

            _connection.Close();

            if (id != -1)
            {
                foreach (var item in _EntryData)
                {
                    Dictionary<string, string> LocalDictionary = item.Value;

                    string passDataItem = string.Empty;
                    string userDataItem = string.Empty;

                    foreach (var subitem in LocalDictionary)
                    {
                        userDataItem = subitem.Key;
                        passDataItem = subitem.Value;
                    }

                    string[] vs = passDataItem.Split('$');

                    Border dataView = new Border
                    {
                        Name = $"lbl_dataentry_{item.Key}_entry",
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        Width = 200,
                        Height = 180,
                        Margin = new Thickness(5, 5, 5, 5)
                    };
                    pnl_parentdataviewer.Children.Add(dataView);

                    StackPanel stackPanel = new StackPanel();
                    dataView.Child = stackPanel;

                    Label platformLabel = new Label
                    {
                        Name = $"lbl_platform_{item.Key}_entry",
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = 16,
                        Content = "Platform: "+item.Key
                    };
                    stackPanel.Children.Add(platformLabel);

                    Label usernameLabel = new Label
                    {
                        Name = $"lbl_username_{item.Key}_entry",
                        Margin = new Thickness(10, 10, 10, 10),
                        Content = "Username: "+userDataItem,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center
                    };
                    stackPanel.Children.Add(usernameLabel);

                    Label safezonesLabel = new Label
                    {
                        Name = $"lbl_safezones_{item.Key}_entry",
                        Margin = new Thickness(10, 10, 10, 10),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalContentAlignment= HorizontalAlignment.Center,
                        Content = $"Safe-zones available: {vs.Length}",
                        Foreground = Brushes.Green
                    };
                    stackPanel.Children.Add(safezonesLabel);

                    Button uncoverButton = new Button
                    {
                        Name = $"btn_unlock_{item.Key}_entry",
                        Height = 25,
                        Width = 135,
                        Margin = new Thickness(25, 0, 25, 25),
                        Content = "******"
                    };

                    uncoverButton.Click += new RoutedEventHandler(btn_unlock_entry_Click);
                    uncoverButton.MouseEnter += new MouseEventHandler(btn_unlock_entry_MouseEnter);
                    uncoverButton.MouseLeave += new MouseEventHandler(btn_unlock_entry_MouseLeave);

                    stackPanel.Children.Add(uncoverButton);
                }

                gif_loading.Visibility = Visibility.Hidden;
            } 
            else
            {
                //Label no entries found - TODO
                gif_loading.Visibility = Visibility.Hidden;
            }
        }

        private void btn_unlock_entry_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Button)sender).Content == "******")
            {
                ((Button)sender).Content = "Reveal";
                ((Button)sender).Foreground = Brushes.Black;
            }
        }

        private void btn_unlock_entry_MouseLeave(object sender, MouseEventArgs e)
        {
            if (((Button)sender).Content == "Reveal")
            {
                ((Button)sender).Content = "******";
                ((Button)sender).Foreground = Brushes.Gray;
            }
        }

        private void SetOnTickEvent(int interval)
        {
            _OnTickEvent = new Timer(interval);
            _OnTickEvent.Elapsed += OnTimedEvent;
            _OnTickEvent.AutoReset = false;
            _OnTickEvent.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            lbl_clipboard_copy.Dispatcher.Invoke(new Action(() =>
            {
                lbl_clipboard_copy.Visibility = Visibility.Hidden;
            }));
        }

        private async void btn_unlock_entry_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content != "Reveal")
            {
                Clipboard.SetText((string)((Button)sender).Content);
                lbl_clipboard_copy.Visibility = Visibility.Visible;
                lbl_clipboard_copy.Content = "Copied to clipboard";
                lbl_clipboard_copy.Foreground = Brushes.Green;
                SetOnTickEvent(3000);
                return;
            }

            gif_loading.Visibility = Visibility.Visible;
            grd_viewentrymain.IsEnabled = false;

            string keyFind = ((Button)sender).Name.Split('_')[2];

            foreach (var item in _EntryData)
            {
                Dictionary<string, string> LocalDictionary = item.Value;

                string userDataItem = string.Empty;
                string[] passDataItem = null;

                if (item.Key == keyFind )
                {
                    foreach (var subitem in LocalDictionary)
                    {
                        userDataItem = subitem.Key;
                        passDataItem = subitem.Value.Split('$');

                        if (passDataItem.Length > 0)
                        {
                            for (int i = 0; i < passDataItem.Length; i++)
                            {
                                byte[] cipherText = Convert.FromBase64String(passDataItem[i]);

                                string word = await GetWhat3WordsFromPosAsync(_latitude,_longitude);

                                var preProcessBytes = Encoding.ASCII.GetBytes(word.Trim());

                                List<byte> processingBytes = preProcessBytes.ToList();

                                while (processingBytes.Count > 32)
                                {
                                    processingBytes.RemoveAt(0);
                                }

                                while (processingBytes.Count < 32)
                                {
                                    //Padding
                                    processingBytes.Insert(0, (byte)3);
                                }

                                byte[] W3PKey = processingBytes.ToArray();

                                string plaintext = "Empty";

                                try
                                {
                                    plaintext = DecryptStringFromBytes_Aes(cipherText, W3PKey);
                                    Console.WriteLine("Success!");
                                    ((Button)sender).Content = plaintext;
                                    ((Button)sender).Foreground = Brushes.Black;
                                    ((Button)sender).Opacity = 0.8;
                                    ((Button)sender).Background = Brushes.LightGreen;
                                    ((Button)sender).VerticalContentAlignment = VerticalAlignment.Center;
                                    ((Button)sender).HorizontalContentAlignment = HorizontalAlignment.Center;
                                    gif_loading.Visibility = Visibility.Hidden;
                                    lbl_clipboard_copy.Content = "Success, password copied to clipboard!";
                                    lbl_clipboard_copy.Foreground = Brushes.Green;
                                    grd_viewentrymain.IsEnabled = true;
                                    Clipboard.SetText((string)((Button)sender).Content);
                                    lbl_clipboard_copy.Visibility = Visibility.Visible;
                                    SetOnTickEvent(3000);

                                    return;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                    }
                    gif_loading.Visibility = Visibility.Hidden;
                    lbl_clipboard_copy.Visibility = Visibility.Visible;
                    lbl_clipboard_copy.Foreground = Brushes.Red;
                    lbl_clipboard_copy.Content = "Your current Geo-location does not match a designated safe-zone allocated to this password, please try again!";
                    grd_viewentrymain.IsEnabled = true;
                    SetOnTickEvent(8000);
                }
            }
        }

        private void IncrementMapPosition(double meters)
        {
            double coef = meters * 0.0000089;

            _localLatLong.Lat = _localLatLong.Lat + coef;
            _localLatLong.Lng = _localLatLong.Lng + coef / Math.Cos(_localLatLong.Lat * 0.018);
        }

        private void DecrementMapPosition(double meters)
        {
            double coef = meters * 0.0000089;

            _localLatLong.Lat = _localLatLong.Lat - coef;
            _localLatLong.Lng = _localLatLong.Lng - coef / Math.Cos(_localLatLong.Lat * 0.018);
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key)
        {
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                Byte[] IVKey = Encoding.ASCII.GetBytes("COLLABORATIONISM");
                aesAlg.IV = IVKey;
                aesAlg.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private async Task<string> GetWhat3WordsFromPosAsync(double lat, double lng)
        {
            var indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(lat, lng)).RequestAsync();

            return indexResult.Data.Words;
        }

        private async void GetWhat3WordsFromPosAsync()
        {
            var indexResult = await _what3wordsAPIWrapper.ConvertTo3WA(new Coordinates(_latitude, _longitude)).RequestAsync();

            lbl_what3words_position.Content = indexResult.Data.Words;
        }
    }
}