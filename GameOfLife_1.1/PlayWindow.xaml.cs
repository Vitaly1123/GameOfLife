using System.Windows;

namespace GameOfLife
{
    public partial class PlayWindow : Window
    {
        public PlayWindow()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow gameWindow = new MainWindow();
            gameWindow.Show();
            this.Close(); // Закриваємо поточне вікно
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // Закриваємо весь додаток
        }
    }
}