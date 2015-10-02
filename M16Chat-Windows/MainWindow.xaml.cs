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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnChatLoginHandler(String user)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                MainSpinner.IsActive = false;
                MainChatBox.Text += user + "님이 입장하셨습니다.\r\n";
            }));
        }

        private void OnChatUserHandler(String user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                MainChatBox.Text += user + ": " + message + "\r\n";
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
                this.MainChatBox.Text += bClient.bnetUserUid + ": " + input + "\r\n";
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
