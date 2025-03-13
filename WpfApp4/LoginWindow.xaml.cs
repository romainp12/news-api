using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.MouseDown += LoginWindow_MouseDown;
        }

        private void LoginWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Permettre le déplacement de la fenêtre quand on clique dessus
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameBox.Text;
                string password = PasswordBox.Password;
                string hashedPassword = BitConverter.ToString(System.Security.Cryptography.SHA256.Create()
                                            .ComputeHash(Encoding.UTF8.GetBytes(password)))
                                            .Replace("-", "").ToLower();

                using (MySqlConnection conn = new MySqlConnection("server=localhost;database=newsdb;user=root;password=root"))
                {
                    conn.Open();
                    string query = "SELECT id FROM users WHERE username=@username AND password_hash=@password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var customMessageBox = new CustomMessageBox("Connexion réussie !");
                            customMessageBox.ShowDialog();
                            new MainWindow().Show();
                            this.Close();
                        }
                        else
                        {
                            var customMessageBox = new CustomMessageBox("Identifiants incorrects");
                            customMessageBox.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var customMessageBox = new CustomMessageBox($"Erreur de connexion : {ex.Message}");
                customMessageBox.ShowDialog();
            }
        }
    }
}
