using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Ici, tu peux ajouter des conditions avant d'ouvrir la fenêtre de connexion
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }

}
