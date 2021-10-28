using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ludavi_Client.Models;
using Ludavi_Client.Util;
using Ludavi_Client.Views;
using Newtonsoft.Json;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Ludavi_Client.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {

        #region define roomName

        private string roomName { get; set; }
        public string RoomName
        {
            get { return roomName; }
            set
            {
                roomName = value;
                OnPropertyChanged("RoomName");
            }
        }

        #endregion

        #region define roomTopic
        private string roomTopic { get; set; }
        public string RoomTopic
        {
            get { return roomTopic; }
            set
            {
                roomTopic = value;
                OnPropertyChanged("RoomTopic");
            }
        }
        #endregion

        #region define roomsCollection
        private ObservableCollectionEx<Room> roomsCollection { get; set; }
        public ObservableCollectionEx<Room> RoomsCollection
        {
            get { return roomsCollection; }
            set
            {
                roomsCollection = value;
                OnPropertyChanged("RoomsCollection");
            }
        }

        #endregion

        #region define Messages
        private ObservableCollectionEx<Message> messages { get; set; }
        public ObservableCollectionEx<Message> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                OnPropertyChanged("Messages");
            }
        }

        #endregion

        private static TCPHandler tcpHandler;
        public static uint ID { get; private set; }
        public RoomManager roomManager { get; set; }
        private TcpClient client;
        private NetworkManager network;
        
        public MainWindowViewModel()
        {
            this.openRoomDialogCommand = new RelayCommand(OnOpenRoomDialog);
            this.openLoginDialogCommand = new RelayCommand(OnOpenLoginDialog);
            
            
            connectToServer();
            roomManager = new RoomManager(tcpHandler, ID, this);
            roomsCollection = new ();

            roomManager.rooms.ForEach(room => RoomsCollection.Add(room));

            network = new NetworkManager(tcpHandler, this);

            roomName = "Welcome";
            roomTopic = "please select or add a room to start communicating!";
            if(Messages == null)
            {
                Messages = new ObservableCollectionEx<Message>();
            }
            

            SelectedItemChangedCommand = new RelayCommand((selectedItem) =>
            {
                if(selectedItem != null)
                    initRoom((Room)selectedItem);
            });
        }

        public async void SendMessageToRoom(string message)
        {
            await tcpHandler.SendMessage(ID, roomManager.currentRoom.RoomID.ToString(), TCPHandler.MessageTypes.CHAT, message);
        }

        public void connectToServer()
        {
            client = new TcpClient();
            client.Connect("localhost", 80);
            tcpHandler = new TCPHandler(client.GetStream());
            tcpHandler.SendMessage(0, "", TCPHandler.MessageTypes.LOGIN, "Luca Password");
            string[] message = tcpHandler.ReadMessage();
            Console.WriteLine(message[((int)TCPHandler.StringIndex.MESSAGE)]);
            ID = uint.Parse(message[((int)TCPHandler.StringIndex.ID)]);
        }

        private void OnOpenLoginDialog(object paramater)
        {
            loginWindow loginDialog = new loginWindow();
            loginDialog.ShowDialog();
        }

        public void initRoom(Room room)
        {
            RoomName = room.Name;
            RoomTopic = room.Topic;
            roomManager.SelectRoom(room.RoomID);
            Messages = new ObservableCollectionEx<Message>(roomManager.GetMessagesFromRoom());
        }


        private ICommand openLoginDialogCommand = null;

        public ICommand OpenLoginDialogCommand
        {
            get { return this.openLoginDialogCommand; }
            set { this.openLoginDialogCommand = value; }
        }


        #region add room dialog, commands and helper methods

        private ICommand openRoomDialogCommand = null;
        public ICommand OpenRoomDialogCommand
        {
            get { return this.openRoomDialogCommand; }
            set { this.openRoomDialogCommand = value; }
        }

        public static Room OpenRoomDialog()
        {
            AddRoomWindow roomDialog = new AddRoomWindow();
            roomDialog.ShowDialog();
            AddRoomViewModel roomDialogContext = (AddRoomViewModel)(roomDialog.DataContext);
            Room result = roomDialogContext.RoomResult;
            tcpHandler.SendMessage(ID, "", TCPHandler.MessageTypes.ROOM, "ADDROOM " + JsonConvert.SerializeObject(result));
            return result;
        }

        private void OnOpenRoomDialog(object parameter)
        {

            Room room = OpenRoomDialog();

            if (room == null) return;
            
            Console.WriteLine(room);
            RoomsCollection.Add(room);
            initRoom(room);
        }

        #endregion

        public RelayCommand SelectedItemChangedCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Dispose()
        {
            this.network.Connected = false;
            this.client.GetStream().Close();
        }
    }
}
