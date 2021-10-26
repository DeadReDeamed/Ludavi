using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

namespace Ludavi_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TCPHandler tcpHandler;
        private uint ID;

        public MainWindow(TCPHandler handler)
        {
            InitializeComponent();
            new Thread(async () => { await connectToServer(); }).Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        

        public async Task<Task> connectToServer()
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync("localhost", 80);
            TCPHandler tcpHandler = new TCPHandler(client.GetStream());
            await tcpHandler.SendMessage(0, "", TCPHandler.MessageTypes.LOGIN, "Luca Password");
            string[] message = await tcpHandler.ReadMessage();
            Console.WriteLine(message[((int)TCPHandler.StringIndex.MESSAGE)]);
            ID = uint.Parse(message[((int)TCPHandler.StringIndex.ID)]);
            return Task.CompletedTask;
        }
    }
}
