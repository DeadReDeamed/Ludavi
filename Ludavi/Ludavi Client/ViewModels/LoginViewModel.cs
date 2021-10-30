using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ludavi_Client.Annotations;
using TCPHandlerNameSpace;

namespace Ludavi_Client.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {

        public string windowTitle => "login";

        public TCPHandler TcpHandler;

        #region define userName

        private string userName { get; set; }
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }

        #endregion

        #region define errorText

        private string errorText { get; set; }
        public string ErrorText
        {
            get { return errorText; }
            set
            {
                errorText = value;
                OnPropertyChanged("ErrorText");
            }
        }

        #endregion

        #region define password

        public string Password { private get; set; }

        #endregion

        #region define ID

        private Guid id { get; set; }
        public Guid Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        #endregion

        public string Type { get; set; }

        public LoginViewModel(TCPHandler tcpHandler)
        {
            TcpHandler = tcpHandler;
            this.loginCommand = new RelayCommand(LoginClicked);
            this.registerCommand = new RelayCommand(RegisterClicked);
        }

        #region define registerCommand

        private ICommand registerCommand = null;
        public ICommand RegisterCommand
        {
            get { return registerCommand; }
            set { registerCommand = value; }
        }

        private async void RegisterClicked(object parameter)
        {
            Type = "REGISTER";
            if (String.IsNullOrEmpty(UserName) || String.IsNullOrEmpty(Password))
            {
                ErrorText = "please enter both a username and password";
                return;
            }
            if (UserName.Contains(":"))
            {
                ErrorText = "please use a proper username";
                return;
            }

            Window dialog = parameter as Window;

           Id = GuidFromString(UserName + ":" + Password);

            await TcpHandler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.REGISTER, $"{Id} {UserName}");
            string[] message = await TcpHandler.ReadMessage();
            Console.WriteLine(message[((int)TCPHandler.StringIndex.MESSAGE)]);
            ErrorText = message[((int)TCPHandler.StringIndex.MESSAGE)];

            if (message[((int)TCPHandler.StringIndex.MESSAGE)] == "ok") dialog.DialogResult = true;
            else ErrorText = message[((int)TCPHandler.StringIndex.MESSAGE)];

            if (false)
                dialog.DialogResult = true;
        }

        #endregion

        #region define loginCommand

        private ICommand loginCommand = null;
        public ICommand LoginCommand
        {
            get { return loginCommand; }
            set { loginCommand = value; }
        }

        private async void LoginClicked(object parameter)
        {
            Type = "LOGIN";
            //return if any field is empty
            if (String.IsNullOrEmpty(UserName) || String.IsNullOrEmpty(Password))
            {
                ErrorText = "please enter both a username and password";
                return;
            }
            if (UserName.Contains(":"))
            {
                ErrorText = "please use a proper username";
                return;
            }

            Window dialog = parameter as Window;

            Id = GuidFromString(UserName + ":" + Password);

            //check if server knows id
            await TcpHandler.SendMessage(Guid.Empty, "", TCPHandler.MessageTypes.LOGIN, $"{Id} {UserName}");
            string[] message = await TcpHandler .ReadMessage();
            
            if (message[((int)TCPHandler.StringIndex.MESSAGE)] == "ok") dialog.DialogResult = true;
            else ErrorText = message[((int)TCPHandler.StringIndex.MESSAGE)];


        }

        #endregion


        private static Guid GuidFromString(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                return new Guid(hash);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
