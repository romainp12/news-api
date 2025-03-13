using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for ArticleWindow.xaml
    /// </summary>
    public partial class ArticleWindow : Window
    {
        private readonly string connectionString = "server=localhost;database=newsdb;user=root;password=root";
        private string currentUrl;
        private string currentTitle;
        public ArticleWindow(string articleUrl)
        {
            InitializeComponent();
            currentUrl = articleUrl;

            // Attacher les gestionnaires d'événements pour le déplacement de fenêtre
            this.MouseDown += ArticleWindow_MouseDown;

            InitializeWebView(articleUrl);

        }

        private void ArticleWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Permettre le déplacement de la fenêtre quand on clique dessus
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Contrôles de fenêtre
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void InitializeWebView(string url)
        {
            try
            {
                await WebViewer.EnsureCoreWebView2Async();
                WebViewer.CoreWebView2.Navigate(url);

                // S'abonner à l'événement de fin de navigation pour récupérer le titre
                WebViewer.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    currentTitle = WebViewer.CoreWebView2.DocumentTitle;
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la page web: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = currentUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                var customMessageBox = new CustomMessageBox($"Erreur lors de l'ouverture du navigateur: {ex.Message}");
                customMessageBox.ShowDialog();
            }
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentUrl))
            {
                var customMessageBox = new CustomMessageBox("URL de l'article invalide.");
                customMessageBox.ShowDialog();
                return;
            }

            // Vérifier si l'article est déjà dans les favoris
            if (IsArticleFavorite(currentUrl))
            {
                var customMessageBox = new CustomMessageBox("Cet article est déjà dans vos favoris !");
                customMessageBox.ShowDialog();
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    INSERT INTO favorites (title, url, description, image_url, published_at, added_at, category)
                    VALUES (@title, @url, @desc, @img, NOW(), NOW(), @category)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@title", currentTitle ?? "Article sans titre");
                    cmd.Parameters.AddWithValue("@url", currentUrl);
                    cmd.Parameters.AddWithValue("@desc", ""); // Pas de description disponible directement
                    cmd.Parameters.AddWithValue("@img", ""); // Pas d'image disponible directement
                    cmd.Parameters.AddWithValue("@category", "General");

                    cmd.ExecuteNonQuery();

                    var customMessageBox = new CustomMessageBox("Article ajouté aux favoris !");
                    customMessageBox.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var customMessageBox = new CustomMessageBox($"Erreur lors de l'ajout aux favoris : {ex.Message}");
                customMessageBox.ShowDialog();
            }
        }

        private bool IsArticleFavorite(string url)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM favorites WHERE url = @url";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@url", url);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
