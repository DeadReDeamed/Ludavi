﻿using Ludavi_Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public NetworkManager(TCPHandlerNameSpace.TCPHandler tcphandler, MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            functions = new Dictionary<TCPHandler.MessageTypes, Action<string[]>>();
            functions.Add(TCPHandler.MessageTypes.CHAT, handleChatData);
            functions.Add(TCPHandler.MessageTypes.ROOM, handleRoomData);
            this.handler = tcphandler;
            Connected = true;

            new Thread( async () => {
                while (Connected)
                {
                    string[] data = handler.ReadMessage();
                    functions[(TCPHandler.MessageTypes)int.Parse(data[(int)TCPHandler.StringIndex.TYPE])].Invoke(data);
                }
            }).Start(); ;

        }

        private void handleChatData(string[] data)
        {
            RoomManager roomManager = MainWindowViewModel.roomManager;
            Message message = new Message(data[(int)TCPHandler.StringIndex.ID], DateTime.Now, data[(int)TCPHandler.StringIndex.MESSAGE]);
            roomManager.AddMessageToRoom(uint.Parse(data[(int)TCPHandler.StringIndex.ROOMID]), message);
            if (uint.Parse(data[(int)TCPHandler.StringIndex.ROOMID]) == roomManager.currentRoom.RoomID) {
                MainWindowViewModel.Messages.Add(message);
            }
        }

        private void handleRoomData(string[] data)
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
