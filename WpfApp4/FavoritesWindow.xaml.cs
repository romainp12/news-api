using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MySql.Data.MySqlClient;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for FavoritesWindow.xaml
    /// </summary>
    public partial class FavoritesWindow : Window
    {
        private readonly string connectionString;
        private List<FavoriteArticle> allFavorites;
        public FavoritesWindow()
        {
            InitializeComponent();
            connectionString = "server=localhost;database=newsdb;user=root;password=root";
            this.MouseDown += FavoritesWindow_MouseDown;
            LoadFavorites();
        }
        private void FavoritesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Permettre le déplacement de la fenêtre quand on clique dessus
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Charger les favoris depuis la base de données
        private void LoadFavorites()
        {
            try
            {
                allFavorites = new List<FavoriteArticle>();

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    SELECT id, title, url, description, image_url, published_at, added_at, category
                    FROM favorites
                    ORDER BY added_at DESC";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allFavorites.Add(new FavoriteArticle
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Url = reader.GetString("url"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? "" : reader.GetString("image_url"),
                                PublishedAt = reader.IsDBNull(reader.GetOrdinal("published_at")) ? DateTime.MinValue : reader.GetDateTime("published_at"),
                                AddedAt = reader.GetDateTime("added_at"),
                                Category = reader.IsDBNull(reader.GetOrdinal("category")) ? "General" : reader.GetString("category")
                            });
                        }
                    }
                }

                // Définir la source de données pour la ListView
                FavoritesListView.ItemsSource = allFavorites;

                // Sélectionner "Toutes les catégories" par défaut
                if (CategoryFilterComboBox != null)
                {
                    CategoryFilterComboBox.SelectedIndex = 0;
                }

                // Afficher un message si aucun favori n'a été trouvé
                if (allFavorites.Count == 0)
                {
                    var customMessageBox = new CustomMessageBox("Aucun article favori n'a été trouvé.");
                    customMessageBox.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des favoris : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Boutons de contrôle de la fenêtre
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

        // Retourner à la fenêtre principale
        private void ReturnToMain_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Filtrer les favoris par catégorie
        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Nouvelle fonction : Recherche en temps réel
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Nouvelle fonction : Bouton de recherche
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        // Nouvelle fonction : Appliquer les filtres (catégorie + recherche)
        private void ApplyFilters()
        {
            if (CategoryFilterComboBox.SelectedItem is ComboBoxItem selectedItem && allFavorites != null)
            {
                string selectedCategory = selectedItem.Content.ToString();
                string searchText = SearchTextBox?.Text?.ToLower() ?? "";

                // Appliquer les deux filtres ensemble
                var filteredFavorites = allFavorites.Where(article =>
                {
                    // Filtrer par catégorie
                    bool categoryMatch = selectedCategory == "Toutes les catégories" || article.Category == selectedCategory;

                    // Filtrer par texte de recherche
                    bool searchMatch = string.IsNullOrEmpty(searchText) ||
                                      article.Title.ToLower().Contains(searchText) ||
                                      article.Description.ToLower().Contains(searchText);

                    return categoryMatch && searchMatch;
                }).ToList();

                // Mettre à jour la liste
                FavoritesListView.ItemsSource = filteredFavorites;

                // Afficher un message si aucun résultat après filtrage
                if (filteredFavorites.Count == 0 && !string.IsNullOrEmpty(searchText))
                {
                    var customMessageBox = new CustomMessageBox("Aucun article trouvé correspondant à votre recherche.");
                    customMessageBox.ShowDialog();
                }
            }
        }

        // Supprimer un article des favoris
        private void RemoveFromFavorites_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag != null)
            {
                // Récupérer l'ID de l'article à supprimer
                if (int.TryParse(button.Tag.ToString(), out int articleId))
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string query = "DELETE FROM favorites WHERE id = @id";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", articleId);
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                var customMessageBox = new CustomMessageBox("Article supprimé des favoris avec succès.");
                                customMessageBox.ShowDialog();
                                LoadFavorites(); // Recharger la liste des favoris
                            }
                            else
                            {
                                MessageBox.Show("Article non trouvé dans les favoris.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("ID d'article invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Nouvelle fonction : Ouvrir l'article via le bouton "Lire l'article"
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

        // Gérer la navigation des liens (pour la rétrocompatibilité)
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string url = e.Uri.AbsoluteUri;
            ArticleWindow articleWindow = new ArticleWindow(url);
            articleWindow.Show();
            e.Handled = true;
        }
    }
}
