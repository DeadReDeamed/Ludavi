﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPHandlerNameSpace;

namespace Ludavi_Client.ViewModels
{
    class NetworkManager
    {
        private Dictionary<TCPHandler.MessageTypes, Action<string[]>> functions;
        private TCPHandler handler;
        public bool Connected { get; set; }
        public MainWindowViewModel MainWindowViewModel;
        public NetworkManager(TCPHandler tcphandler, MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            functions = new Dictionary<TCPHandler.MessageTypes, Action<string[]>>();
            functions.Add(TCPHandler.MessageTypes.CHAT, handleChatData);
            functions.Add(TCPHandler.MessageTypes.ROOM, handleRoomData);
            this.handler = tcphandler;
            Connected = true;

            new Thread(() => {
                while (Connected)
                {
                    string[] data = handler.ReadMessage();
                    functions[(TCPHandler.MessageTypes)int.Parse(data[(int)TCPHandler.StringIndex.TYPE])].Invoke(data);
                }
            }).Start(); ;

        }

        private void handleChatData(string[] data)
        {

        }

        private void handleRoomData(string[] data)
        {
            if (data[(int)TCPHandler.StringIndex.MESSAGE] == "UPDATEROOMS")
            {
                MainWindowViewModel.roomManager = new Models.RoomManager(handler, MainWindowViewModel.ID);
                MainWindowViewModel.RoomsCollection.Clear();
                MainWindowViewModel.roomManager.rooms.ForEach(room => { MainWindowViewModel.RoomsCollection.Add(room); });
            }
        }
    }
}
