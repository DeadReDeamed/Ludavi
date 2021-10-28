using Ludavi_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCPHandlerNameSpace;

namespace Ludavi_Client.Views
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class loginWindow : Window
    {
        public loginWindow(TCPHandler tcpHandler)
        {
            this.DataContext = new LoginViewModel(tcpHandler);
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).Password = ((PasswordBox)sender).Password; }
        }
    }
}
