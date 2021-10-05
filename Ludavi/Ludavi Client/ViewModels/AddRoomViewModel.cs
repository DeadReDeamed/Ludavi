using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ludavi_Client.Models;
using Ludavi_Client.Views;

namespace Ludavi_Client.ViewModels
{
    public class AddRoomViewModel
    {
        public Room RoomResult
        {
            get;
            private set;
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
        }

        public void CloseDialogWithResult(Window dialog, Room result)
        {
            this.RoomResult = result;
            if (dialog != null)
                dialog.DialogResult = true;
        }

        private void ConfirmClicked(object parameter)
        {
            //TODO add some format checks here
            Room room = new Room("these", "can be taken from a xaml element", "Voice");
            this.CloseDialogWithResult(parameter as Window, room);
        }

        private void CancelClicked(object parameter)
        {
            Window dialog = parameter as Window;
            ;
            dialog.DialogResult = false;
        }

        

    }
}
