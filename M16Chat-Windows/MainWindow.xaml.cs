using System;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Bnet;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using M16Chat_Windows.BnetChatting;

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
                //MessageDialogResult messageResult = await this.ShowMessageAsync("Authentication Information", String.Format("Username: {0}\nPassword: {1}", result.Username, result.Password));
                this.MainChatInput.IsEnabled = false;
                this.AddListItem("M16 서버와 연결중입니다.", BnetChattingColor.Info);
                this.bClient.Connect(result.Username, result.Password);
            }
        }

        private void AddListItem(String data, BnetChattingColor bnetChattingColor = BnetChattingColor.Plain)
        {
            ListBoxItem lb = new ListBoxItem();
            lb.Content = data;
            MainChatList.Items.Add(lb);
            MainChatList.SelectedIndex = MainChatList.Items.Count - 1;
            MainChatList.ScrollIntoView(MainChatList.Items[MainChatList.Items.Count - 1]);

            BnetChattingRGB colorSet = new BnetChattingRGB();
            BnetChattingRGB borderSet = new BnetChattingRGB();

            switch(bnetChattingColor)
            {
                case BnetChattingColor.Error:
                    colorSet.r = 255;
                    colorSet.g = 200;
                    colorSet.b = 200;
                    break;
                case BnetChattingColor.Info:
                    colorSet.r = 200;
                    colorSet.g = 230;
                    colorSet.b = 255;
                    break;
                case BnetChattingColor.Tip:
                    colorSet.r = 224;
                    colorSet.g = 240;
                    colorSet.b = 255;
                    break;
                case BnetChattingColor.Whisper:
                    colorSet.r = 224;
                    colorSet.g = 255;
                    colorSet.b = 224;
                    break;
                case BnetChattingColor.Me:
                    colorSet.r = 255;
                    colorSet.g = 240;
                    colorSet.b = 224;
                    break;
                default:
                    colorSet.r = 248;
                    colorSet.g = 248;
                    colorSet.b = 255;
                    break;
            }
            borderSet.r = (byte)Math.Max(0, colorSet.r - 32);
            borderSet.g = (byte)Math.Max(0, colorSet.g - 32);
            borderSet.b = (byte)Math.Max(0, colorSet.b - 32);
            lb.Background = new SolidColorBrush(Color.FromRgb(colorSet.r, colorSet.g, colorSet.b));
            lb.BorderBrush = new SolidColorBrush(Color.FromRgb(borderSet.r, borderSet.g, borderSet.b));
            lb.BorderThickness = new Thickness(1, 1, 1, 1);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnChatLoginHandler(String user)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                MainChatInput.IsEnabled = true;
                MainChatInput.Focus();
                this.AddListItem(user + "님이 입장하셨습니다.", BnetChattingColor.Info);
                MainSpinner.IsActive = false;
            }));
        }

        private void OnChatSockError()
        {
            this.AddListItem("M16 서버와의 연결이 종료되었습니다.", BnetChattingColor.Error);
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                MainChatInput.IsEnabled = false;
            }));
        }

        private void OnChatUserHandler(String user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user + ": " + message, BnetChattingColor.Plain);
            }));
        }

        private void OnChatErrorHandler(String user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem("[경고]: " + message, BnetChattingColor.Error);
            }));
        }

        private void OnChatInfoHandler(String user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem("[알림]: " + message, BnetChattingColor.Info);
            }));
        }

        private void OnChatWhisperHandler(String user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user + "[귓속말]: " + message, BnetChattingColor.Whisper);
            }));
        }

        private void Initializing(object sender, RoutedEventArgs e)
        {
            BnetClient.OnChatUser += new BnetClient.OnChatUserDelegate(OnChatUserHandler);
            BnetClient.OnChatError += new BnetClient.OnChatErrorDelegate(OnChatErrorHandler);
            BnetClient.OnChatInfo += new BnetClient.OnChatInfoDelegate(OnChatInfoHandler);
            BnetClient.OnChatWhisper += new BnetClient.OnChatWhisperDelegate(OnChatWhisperHandler);
            BnetClient.OnChatLogined += new BnetClient.OnChatLoginedDelegate(OnChatLoginHandler);
            BnetClient.OnChatSockError += new BnetClient.OnChatSockErrorDelegate(OnChatSockError);
            this.ShowLoginDialog(sender, e);
        }

        private void OnChatSend(object sender, RoutedEventArgs e)
        {
            String input = this.MainChatInput.Text;
            if(input != "")
            {
                bClient.setChatMessage(input);
                this.MainChatInput.Text = "";
                if (input[0] != '/')
                {
                    this.AddListItem(bClient.bnetUserUid + ": " + input, BnetChattingColor.Me);
                }
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
