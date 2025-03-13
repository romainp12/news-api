using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MySql.Data.MySqlClient;
using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;


namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NewsApiClient newsApiClient;
        private readonly string apiKey;
        private readonly string connectionString;

        public MainWindow()
        {
            InitializeComponent();
            apiKey = "5f187fa0286649a2ba5f9c6e19a66048";
            connectionString = "server=localhost;database=newsdb;user=romaina;password=romaina";
            newsApiClient = new NewsApiClient(apiKey);

            // Attacher les gestionnaires d'événements pour le déplacement de fenêtre
            this.MouseDown += MainWindow_MouseDown;

            // Sélectionner la première catégorie par défaut
            if (CategoryComboBox.Items.Count > 0)
                CategoryComboBox.SelectedIndex = 0;
            else
                LoadDefaultNews();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
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

        // Charger les actualités par défaut au démarrage
        private async void LoadDefaultNews()
        {
            await LoadNewsAsync();
        }

        // Méthode pour récupérer les articles de l'API
        private async Task LoadNewsAsync(string category = null, string keyword = null, DateTime? dateMin = null, DateTime? dateMax = null)
        {
            try
            {
                var request = new TopHeadlinesRequest
                {
                    Q = keyword,
                    Country = Countries.US,
                    Language = Languages.EN
                };

                if (!string.IsNullOrEmpty(category) && Enum.TryParse(category, out Categories parsedCategory))
                {
                    request.Category = parsedCategory;
                }

                var articlesResponse = await Task.Run(() => newsApiClient.GetTopHeadlines(request));

                if (articlesResponse.Status == Statuses.Ok)
                {
                    var filteredArticles = articlesResponse.Articles
                        .Where(a => !string.IsNullOrWhiteSpace(a.Title) && a.Title != "[Removed]");

                    if (dateMin.HasValue)
                        filteredArticles = filteredArticles.Where(a => a.PublishedAt >= dateMin.Value).ToList();

                    if (dateMax.HasValue)
                        filteredArticles = filteredArticles.Where(a => a.PublishedAt <= dateMax.Value).ToList();

                    ArticlesListView.ItemsSource = filteredArticles.ToList();
                }
                else
                {
                    var customMessageBox = new CustomMessageBox($"Erreur API: {articlesResponse.Status}\n{articlesResponse.Error?.Message}");
                    customMessageBox.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var customMessageBox = new CustomMessageBox($"Erreur lors de la récupération des articles : {ex.Message}");
                customMessageBox.ShowDialog();
            }
        }

        // Gestion du changement de catégorie
        private async void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedCategory = selectedItem?.Content.ToString();
                await LoadNewsAsync(category: selectedCategory);
            }
        }

        // Ajouter un article aux favoris
        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag != null)
            {
                var articleObject = button.Tag;

                // Accéder aux propriétés par réflexion
                string url = articleObject.GetType().GetProperty("Url")?.GetValue(articleObject) as string;

                if (!string.IsNullOrEmpty(url) && !IsArticleFavorite(url))
                {
                    // Récupérer la catégorie sélectionnée
                    string selectedCategory = "General";
                    if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        selectedCategory = selectedItem.Content.ToString();
                    }

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"
                        INSERT INTO favorites (title, url, description, image_url, published_at, added_at, category)
                        VALUES (@title, @url, @desc, @img, @published, NOW(), @category)";

                        MySqlCommand cmd = new MySqlCommand(query, conn);

                        // Obtenir les valeurs par réflexion
                        cmd.Parameters.AddWithValue("@title", articleObject.GetType().GetProperty("Title")?.GetValue(articleObject) as string ?? "");
                        cmd.Parameters.AddWithValue("@url", url);
                        cmd.Parameters.AddWithValue("@desc", articleObject.GetType().GetProperty("Description")?.GetValue(articleObject) as string ?? "");
                        cmd.Parameters.AddWithValue("@img", articleObject.GetType().GetProperty("UrlToImage")?.GetValue(articleObject) as string ?? "");
                        cmd.Parameters.AddWithValue("@category", selectedCategory);

                        // Pour la date, un peu plus complexe
                        var publishedAt = articleObject.GetType().GetProperty("PublishedAt")?.GetValue(articleObject);
                        cmd.Parameters.AddWithValue("@published", publishedAt != null ? publishedAt : DBNull.Value);

                        cmd.ExecuteNonQuery();

                        var customMessageBox = new CustomMessageBox("Article ajouté aux favoris !");
                        customMessageBox.ShowDialog();
                    }
                }
                else if (!string.IsNullOrEmpty(url))
                {
                    var customMessageBox = new CustomMessageBox("Cet article est déjà dans vos favoris !");
                    customMessageBox.ShowDialog();
                }
                else
                {
                    var customMessageBox = new CustomMessageBox("Article invalide.");
                    customMessageBox.ShowDialog();
                }
            }
            else
            {
                var customMessageBox = new CustomMessageBox("Veuillez sélectionner un article avant d'ajouter aux favoris.");
                customMessageBox.ShowDialog();
            }
        }

        // Vérifier si un article est déjà dans les favoris
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

        // Ouvrir la fenêtre des favoris
        private void OpenFavorites_Click(object sender, RoutedEventArgs e)
        {
            FavoritesWindow favoritesWindow = new FavoritesWindow();
            favoritesWindow.Show();
        }

        // Lire un article (nouvelle méthode)
        private void ReadArticle_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string url = button.Tag.ToString();
                ArticleWindow articleWindow = new ArticleWindow(url);
                articleWindow.Show();
            }
        }

        // Pour la rétrocompatibilité avec les hyperliens
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string url = e.Uri.AbsoluteUri;
            ArticleWindow articleWindow = new ArticleWindow(url);
            articleWindow.Show();
            e.Handled = true;
        }
    }
}