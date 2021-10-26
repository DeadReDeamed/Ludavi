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

namespace Ludavi_Client.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {

        public string windowTitle => "login";

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

        #region define userName

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

        public LoginViewModel()
        {
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

        private void RegisterClicked(object parameter)
        {
            if (String.IsNullOrEmpty(UserName) || String.IsNullOrEmpty(Password))
            {
                ErrorText = "please enter both a username and password";
                return;
            }

            Window dialog = parameter as Window;

            var id = GuidFromString(UserName + ":" + Password);

            //send details to server.
            //only continue if server tells you the 
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

        private void LoginClicked(object parameter)
        {
            //return if any field is empty
            if (String.IsNullOrEmpty(UserName) || String.IsNullOrEmpty(Password))
            {
                ErrorText = "please enter both a username and password";
                return;
            }

            Window dialog = parameter as Window;

            var id = GuidFromString(UserName + ":" + Password);

            //check if server knows id
            if (UserName.Equals("test") && Password.Equals("test"))
            {
                dialog.DialogResult = true;
            }
            else ErrorText = "username or password is incorrect";
            
            
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
