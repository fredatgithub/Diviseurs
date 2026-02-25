using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Diviseurs
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
