using Ludavi_Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TCPHandlerNamespace;
using TCPHandlerNameSpace;
using TCPHandlerNameSpace.Models;

namespace Ludavi_Client.ViewModels
{
    class NetworkManager
    {
        private Dictionary<TCPHandler.MessageTypes, Action<string[]>> functions;
        private TCPHandler handler;
        public bool Connected { get; set; }
        public MainWindowViewModel MainWindowViewModel;
        private UDPHandler udpHandler;
        public NetworkManager(TCPHandlerNameSpace.TCPHandler tcphandler, MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            udpHandler = new UDPHandler();
            functions = new Dictionary<TCPHandler.MessageTypes, Action<string[]>>();
            functions.Add(TCPHandler.MessageTypes.CHAT, HandleChatData);
            functions.Add(TCPHandler.MessageTypes.ROOM, HandleRoomData);
            functions.Add(TCPHandler.MessageTypes.LEAVE, HandleLeaving);
            functions.Add(TCPHandler.MessageTypes.VOICE, handleVoiceData);
            this.handler = tcphandler;
            Connected = true;

            new Thread( async () => {
                while (Connected)
                {
                    string[] data = await handler.ReadMessage();
                    functions[(TCPHandler.MessageTypes)int.Parse(data[(int)TCPHandler.StringIndex.TYPE])].Invoke(data);
                }
            }).Start(); ;

        }

        private void handleVoiceData(string[] data)
        {
            string[] message = data[(int)TCPHandler.StringIndex.MESSAGE].Split(" ", 3);
            if(message[0] == "OK")
            {
                udpHandler.Connect("", int.Parse(message[1]));
                udpHandler.SetReceivePoint(IPAddress.Any, int.Parse(message[2]));
                startListeningForVoice();
                MainWindowViewModel.StartSendingVoiceData();
            }
        }

        private void startListeningForVoice()
        {
            new Thread(async () =>
            {
                Tuple<Guid, uint, byte[]> message = udpHandler.ReceiveUdpMessage();
                MainWindowViewModel.PlayVoiceData(message.Item3);
            }).Start();
        }

        public void SendVoiceData(byte[] data)
        {
            udpHandler.SendUdpMessage(MainWindowViewModel.user.UserId, MainWindowViewModel.roomManager.currentRoom.RoomID, data);
        }

        private void HandleLeaving(string[] data)
        {
            Connected = false;
            MainWindowViewModel.client.GetStream().Close();   
        }

        private void HandleChatData(string[] data)
        {
            RoomManager roomManager = MainWindowViewModel.roomManager;
            string[] messageValues = data[(int)TCPHandler.StringIndex.MESSAGE].Split(':', 2);
            Message message = new Message(messageValues[0] + "#" + data[(int)TCPHandler.StringIndex.ID].Substring(0,4), DateTime.Now, messageValues[1]);
            roomManager.AddMessageToRoom(uint.Parse(data[(int)TCPHandler.StringIndex.ROOMID]), message);
            if (uint.Parse(data[(int)TCPHandler.StringIndex.ROOMID]) == roomManager.currentRoom.RoomID) {
                MainWindowViewModel.Messages.Add(message);
            }
        }

        private void HandleRoomData(string[] data)
        {
            string[] message = data[(int)TCPHandler.StringIndex.MESSAGE].Split(" ", 2);
            switch (message[0])
            {
                case "UPDATEROOMS":
                    MainWindowViewModel.roomManager.UpdateRooms();

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.MainWindowViewModel.RoomsCollection.Clear();
                    }));

                    MainWindowViewModel.roomManager.rooms.ForEach(room =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.MainWindowViewModel.RoomsCollection.Add(room);
                            }));
                    });
                    break;
                case "RETURNUSERS":
                    MainWindowViewModel.VoiceUsers = new Util.ObservableCollectionEx<User>(JsonConvert.DeserializeObject<List<User>>(message[1]));
                    break;
                case "RETURNROOMS":
                    MainWindowViewModel.roomManager.UpdateRoomList(message[1]);
                    break;
            } 
        }
    }
}
