using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Diviseurs.Properties;

namespace Diviseurs
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadWindowSettings();
        }

        private void LoadWindowSettings()
        {
            // Charger la position et la taille depuis les paramètres
            this.Left = Settings.Default.MainWindowLeft;
            this.Top = Settings.Default.MainWindowTop;
            this.Width = Settings.Default.MainWindowWidth;
            this.Height = Settings.Default.MainWindowHeight;
            this.WindowState = Settings.Default.MainWindowWindowState;

            // S'assurer que la fenêtre est visible sur l'écran
            EnsureWindowIsVisible();
        }

        private void EnsureWindowIsVisible()
        {
            // Vérifier si la fenêtre est complètement hors écran
            if (this.Left < -this.Width || this.Top < -this.Height ||
                this.Left > SystemParameters.PrimaryScreenWidth ||
                this.Top > SystemParameters.PrimaryScreenHeight)
            {
                // Centrer la fenêtre sur l'écran primaire
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
        }

        private void SaveWindowSettings()
        {
            // Sauvegarder la position et la taille seulement si la fenêtre n'est pas minimisée
            if (this.WindowState != WindowState.Minimized)
            {
                Settings.Default.MainWindowLeft = this.RestoreBounds.Left;
                Settings.Default.MainWindowTop = this.RestoreBounds.Top;
                Settings.Default.MainWindowWidth = this.RestoreBounds.Width;
                Settings.Default.MainWindowHeight = this.RestoreBounds.Height;
                Settings.Default.MainWindowWindowState = this.WindowState;
                Settings.Default.Save();
            }
        }

        private async void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtMaxNumber.Text, out int maxNumber) || maxNumber < 1)
            {
                MessageBox.Show("Veuillez entrer un nombre valide supérieur à 0.", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WaitingWindow waitingWindow = new WaitingWindow();
            waitingWindow.Owner = this;
            
            try
            {
                waitingWindow.Show();
                
                var divisorData = await Task.Run(() => DivisorData.CalculateDivisors(maxNumber));
                
                dgDivisors.ItemsSource = divisorData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                waitingWindow.Close();
            }
        }
    }
}
