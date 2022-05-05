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

namespace what3pass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ViewEntryWindow : Window
    {
        private SqlConnection _connection;

        public ViewEntryWindow()
        {
            InitializeComponent();

            var connectionString = ConfigurationManager.ConnectionStrings["W3PDB"].ConnectionString;

            _connection = new SqlConnection(connectionString: connectionString);
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            gif_loading.Visibility = Visibility.Visible;
            pnl_parentdataviewer.Margin = new Thickness(0,0,0,50);
            PopulateDataEntries_DB();
        }

        private async void PopulateDataEntries_DB()
        {
            
            int id = -1;
            string username;
            string password;

            SqlCommand cmd = new SqlCommand("SELECT * FROM [User];", _connection);

            await _connection.OpenAsync();

            SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                id = (int)dataReader["Id"];
                username = (string)dataReader["Username"];
                password = (string)dataReader["Password"];

                if (id != -1)
                {
                    Border dataView = new Border
                    {
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        Width = 200,
                        Height = 150,
                        Margin = new Thickness(5, 5, 5, 5)
                    };
                    pnl_parentdataviewer.Children.Add(dataView);

                    StackPanel stackPanel = new StackPanel();
                    dataView.Child = stackPanel;

                    Label label = new Label
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Content = "Platform"
                    };
                    stackPanel.Children.Add(label);

                    Label label2 = new Label
                    {
                        Margin = new Thickness(10, 10, 10, 10),
                        Content = username
                    };
                    stackPanel.Children.Add(label2);

                    Label label3 = new Label
                    {
                        Margin = new Thickness(10, 10, 10, 10),
                        Content = password
                    };
                    stackPanel.Children.Add(label3);

                    Button button = new Button
                    {
                        Height = 25,
                        Width = 50,
                        Margin = new Thickness(25, 0, 25, 25),
                        Content = "Unlock"
                    };
                    stackPanel.Children.Add(button);
                }
                else
                {
                    Border dataView = new Border
                    {
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        Width = 200,
                        Height = 150,
                        Margin = new Thickness(5, 5, 5, 5)
                    };
                    pnl_parentdataviewer.Children.Add(dataView);

                    StackPanel stackPanel = new StackPanel();
                    dataView.Child = stackPanel;

                    Label label = new Label
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Content = "Platform"
                    };
                    stackPanel.Children.Add(label);

                    Label label2 = new Label
                    {
                        Margin = new Thickness(10, 10, 10, 10),
                        Content = username
                    };
                    stackPanel.Children.Add(label2);

                    Label label3 = new Label
                    {
                        Margin = new Thickness(10, 10, 10, 10),
                        Content = password
                    };
                    stackPanel.Children.Add(label3);

                    Button button = new Button
                    {
                        Height = 25,
                        Width = 50,
                        Margin = new Thickness(25, 0, 25, 25),
                        Content = "Unlock"
                    };
                    stackPanel.Children.Add(button);
                }
            }

            _connection.Close();
            gif_loading.Visibility = Visibility.Hidden;
        }
    }
}
