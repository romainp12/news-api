using System.Windows;
using System.Windows.Input;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;

            this.MouseDown += CustomMessageBox_MouseDown;
        }

        private void CustomMessageBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Permettre le déplacement de la fenêtre quand on clique dessus
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
