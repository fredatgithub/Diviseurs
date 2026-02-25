using Diviseurs.Properties;
using System;
using System.Collections.Generic;
using System.IO;
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

      // Charger la valeur du textbox
      txtMaxNumber.Text = Settings.Default.MaxNumber;

      // Charger le chemin du fichier
      txtFilePath.Text = Settings.Default.FilePath;

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

        // Sauvegarder la valeur du textbox
        Settings.Default.MaxNumber = txtMaxNumber.Text;

        // Sauvegarder le chemin du fichier
        Settings.Default.FilePath = txtFilePath.Text;

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

        var divisorData = await Task.Run(() => DivisorData.CalculateDivisors(maxNumber, currentNumber =>
        {
          // Mettre à jour l'UI sur le thread principal
          this.Dispatcher.Invoke(() =>
          {
            waitingWindow.CurrentNumberText = $"Calcul du nombre : {currentNumber} / {maxNumber}";
          });
        }));

        dgDivisors.ItemsSource = divisorData;

        // Mettre à jour le chemin du fichier avec le nombre maxi
        var currentFilePath = txtFilePath.Text;
        var directory = Path.GetDirectoryName(currentFilePath);
        var extension = Path.GetExtension(currentFilePath);
        var newFilePath = Path.Combine(directory ?? "", $"Diviseurs-{maxNumber}{extension}");
        txtFilePath.Text = newFilePath;
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

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        // Vérifier si des données sont disponibles
        if (dgDivisors.ItemsSource == null)
        {
          MessageBox.Show("Aucune donnée à sauvegarder. Veuillez d'abord effectuer un calcul.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        // Vérifier le chemin du fichier
        if (string.IsNullOrWhiteSpace(txtFilePath.Text))
        {
          MessageBox.Show("Veuillez entrer un chemin de fichier valide.", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }

        var divisorData = dgDivisors.ItemsSource as List<DivisorData>;
        if (divisorData == null || !divisorData.Any())
        {
          MessageBox.Show("Aucune donnée à sauvegarder.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        // Générer le nom de fichier final pour le message
        var maxNumber = divisorData.LastOrDefault()?.Number ?? 0;
        var directory = Path.GetDirectoryName(txtFilePath.Text);
        var fileName = Path.GetFileNameWithoutExtension(txtFilePath.Text);
        var extension = Path.GetExtension(txtFilePath.Text);
        var finalFileName = fileName.Replace("{nombre maxi}", maxNumber.ToString());
        var finalFilePath = Path.Combine(directory ?? "", finalFileName + extension);

        // Sauvegarder en CSV
        SaveToCsv(divisorData, txtFilePath.Text);
        
        MessageBox.Show($"Les données ont été sauvegardées avec succès dans : {finalFilePath}", "Sauvegarde réussie", MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Une erreur est survenue lors de la sauvegarde : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void SaveToCsv(List<DivisorData> data, string filePath)
    {
      // Générer le nom de fichier avec le nombre maxi
      var maxNumber = data.LastOrDefault()?.Number ?? 0;
      var directory = Path.GetDirectoryName(filePath);
      var fileName = Path.GetFileNameWithoutExtension(filePath);
      var extension = Path.GetExtension(filePath);
      
      // Remplacer les placeholders dans le nom de fichier
      var finalFileName = fileName.Replace("{nombre maxi}", maxNumber.ToString());
      var finalFilePath = Path.Combine(directory ?? "", finalFileName + extension);

      // Créer le répertoire si nécessaire
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }

      // Écrire le fichier CSV
      using (var writer = new StreamWriter(finalFilePath, false, System.Text.Encoding.UTF8))
      {
        // En-tête CSV
        writer.WriteLine("Nombre,Diviseurs,Nombre de diviseurs,Est premier,Nombre jumeau");

        // Données
        foreach (var item in data)
        {
          // Échapper les virgules dans les diviseurs si nécessaire
          var divisors = item.Divisors.Replace("\"", "\"\"");
          writer.WriteLine($"{item.Number},\"{divisors}\",{item.DivisorCount},{(item.IsPrime ? "Oui" : "Non")},{(item.HasTwinPrime ? "Oui" : "Non")}");
        }
      }
    }
  }
}
