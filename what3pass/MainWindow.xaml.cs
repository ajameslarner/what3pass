using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        private SqlConnection _connection;
        public static User s_currentUser;

        public static byte[] GlobalIVKey;
        public MainWindow()
        {
            InitializeComponent();

            grd_main_login.Visibility = Visibility.Visible;

            var connectionString = ConfigurationManager.ConnectionStrings["W3PDB"].ConnectionString;

            _connection = new SqlConnection(connectionString: connectionString);
        }

        private void txt_info_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txt_info.Visibility = Visibility.Hidden;
            lbl_welcome.Content = "Welcome, " + s_currentUser.Username;
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
            ViewEntryWindow newWindow = new ViewEntryWindow();
            newWindow.Show();

        }

        private void btn_newentry_Click(object sender, RoutedEventArgs e)
        {
            NewEntryWindow newWindow = new NewEntryWindow();
            newWindow.Show();
        }

        private void txt_register_username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_register_username.Text == "Enter your username...")
            {
                txt_register_username.Text = "";
                txt_register_username.Foreground = Brushes.Black;
            }
        }

        private void txt_register_username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_register_username.Text))
            {
                txt_register_username.Text = "Enter your username...";
                txt_register_username.Foreground = Brushes.Gray;
            }
        }

        private void txt_register_password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_register_password.Password == "Password...")
            {
                txt_register_password.Password = "";
                txt_register_password.Foreground = Brushes.Black;
            }
        }

        private void txt_register_password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_register_password.Password))
            {
                txt_register_password.Password = "Password...";
                txt_register_password.Foreground = Brushes.Gray;
            }
        }

        private void txt_register_passwordconfirm_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_register_passwordconfirm.Password == "Password...")
            {
                txt_register_passwordconfirm.Password = "";
                txt_register_passwordconfirm.Foreground = Brushes.Black;
            }
        }

        private void txt_register_passwordconfirm_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_register_passwordconfirm.Password))
            {
                txt_register_passwordconfirm.Password = "Password...";
                txt_register_passwordconfirm.Foreground = Brushes.Gray;
            }
        }

        private void txt_login_username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_login_username.Text == "Enter your username...")
            {
                txt_login_username.Text = "";
                txt_login_username.Foreground = Brushes.Black;
            }
        }

        private void txt_login_username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_login_username.Text))
            {
                txt_login_username.Text = "Enter your username...";
                txt_login_username.Foreground = Brushes.Gray;
            }
        }

        private void txt_login_password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txt_login_password.Password == "Password...")
            {
                txt_login_password.Password = "";
                txt_login_password.Foreground = Brushes.Black;
            }
        }

        private void txt_login_password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_login_password.Password))
            {
                txt_login_password.Password = "Password...";
                txt_login_password.Foreground = Brushes.Gray;
            }
        }

        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            btn_login.Visibility = Visibility.Hidden;
            gif_loading.Visibility = Visibility.Visible;
            txt_errormessage.Visibility = Visibility.Hidden;

            if ((txt_login_username.Text != null && txt_login_username.Text != "Enter your username...") && (txt_login_password.Password != null && txt_login_password.Password != "Enter your password..."))
            {
                CheckUsernameExists_DB(txt_login_username.Text, txt_login_password.Password);
            }
            else
            {
                txt_errormessage.Content = "Warning, do not leave empty fields!";
                txt_errormessage.Foreground = Brushes.Orange;
                txt_errormessage.Visibility = Visibility.Visible;

                gif_loading.Visibility = Visibility.Hidden;
                btn_login.Visibility = Visibility.Visible;
            }
        }

        private bool DoesPasswordMeetRequirements(string password, out string errorMsg)
        {
            var input = password;
            errorMsg = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(password))
            {
                errorMsg = "Password should contain at least one lower case letter.";
                return false;
            }
            else if (!hasUpperChar.IsMatch(password))
            {
                errorMsg = "Password should contain at least one upper case letter.";
                return false;
            }
            else if (!hasMiniMaxChars.IsMatch(password))
            {
                errorMsg = "Password should not be lesser than 8 or greater than 15 characters.";
                return false;
            }
            else if (!hasNumber.IsMatch(password))
            {
                errorMsg = "Password should contain at least one numeric value.";
                return false;
            }

            else if (!hasSymbols.IsMatch(password))
            {
                errorMsg = "Password should contain at least one special case character.";
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void CheckUsernameExists_DB(string username, string password)
        {
            int id = -1;
            SqlCommand cmd = new SqlCommand("SELECT * FROM [User] WHERE username = @Username", _connection);
            cmd.Parameters.AddWithValue("@Username", username);

            await _connection.OpenAsync();

            SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                id = (Int32)dataReader["Id"];
            }

            _connection.Close();

            if (id > 0)
            {
                CheckPasswordMatch_DB(username, password);
            }
            else
            {
                txt_errormessage.Content = "Username does not exist, please try again.";
                txt_errormessage.Foreground = Brushes.Red;
                txt_errormessage.Visibility = Visibility.Visible;

                gif_loading.Visibility = Visibility.Hidden;
                btn_login.Visibility = Visibility.Visible;
            }
        }

        private async void CheckInsertRegistration_DB(string username, string password)
        {
            int id = -1;
            SqlCommand cmd = new SqlCommand("SELECT * FROM [User] WHERE username = @Username", _connection);
            cmd.Parameters.AddWithValue("@Username", username);

            await _connection.OpenAsync();

            SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                id = (Int32)dataReader["Id"];
            }

            _connection.Close();

            if (!(id > 0))
            {
                InsertUserInto_DB(username, password);
                grd_main_login.Visibility = Visibility.Visible;
                grd_main_register.Visibility = Visibility.Hidden;

                txt_errormessage.Content = "Success. Account created!";
                txt_errormessage.Foreground = Brushes.Green;
                txt_errormessage.Visibility = Visibility.Visible;

                gif_loading.Visibility = Visibility.Hidden;
                btn_register_register.Visibility = Visibility.Visible;
                lbl_welcome.Content = "Account Login";
            }
            else
            {
                txt_errormessage.Content = "Username already exists, please try again.";
                txt_errormessage.Foreground = Brushes.Red;
                txt_errormessage.Visibility = Visibility.Visible;

                gif_loading.Visibility = Visibility.Hidden;
                btn_register_register.Visibility = Visibility.Visible;
            }
        }

        private async void CheckPasswordMatch_DB(string username, string password)
        {
            int id = -1;
            SqlCommand cmd = new SqlCommand("SELECT * FROM [User] WHERE username LIKE @Username AND password = @Password;", _connection);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Password", password);

            await _connection.OpenAsync();

            SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                id = (Int32)dataReader["Id"];
            }

            _connection.Close();

            if (id > 0)
            {
                s_currentUser = new User(txt_login_username.Text, id);
                grd_main_login.Visibility = Visibility.Hidden;
                grd_main.Visibility = Visibility.Visible;
                gif_loading.Visibility = Visibility.Hidden;
                lbl_welcome.Content = $"Welcome, {txt_login_username.Text}.";
            }
            else
            {
                txt_errormessage.Content = "Incorrect password, please try again.";
                txt_errormessage.Foreground = Brushes.Red;
                txt_errormessage.Visibility = Visibility.Visible;

                btn_login.Visibility = Visibility.Visible;
                gif_loading.Visibility = Visibility.Hidden;
            }
        }

        private async void InsertUserInto_DB(string username, string password)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO [User] (username, password, email, created_at, last_updated) VALUES (@Username, @Password, @Email, @Created_at, @Last_updated)", _connection);
            cmd.Parameters.Add(new SqlParameter("Username", username));
            cmd.Parameters.Add(new SqlParameter("Password", password));
            cmd.Parameters.Add(new SqlParameter("Email", "Default@Email"));
            cmd.Parameters.Add(new SqlParameter("Created_at", DateTime.Now.ToString("MM/dd/yyyy")));
            cmd.Parameters.Add(new SqlParameter("Last_updated", DateTime.Now.ToString("MM/dd/yyyy")));

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            _connection.Close();
        }

        private void btn_register_Click(object sender, RoutedEventArgs e)
        {
            grd_main_register.Visibility = Visibility.Visible;
            grd_main_login.Visibility = Visibility.Hidden;

            lbl_welcome.Content = "Account Register";

            txt_errormessage.Visibility = Visibility.Hidden;
            txt_errormessage.Content = "";
        }

        private void btn_register_register_Click(object sender, RoutedEventArgs e)
        {
            btn_register_register.Visibility = Visibility.Hidden;
            gif_loading.Visibility = Visibility.Visible;

            txt_errormessage.Visibility = Visibility.Hidden;
            txt_errormessage.Content = "";

            if ((txt_register_username.Text != null && txt_register_username.Text != "Enter your username...") && (txt_register_password.Password != null && txt_register_password.Password != "Password...") && (txt_register_passwordconfirm.Password != null && txt_register_passwordconfirm.Password != "Password..."))
            {
                if (txt_register_password.Password != txt_register_passwordconfirm.Password)
                {
                    txt_errormessage.Content = "Passwords do not match, please try again.";
                    txt_errormessage.Foreground = Brushes.Red;
                    txt_errormessage.Visibility = Visibility.Visible;

                    gif_loading.Visibility = Visibility.Hidden;
                    btn_register_register.Visibility = Visibility.Visible;
                    return;
                }

                if (DoesPasswordMeetRequirements(txt_register_password.Password, out string errorMsg) == false)
                {
                    txt_errormessage.Content = errorMsg;
                    txt_errormessage.Foreground = Brushes.Red;
                    txt_errormessage.Visibility = Visibility.Visible;

                    gif_loading.Visibility = Visibility.Hidden;
                    btn_register_register.Visibility = Visibility.Visible;
                    return;
                }

                CheckInsertRegistration_DB(txt_register_username.Text, txt_register_password.Password);
            }
            else
            {
                txt_errormessage.Content = "Warning, do not leave empty fields!";
                txt_errormessage.Foreground = Brushes.Orange;
                txt_errormessage.Visibility = Visibility.Visible;

                gif_loading.Visibility = Visibility.Hidden;
                btn_register_register.Visibility = Visibility.Visible;
            }
        }

        private void btn_register_back_Click(object sender, RoutedEventArgs e)
        {
            grd_main_register.Visibility = Visibility.Hidden;
            grd_main_login.Visibility = Visibility.Visible;
            txt_errormessage.Visibility = Visibility.Hidden;

            lbl_welcome.Content = "Account Login";
        }
    }
}
