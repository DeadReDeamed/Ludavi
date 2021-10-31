using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ludavi_Client.Models;
using Ludavi_Client.Util;
using Ludavi_Client.Views;
using LumiSoft.Media.Wave;
using Newtonsoft.Json;
using TCPHandlerNamespace;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Ludavi_Client.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {

        #region define windowName

        private string windowName;
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

        private string roomName;
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

        private string textBoxText;
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
        private string roomTopic;
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
        private ObservableCollectionEx<Room> roomsCollection;
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

        private ObservableCollectionEx<User> voiceUsers;
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
        private ObservableCollectionEx<Message> messages;
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
        public static User User { get; private set; }
        public RoomManager RoomManager { get; set; }
        public TcpClient client;
        private NetworkManager network;
        public Visibility Chat { get { return _chat; } set { _chat = value; OnPropertyChanged("Chat"); } }
        private Visibility _chat;
        public Visibility Voice { get { return _voice; } set { _voice = value; OnPropertyChanged("Voice"); } }
        private Visibility _voice;
        private string joinButtonText { get; set; }
        public string JoinButtonText { get { return joinButtonText; } set { joinButtonText = value; OnPropertyChanged("JoinButtonText"); } }
        public bool IsJoinedVoice;
        private WaveIn soundReceiver = null;
        private WaveOut soundPlayer = null;
        public MainWindowViewModel()
        {
            WindowName = "Ludavi";
            IsJoinedVoice = false;
            JoinButtonText = "Join";
            this.openRoomDialogCommand = new RelayCommand(OnOpenRoomDialog);
            this.openLoginDialogCommand = new RelayCommand(OnOpenLoginDialog);
            this.VoiceCommand = new RelayCommand(OnJoinVoice);
            ConnectToServer();
            Chat = Visibility.Visible;
            Voice = Visibility.Hidden;

            OpenLoginDialogCommand.Execute("nothing");

            RoomManager = new RoomManager(tcpHandler, User.UserId, this);
            roomsCollection = new ();

            RoomManager.rooms.ForEach(room => RoomsCollection.Add(room));

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
                    InitRoom((Room)selectedItem);
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
            tcpHandler.SendMessage(User.UserId, RoomManager.currentRoom.RoomID.ToString(), TCPHandler.MessageTypes.CHAT, User.Name + ":" + message);
        }

        public void ConnectToServer()
        {
            client = new TcpClient();
            client.Connect("86.82.62.86", 80);
            tcpHandler = new TCPHandler(new MyNetworkStream(client.GetStream()));
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
            User = OpenLoginDialog();
            WindowName += $" - {User.Name}";
        }

        private async void OnJoinVoice(object paramater)
        {
            if (!IsJoinedVoice)
            {
                IsJoinedVoice = true;
                tcpHandler.SendMessage(User.UserId, RoomManager.currentRoom.RoomID.ToString(), TCPHandler.MessageTypes.VOICE, "JOIN");
                JoinButtonText = "Leave";
            }
            else
            {
                IsJoinedVoice = false;
                tcpHandler.SendMessage(User.UserId, RoomManager.currentRoom.RoomID.ToString(), TCPHandler.MessageTypes.VOICE, "LEAVE");
                network.udpHandler.close();
                soundReceiver.Stop();
                JoinButtonText = "Join";
            }
        }

        public void InitRoom(Room room)
        {
            RoomName = room.Name;
            RoomTopic = room.Topic;
            if (room.Type == (int)RoomType.Text)
            {
                RoomManager.SelectRoom(room.RoomID);
                Messages = new ObservableCollectionEx<Message>(RoomManager.GetMessagesFromRoom());
                Chat = Visibility.Visible;
                Voice = Visibility.Hidden;
            } else if(room.Type == (int)RoomType.Voice)
            {
                RoomManager.SelectRoom(room.RoomID);
                RoomManager.GetVoiceUsers();
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

        public async static Task<Room> OpenRoomDialog()
        {
            AddRoomWindow roomDialog = new AddRoomWindow();
            roomDialog.ShowDialog();
            AddRoomViewModel roomDialogContext = (AddRoomViewModel)(roomDialog.DataContext);
            Room result = roomDialogContext.RoomResult;
            tcpHandler.SendMessage(User.UserId, "", TCPHandler.MessageTypes.ROOM, "ADDROOM " + JsonConvert.SerializeObject(result));
            return result;
        }

        private async void OnOpenRoomDialog(object parameter)
        {

            Room room = await OpenRoomDialog();

            if (room == null) return;
            
            Console.WriteLine(room);
            RoomsCollection.Add(room);
            InitRoom(RoomManager.currentRoom);
        }

        #endregion

        public RelayCommand SelectedItemChangedCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public async void Dispose()
        {
            tcpHandler.SendMessage(User.UserId, "", TCPHandler.MessageTypes.LEAVE, "CLOSE ");
        }

        public void StartSendingVoiceData()
        {
            soundReceiver = new WaveIn(WaveIn.Devices[0], 8000, 16, 1, 400);
            soundPlayer = new WaveOut(WaveOut.Devices[0], 8000, 16, 1);
            soundReceiver.BufferFull += new BufferFullHandler(VoiceBufferFull);
            soundReceiver.Start();
        }
        public void VoiceBufferFull(byte[] buffer)
        {
            network.SendVoiceData(buffer);
        }

        public void PlayVoiceData(byte[] data)
        {
            soundPlayer.Play(data, 0, data.Length);
        }
    }
}
