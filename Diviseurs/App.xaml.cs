using System.Windows;

namespace Diviseurs
{
  /// <summary>
  /// Logique d'interaction pour App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      // Pas besoin de configuration de licence avec ClosedXML
      base.OnStartup(e);
    }
  }
}
