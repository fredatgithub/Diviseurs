using Diviseurs.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

      // Charger le format d'export
      var exportFormat = Settings.Default.ExportFormat;
      foreach (ComboBoxItem item in cmbExportFormat.Items)
      {
        if (item.Tag.ToString() == exportFormat)
        {
          cmbExportFormat.SelectedItem = item;
          break;
        }
      }

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

        // Sauvegarder le format d'export
        var selectedItem = cmbExportFormat.SelectedItem as ComboBoxItem;
        Settings.Default.ExportFormat = selectedItem?.Tag?.ToString() ?? "csv";

        // Sauvegarder le chemin du fichier
        Settings.Default.FilePath = txtFilePath.Text;

        Settings.Default.Save();
      }
    }

    private void CmbExportFormat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      UpdateFileNameFormat();
    }

    private void UpdateFileNameFormat()
    {
      try
      {
        // Vérifier si le DataGrid est initialisé
        if (dgDivisors == null || dgDivisors.ItemsSource == null)
        {
          return; // Ne pas modifier si le DataGrid n'est pas prêt
        }

        // Vérifier s'il y a des données calculées
        var divisorData = dgDivisors.ItemsSource as List<DivisorData>;
        if (divisorData == null || !divisorData.Any())
        {
          return; // Ne pas modifier si pas de calcul effectué
        }

        // Obtenir le format sélectionné
        var selectedItem = cmbExportFormat.SelectedItem as ComboBoxItem;
        if (selectedItem == null || selectedItem.Tag == null)
        {
          return; // Ne pas modifier si pas de format sélectionné
        }

        var extension = selectedItem.Tag.ToString();

        // Débogage pour voir la valeur exacte
        System.Diagnostics.Debug.WriteLine($"Extension brute: '{extension}'");
        System.Diagnostics.Debug.WriteLine($"Extension nettoyée: '{extension.Replace(".", "")}'");

        // Mettre à jour le nom du fichier avec le nouveau format
        var currentFilePath = txtFilePath.Text;
        if (string.IsNullOrEmpty(currentFilePath))
        {
          return; // Ne pas modifier si pas de chemin
        }

        var directory = Path.GetDirectoryName(currentFilePath);
        var fileName = Path.GetFileNameWithoutExtension(currentFilePath);
        var maxNumber = 0;
        if (int.TryParse(txtMaxNumber.Text, out int parsedMaxNumber))
        {
          maxNumber = parsedMaxNumber;
        }

        var cleanExtension = extension.Replace(".", "");
        var newFileName = $"Diviseurs-{maxNumber}.{cleanExtension}";
        var newFilePath = string.IsNullOrEmpty(directory) ? newFileName : Path.Combine(directory, newFileName);

        // Débogage complet pour tracer le problème
        System.Diagnostics.Debug.WriteLine($"maxNumber: {maxNumber}");
        System.Diagnostics.Debug.WriteLine($"extension: '{extension}'");
        System.Diagnostics.Debug.WriteLine($"cleanExtension: '{cleanExtension}'");
        System.Diagnostics.Debug.WriteLine($"newFileName: '{newFileName}'");
        System.Diagnostics.Debug.WriteLine($"directory: '{directory}'");
        System.Diagnostics.Debug.WriteLine($"newFilePath: '{newFilePath}'");

        txtFilePath.Text = newFilePath;
      }
      catch (Exception ex)
      {
        // Ne pas afficher d'erreur pour éviter de perturber l'utilisateur
        System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour du nom de fichier : {ex.Message}");
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

        // Mettre à jour le chemin du fichier avec le nombre maxi et le format
        UpdateFileNameFormat();
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
        var maxNumber = 0;
        if (int.TryParse(txtMaxNumber.Text, out int parsedMaxNumber))
        {
          maxNumber = parsedMaxNumber;
        }
        var directory = Path.GetDirectoryName(txtFilePath.Text);
        var fileName = Path.GetFileNameWithoutExtension(txtFilePath.Text);
        var selectedItem = cmbExportFormat.SelectedItem as ComboBoxItem;
        var extension = selectedItem?.Tag?.ToString() ?? "csv";
        var finalFileName = fileName.Replace("{nombre maxi}", maxNumber.ToString());
        var cleanExtension = extension.Replace(".", "");
        var finalFilePath = Path.Combine(directory ?? "", finalFileName + "." + cleanExtension);

        // Sauvegarder selon le format
        SaveToFile(divisorData, txtFilePath.Text);

        MessageBox.Show($"Les données ont été sauvegardées avec succès dans : {finalFilePath}", "Sauvegarde réussie", MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Une erreur est survenue lors de la sauvegarde : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void SaveToFile(List<DivisorData> data, string filePath)
    {
      // Générer le nom de fichier avec le nombre maxi
      var maxNumber = data.LastOrDefault()?.Number ?? 0;
      var directory = Path.GetDirectoryName(filePath);
      var fileName = Path.GetFileNameWithoutExtension(filePath);
      var selectedItem = cmbExportFormat.SelectedItem as ComboBoxItem;
      var extension = selectedItem?.Tag?.ToString() ?? "csv";
      var finalFilePath = Path.Combine(directory ?? "", fileName + "." + extension);

      // Écrire le fichier selon le format
      switch (extension.ToLower())
      {
        case "csv":
          SaveAsCsv(data, finalFilePath);
          break;
        case "txt":
          SaveAsTxt(data, finalFilePath);
          break;
        case "json":
          SaveAsJson(data, finalFilePath);
          break;
        case "xlsx":
          SaveAsXlsx(data, finalFilePath);
          break;
      }
    }

    private void SaveAsCsv(List<DivisorData> data, string filePath)
    {
      using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
      {
        writer.WriteLine("Nombre,Diviseurs,Nombre de diviseurs,Est premier,Nombre jumeau");
        foreach (var item in data)
        {
          var divisors = item.Divisors.Replace("\"", "\"\"");
          writer.WriteLine($"{item.Number},\"{divisors}\",{item.DivisorCount},{(item.IsPrime ? "Oui" : "Non")},{(item.HasTwinPrime ? "Oui" : "Non")}");
        }
      }
    }

    private void SaveAsTxt(List<DivisorData> data, string filePath)
    {
      using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
      {
        writer.WriteLine("Rapport des diviseurs");
        writer.WriteLine("====================");
        writer.WriteLine();

        foreach (var item in data)
        {
          writer.WriteLine($"Nombre : {item.Number}");
          writer.WriteLine($"Diviseurs : {item.Divisors}");
          writer.WriteLine($"Nombre de diviseurs : {item.DivisorCount}");
          writer.WriteLine($"Est premier : {(item.IsPrime ? "Oui" : "Non")}");
          writer.WriteLine($"Nombre jumeau : {(item.HasTwinPrime ? "Oui" : "Non")}");
          writer.WriteLine();
        }
      }
    }

    private void SaveAsXlsx(List<DivisorData> data, string filePath)
    {
      using (var package = new OfficeOpenXml.ExcelPackage())
      {
        var worksheet = package.Workbook.Worksheets.Add("Diviseurs");

        // En-têtes
        worksheet.Cells[1, 1].Value = "Nombre";
        worksheet.Cells[1, 2].Value = "Diviseurs";
        worksheet.Cells[1, 3].Value = "Nombre de diviseurs";
        worksheet.Cells[1, 4].Value = "Est premier";
        worksheet.Cells[1, 5].Value = "Nombre jumeau";

        // Données
        for (int i = 0; i < data.Count; i++)
        {
          var row = i + 2;
          var item = data[i];

          worksheet.Cells[row, 1].Value = item.Number;
          worksheet.Cells[row, 2].Value = item.Divisors;
          worksheet.Cells[row, 3].Value = item.DivisorCount;
          worksheet.Cells[row, 4].Value = item.IsPrime ? "Oui" : "Non";
          worksheet.Cells[row, 5].Value = item.HasTwinPrime ? "Oui" : "Non";
        }

        package.SaveAs(filePath);
      }
    }

    private void SaveAsJson(List<DivisorData> data, string filePath)
    {
      using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
      {
        writer.WriteLine("{");
        writer.WriteLine("  \"diviseurs\": [");

        for (int i = 0; i < data.Count; i++)
        {
          var item = data[i];
          writer.WriteLine("    {");
          writer.WriteLine($"      \"nombre\": {item.Number},");
          writer.WriteLine($"      \"diviseurs\": \"{item.Divisors.Replace("\"", "\\\"")}\",");
          writer.WriteLine($"      \"nombreDiviseurs\": {item.DivisorCount},");
          writer.WriteLine($"      \"estPremier\": {(item.IsPrime ? "true" : "false")},");
          writer.WriteLine($"      \"nombreJumeau\": {(item.HasTwinPrime ? "true" : "false")}");
          writer.Write(i < data.Count - 1 ? "    }," : "    }");
          writer.WriteLine();
        }

        writer.WriteLine("  ]");
        writer.WriteLine("}");
      }
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        // Vérifier le chemin du fichier
        if (string.IsNullOrWhiteSpace(txtFilePath.Text))
        {
          MessageBox.Show("Veuillez entrer un chemin de fichier valide.", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }

        // Générer le nom de fichier final comme dans SaveToFile
        var divisorData = dgDivisors.ItemsSource as List<DivisorData>;
        var maxNumber = divisorData?.LastOrDefault()?.Number ?? 0;
        var directory = Path.GetDirectoryName(txtFilePath.Text);
        var fileName = Path.GetFileNameWithoutExtension(txtFilePath.Text);
        var selectedItem = cmbExportFormat.SelectedItem as ComboBoxItem;
        var extension = selectedItem?.Tag?.ToString() ?? "csv";
        var finalFileName = fileName.Replace("{nombre maxi}", maxNumber.ToString());
        var cleanExtension = extension.Replace(".", "");
        var finalFilePath = Path.Combine(directory ?? "", finalFileName + "." + cleanExtension);

        // Vérifier si le fichier existe
        if (!File.Exists(finalFilePath))
        {
          MessageBox.Show($"Le fichier n'existe pas : {finalFilePath}\nVeuillez d'abord sauvegarder les données.", "Fichier introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }

        // Ouvrir le fichier avec le programme par défaut
        Process.Start(new ProcessStartInfo
        {
          FileName = finalFilePath,
          UseShellExecute = true
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Une erreur est survenue lors de l'ouverture du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }
}
