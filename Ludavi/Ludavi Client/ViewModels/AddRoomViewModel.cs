using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ludavi_Client.Models;
using Ludavi_Client.Views;
using TCPHandlerNameSpace.Models;

namespace Ludavi_Client.ViewModels
{
    public class AddRoomViewModel : INotifyPropertyChanged
    {

        public string windowTitle => "add a room";

        
        public Room RoomResult
        {
            get;
            private set;
        }

        public string RoomName { get; set; }
        public string RoomTopic { get; set; }
        public int RoomType { get; set; }

        

        private string nameBorderColor;
        public string NameBorderColor
        {
            get { return nameBorderColor; }
            set
            {
                nameBorderColor = value;
                OnPropertyChanged("NameBorderColor");
            }
        }

        private string topicBorderColor;
        public string TopicBorderColor
        {
            get { return topicBorderColor; }
            set
            {
                topicBorderColor = value;
                OnPropertyChanged("TopicBorderColor");
            }
        }



        private ICommand createRoomCommand = null;
        public ICommand CreateRoomCommand
        {
            get { return createRoomCommand; }
            set { createRoomCommand = value; }
        }

        private ICommand cancelCommand = null;
        public ICommand CancelCommand
        {
            get { return cancelCommand; }
            set { cancelCommand = value; }
        }

        public AddRoomViewModel()
        {
           
            this.createRoomCommand = new RelayCommand(ConfirmClicked);
            this.cancelCommand = new RelayCommand(CancelClicked);

        this.NameBorderColor = "Black";
        this.TopicBorderColor = "Black";

        }

        public void CloseDialogWithResult(Window dialog, Room result)
        {
            this.RoomResult = result;
            if (dialog != null)
                dialog.DialogResult = true;
        }

        private void ConfirmClicked(object parameter)
        {
            
            if (CheckForValidRoomValues(RoomName, RoomTopic, RoomType))
            {
                Room room = new Room(RoomName, RoomTopic, RoomType, 1);
                this.CloseDialogWithResult(parameter as Window, room);
            }
            
        }

        private bool CheckForValidRoomValues(string roomName, string roomTopic, int roomType)
        {
            bool illegalFormat = false;
            if (String.IsNullOrEmpty(roomName))
            {
                this.NameBorderColor = "Red";
                illegalFormat = true;
            }
            else this.NameBorderColor = "Black";

            if (String.IsNullOrEmpty(roomTopic))
            {
                this.TopicBorderColor = "Red";
                illegalFormat = true;
            }
            else this.TopicBorderColor = "Black";

            return !illegalFormat;
        }

        private void CancelClicked(object parameter)
        {
            Window dialog = parameter as Window;
            ;
            dialog.DialogResult = false;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
