using System;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Bnet;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Input;

namespace M16Chat_Windows
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow
    {
        private static String bServerIP = "m16-chat.ggu.la";
        private static String bServerPort = "6112";
        private BnetClient bClient = new BnetClient(bServerIP, bServerPort);

        private async void ShowLoginDialog(object sender, RoutedEventArgs e)
        {
            LoginDialogData result = await this.ShowLoginAsync("로그인", "M16 계정의 아이디와 비밀번호를 입력해주세요.", new LoginDialogSettings { ColorScheme = this.MetroDialogOptions.ColorScheme, AnimateShow = true });
            if (result == null || (result.Username == null || result.Password == null))
            {
                this.ShowLoginDialog(sender, e);
            }
            else
            {
                MainSpinner.IsActive = true;
                MessageDialogResult messageResult = await this.ShowMessageAsync("Authentication Information", String.Format("Username: {0}\nPassword: {1}", result.Username, result.Password));
                this.bClient.Connect(result.Username, result.Password);
            }
        }

        private void AddListItem(String data)
        {
            MainChatList.Items.Add(data);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnChatLoginHandler(String user)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                MainSpinner.IsActive = false;
                this.AddListItem(user + "님이 입장하셨습니다.");
            }));
        }

        private void OnChatUserHandler(String user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user + ": " + message);
            }));
        }

        private void Initializing(object sender, RoutedEventArgs e)
        {
            BnetClient.OnChatUser += new BnetClient.OnChatUserDelegate(OnChatUserHandler);
            BnetClient.OnChatLogined += new BnetClient.OnChatLoginedDelegate(OnChatLoginHandler);
            this.ShowLoginDialog(sender, e);
        }

        private void OnChatSend(object sender, RoutedEventArgs e)
        {
            String input = this.MainChatInput.Text;
            if(input != "")
            {
                bClient.setChatMessage(input);
                this.MainChatInput.Text = "";
                this.AddListItem(bClient.bnetUserUid + ": " + input);
            }
        }

        private void OnInputPress(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                e.Handled = true;
                OnChatSend(sender, e);
            }
        }
    }
}
