using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Ludavi_Client.Models;
using Ludavi_Client.Views;


namespace Ludavi_Client.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
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
        private ObservableCollection<Room> roomsCollection { get; set; }
        public ObservableCollection<Room> RoomsCollection
        {
            get { return roomsCollection; }
            set
            {
                roomsCollection = value;
                OnPropertyChanged("RoomsCollection");
            }
        }

        #endregion


        public MainWindowViewModel()
        {
            this.openRoomDialogCommand = new RelayCommand(OnOpenRoomDialog);
            this.openLoginDialogCommand = new RelayCommand(OnOpenLoginDialog);
            roomsCollection = new();
            roomName = "Welcome";
            roomTopic = "please select or add a room to start communicating!";


            SelectedItemChangedCommand = new RelayCommand((selectedItem) =>
            {
                initRoom((Room)selectedItem);
            });


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


    }
}
