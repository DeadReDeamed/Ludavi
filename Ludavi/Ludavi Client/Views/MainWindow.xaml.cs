using Ludavi_Client.Models;
using Ludavi_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Ludavi_Client.ViewModels;

namespace Ludavi_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(this.DataContext != null)
            {
                ((MainWindowViewModel)this.DataContext).SendMessageToRoom(messageTextBox.Text);
                messageTextBox.Text = "";
            }
        }
       

        private void MainWindow_OnInitialized(object? sender, EventArgs e)
        {
            if (this.DataContext != null && (this.DataContext.GetType() == typeof(MainWindowViewModel)))
            {
                MainWindowViewModel vm = (MainWindowViewModel) this.DataContext;
                vm.OpenLoginDialogCommand.Execute("nothing");
            }
        }
    }
}
