using System;
using System.Windows;
using System.Windows.Input;
using Ludavi_Client.Models;
using Ludavi_Client.Views;


namespace Ludavi_Client.ViewModels
{
    public class MainWindowViewModel
    {


        public string RoomName => "roomName";
        public string RoomTopic => "roomTopic, but i'm writing a bit more to fill this box ;)";


        public MainWindowViewModel()
        {
            this.openRoomDialogCommand = new RelayCommand(OnOpenRoomDialog);
        }


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
            Console.WriteLine(room);
        }

       




    }
}
