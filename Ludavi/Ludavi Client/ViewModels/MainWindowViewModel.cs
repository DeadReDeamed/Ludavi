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

        private ICommand openDialogCommand = null;
        public ICommand OpenDialogCommand
        {
            get { return this.openDialogCommand; }
            set { this.openDialogCommand = value; }
        }

        public static Room OpenDialog()
        {
            AddRoomWindow roomDialog = new AddRoomWindow();
            roomDialog.ShowDialog();
            AddRoomViewModel roomDialogContext = (AddRoomViewModel)(roomDialog.DataContext);
            Room result = roomDialogContext.RoomResult;
            return result;
        }


        private void OnOpenDialog(object parameter)
        {

            Room room = OpenDialog();
            Console.WriteLine(room);
        }

        public MainWindowViewModel()
        {
            this.openDialogCommand = new RelayCommand(OnOpenDialog);
        }




    }
}
