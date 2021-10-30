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

        #region define windowName

        private string windowName { get; set; }
        public string WindowName
        {
            get { return windowName; }
            set
            {
                windowName = value;
                OnPropertyChanged("WindowName");
            }
        }

        #endregion

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

        #region define textBoxText

        private string textBoxText { get; set; }
        public string TextBoxText
        {
            get { return textBoxText; }
            set
            {
                textBoxText = value;
                OnPropertyChanged("TextBoxText");
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

        #region define VoiceUsers

        private ObservableCollectionEx<User> voiceUsers { get; set; }
        public ObservableCollectionEx<User> VoiceUsers
        {
            get { return voiceUsers; }
            set
            {
                voiceUsers = value;
                OnPropertyChanged("VoiceUsers");
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
        public static User user { get; private set; }
        public RoomManager roomManager { get; set; }
        private TcpClient client;
        private NetworkManager network;
        public Visibility Chat { get { return _chat; } set { _chat = value; OnPropertyChanged("Chat"); } }
        private Visibility _chat;
        public Visibility Voice { get { return _voice; } set { _voice = value; OnPropertyChanged("Voice"); } }
        private Visibility _voice;
        private string joinButtonText { get; set; }
        public string JoinButtonText { get { return joinButtonText; } set { joinButtonText = value; OnPropertyChanged("JoinButtonText"); } }
        public bool IsJoinedVoice;
        public MainWindowViewModel()
        {
            WindowName = "Ludavi";
            IsJoinedVoice = false;
            JoinButtonText = "Join";
            this.openRoomDialogCommand = new RelayCommand(OnOpenRoomDialog);
            this.openLoginDialogCommand = new RelayCommand(OnOpenLoginDialog);
            this.VoiceCommand = new RelayCommand(OnJoinVoice);

            connectToServer();
            Chat = Visibility.Visible;
            Voice = Visibility.Hidden;

            OpenLoginDialogCommand.Execute("nothing");

            roomManager = new RoomManager(tcpHandler, user.UserId, this);
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

            SendCommand = new RelayCommand((nothing) =>
            {
                if(TextBoxText != null)
                    if (!String.IsNullOrEmpty(TextBoxText))
                    {
                        SendMessageToRoom(TextBoxText);
                        TextBoxText = "";
                    }
                    
            });
        }

        public async void SendMessageToRoom(string message)
        {
            await tcpHandler.SendMessage(user.UserId, roomManager.currentRoom.RoomID.ToString(), TCPHandler.MessageTypes.CHAT, user.Name + ":" + message);
        }

        public void connectToServer()
        {
            client = new TcpClient();
            client.Connect("localhost", 80);
            tcpHandler = new TCPHandler(client.GetStream());
            //Console.WriteLine("dawdad");
        }

        private User OpenLoginDialog()
        {
            loginWindow loginDialog = new loginWindow(tcpHandler);
            loginDialog.ShowDialog();
            LoginViewModel loginDialogContext = (LoginViewModel)(loginDialog.DataContext);
            User result = new User(loginDialogContext.UserName, loginDialogContext.Id);
            return result;
        }


        private void OnOpenLoginDialog(object paramater)
        {
            user = OpenLoginDialog();
            WindowName += $" - {user.Name}";
        }

        private async void OnJoinVoice(object paramater)
        {
            if (!IsJoinedVoice)
            {
                IsJoinedVoice = true;
                await tcpHandler.SendMessage(user.UserId, roomManager.currentRoom.RoomID.ToString(), TCPHandler.MessageTypes.VOICE, "JOIN");
                JoinButtonText = "Leave";
            }
            else
            {
                IsJoinedVoice = false;
                await tcpHandler.SendMessage(user.UserId, roomManager.currentRoom.RoomID.ToString(), TCPHandler.MessageTypes.VOICE, "LEAVE");
                JoinButtonText = "Join";
            }
        }

        public void initRoom(Room room)
        {
            RoomName = room.Name;
            RoomTopic = room.Topic;
            if (room.Type == (int)RoomType.Text)
            {
                roomManager.SelectRoom(room.RoomID);
                Messages = new ObservableCollectionEx<Message>(roomManager.GetMessagesFromRoom());
                Chat = Visibility.Visible;
                Voice = Visibility.Hidden;
            } else if(room.Type == (int)RoomType.Voice)
            {
                roomManager.SelectRoom(room.RoomID);
                roomManager.GetVoiceUsers();
                Chat = Visibility.Hidden;
                Voice = Visibility.Visible;
            }
        }
        public RelayCommand VoiceCommand { get; set; }

        public RelayCommand SendCommand { get; set; }

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
            tcpHandler.SendMessage(user.UserId, "", TCPHandler.MessageTypes.ROOM, "ADDROOM " + JsonConvert.SerializeObject(result));
            return result;
        }

        private void OnOpenRoomDialog(object parameter)
        {

            Room room = OpenRoomDialog();

            if (room == null) return;
            
            Console.WriteLine(room);
            RoomsCollection.Add(room);
            initRoom(roomManager.currentRoom);
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
