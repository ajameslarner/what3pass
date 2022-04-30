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

namespace what3pass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void txt_info_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txt_info.Visibility = Visibility.Hidden;
            lbl_welcome.Content = "Welcome";
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_ref_Click(object sender, RoutedEventArgs e)
        {
            txt_info.Visibility = Visibility.Visible;
            txt_info.Text = "Reference details for what3words";
            lbl_welcome.Content = "What3Words";
        }

        private void btn_lbe_Click(object sender, RoutedEventArgs e)
        {
            txt_info.Visibility = Visibility.Visible;
            txt_info.Text = "Location based encryption explanation";
            lbl_welcome.Content = "LBE";
        }

        private void btn_about_Click(object sender, RoutedEventArgs e)
        {
            txt_info.Visibility = Visibility.Visible;
            txt_info.Text = "What what3pass is and a little description on how it works";
            lbl_welcome.Content = "About";
        }

        private void btn_viewentries_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_newentry_Click(object sender, RoutedEventArgs e)
        {
            NewEntryWindow entryWindow = new NewEntryWindow();
            entryWindow.Show();
            Close();
        }
    }
}
