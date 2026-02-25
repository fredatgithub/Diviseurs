using System.Windows;

namespace Diviseurs
{
    public partial class WaitingWindow : Window
    {
        public string CurrentNumberText
        {
            get { return txtCurrentNumber.Text; }
            set { txtCurrentNumber.Text = value; }
        }

        public WaitingWindow()
        {
            InitializeComponent();
        }
    }
}
