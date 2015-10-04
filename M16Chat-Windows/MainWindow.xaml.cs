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
using Bnet.BnetConnect;

namespace M16Chat_Windows
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow
    {
        private static String bServerIP = "m16-chat.ggu.la";
        private static String bServerPort = "6112";
        private static BnetUsername bnetUsername;
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

        private BnetChattingRGB getListColor(BnetChattingColor bnetChattingColor = BnetChattingColor.Plain)
        {
            BnetChattingRGB colorSet = new BnetChattingRGB();
            switch (bnetChattingColor)
            {
                case BnetChattingColor.Error:
                    colorSet.r = 255;
                    colorSet.g = 200;
                    colorSet.b = 200;
                    break;
                case BnetChattingColor.Info:
                    colorSet.r = 230;
                    colorSet.g = 242;
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
                    colorSet.r = 250;
                    colorSet.g = 250;
                    colorSet.b = 255;
                    break;
            }
            return colorSet;
        }

        private void AddListItem(BnetUsername data,  BnetChattingColor bnetChattingColor = BnetChattingColor.Plain, BnetChattingStatus bnetChattingStatus = BnetChattingStatus.Default)
        {
            String message = "";
            if (bnetChattingColor == BnetChattingColor.Info || bnetChattingColor == BnetChattingColor.Error)
            {
                if(bnetChattingStatus == BnetChattingStatus.Join)
                {
                    message = "님이 입장하셨습니다.";
                }
                else if(bnetChattingStatus == BnetChattingStatus.Leave)
                {
                    message = "님이 나가셨습니다.";
                }
            }

            this.AddListItem(data, message, bnetChattingColor);
        }

        private void AddListItem(BnetUsername data, String message, BnetChattingColor bnetChattingColor = BnetChattingColor.Plain)
        {
            ListBoxItem lb = new ListBoxItem();
            DockPanel dp = new DockPanel();
            TextBlock user = new TextBlock();
            user.Text = data.name;
            if(data.color != null)
            {
                user.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#" + data.color));
            }
            TextBlock tb = new TextBlock();
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Text = message;
            if (bnetChattingColor == BnetChattingColor.Error || bnetChattingColor == BnetChattingColor.Info || bnetChattingColor == BnetChattingColor.Whisper)
            {
                TextBlock status = new TextBlock();
                Thickness statusMargin = status.Margin;
                statusMargin.Right = 6;
                status.Margin = statusMargin;
                if (bnetChattingColor == BnetChattingColor.Error)
                {
                    status.Text = "[에러]";
                }
                else if (bnetChattingColor == BnetChattingColor.Info)
                {
                    status.Text = "[정보]";
                }
                else if (bnetChattingColor == BnetChattingColor.Whisper)
                {
                    status.Text = "[귓속말]";
                    Thickness userMargin = user.Margin;
                    userMargin.Right = 6;
                    user.Margin = userMargin;
                    user.Text += ":";
                }
                dp.Children.Add(status);
            } else
            {
                Thickness userMargin = user.Margin;
                userMargin.Right = 6;
                user.Margin = userMargin;
                user.Text += ":";
            }
            dp.Children.Add(user);
            dp.Children.Add(tb);
            lb.Content = dp;
            MainChatList.Items.Add(lb);
            MainChatList.SelectedIndex = MainChatList.Items.Count - 1;
            MainChatList.ScrollIntoView(MainChatList.Items[MainChatList.Items.Count - 1]);

            BnetChattingRGB colorSet = this.getListColor(bnetChattingColor);
            BnetChattingRGB borderSet = new BnetChattingRGB();

            borderSet.r = (byte)Math.Max(0, colorSet.r - 32);
            borderSet.g = (byte)Math.Max(0, colorSet.g - 32);
            borderSet.b = (byte)Math.Max(0, colorSet.b - 32);
            lb.Background = new SolidColorBrush(Color.FromRgb(colorSet.r, colorSet.g, colorSet.b));
            lb.BorderBrush = new SolidColorBrush(Color.FromRgb(borderSet.r, borderSet.g, borderSet.b));
            lb.BorderThickness = new Thickness(1, 1, 1, 1);
        }

        private void AddListItem(String message, BnetChattingColor bnetChattingColor = BnetChattingColor.Plain)
        {
            ListBoxItem lb = new ListBoxItem();
            DockPanel dp = new DockPanel();
            TextBlock tb = new TextBlock();
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Text = message;
            if (bnetChattingColor == BnetChattingColor.Error || bnetChattingColor == BnetChattingColor.Info || bnetChattingColor == BnetChattingColor.Whisper)
            {
                TextBlock status = new TextBlock();
                Thickness statusMargin = status.Margin;
                statusMargin.Right = 6;
                status.Margin = statusMargin;
                if (bnetChattingColor == BnetChattingColor.Error)
                {
                    status.Text = "[에러]";
                }
                else if (bnetChattingColor == BnetChattingColor.Info)
                {
                    status.Text = "[정보]";
                }
                else if (bnetChattingColor == BnetChattingColor.Whisper)
                {
                    status.Text = "[귓속말]";
                }
                dp.Children.Add(status);
            }
            dp.Children.Add(tb);
            lb.Content = dp;
            MainChatList.Items.Add(lb);
            MainChatList.SelectedIndex = MainChatList.Items.Count - 1;
            MainChatList.ScrollIntoView(MainChatList.Items[MainChatList.Items.Count - 1]);

            BnetChattingRGB colorSet = this.getListColor(bnetChattingColor);
            BnetChattingRGB borderSet = new BnetChattingRGB();

            borderSet.r = (byte)Math.Max(0, colorSet.r - 32);
            borderSet.g = (byte)Math.Max(0, colorSet.g - 32);
            borderSet.b = (byte)Math.Max(0, colorSet.b - 32);
            lb.Background = new SolidColorBrush(Color.FromRgb(colorSet.r, colorSet.g, colorSet.b));
            lb.BorderBrush = new SolidColorBrush(Color.FromRgb(borderSet.r, borderSet.g, borderSet.b));
            lb.BorderThickness = new Thickness(1, 1, 1, 1);
        }

        private void AddListFriend(BnetUsername name, String clan, BnetChattingColor bnetChattingColor = BnetChattingColor.Plain)
        {
            ListBoxItem lb = new ListBoxItem();
            TextBlock tb = new TextBlock();
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Text = name.name + ((clan != "") ? " [" + clan + "]" : "");
            lb.Content = tb;
            FriendsList.Items.Add(lb);
            FriendsList.SelectedIndex = MainChatList.Items.Count - 1;
            FriendsList.ScrollIntoView(MainChatList.Items[MainChatList.Items.Count - 1]);

            BnetChattingRGB colorSet = this.getListColor(bnetChattingColor);
            BnetChattingRGB borderSet = new BnetChattingRGB();

            borderSet.r = (byte)Math.Max(0, colorSet.r - 32);
            borderSet.g = (byte)Math.Max(0, colorSet.g - 32);
            borderSet.b = (byte)Math.Max(0, colorSet.b - 32);
            lb.Background = new SolidColorBrush(Color.FromRgb(colorSet.r, colorSet.g, colorSet.b));
            lb.BorderBrush = new SolidColorBrush(Color.FromRgb(borderSet.r, borderSet.g, borderSet.b));
            lb.BorderThickness = new Thickness(1, 1, 1, 1);
        }

        public MainWindow()
        {
            try {
                InitializeComponent();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.StackTrace);
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void OnChatLoginHandler(BnetUsername user)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                MainChatInput.IsEnabled = true;
                MainChatInput.Focus();
                this.AddListItem(user, BnetChattingColor.Info, BnetChattingStatus.Join);
                bnetUsername = bClient.getUsername();
                MainSpinner.IsActive = false;
            }));
        }

        private void OnChatSockError()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.AddListItem("M16 서버와의 연결이 종료되었습니다.", BnetChattingColor.Error);
                MainChatInput.IsEnabled = false;
            }));
        }

        private void OnChatUserHandler(BnetUsername user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user, message, BnetChattingColor.Plain);
            }));
        }

        private void OnChatErrorHandler(BnetUsername user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user, message, BnetChattingColor.Error);
            }));
        }

        private void OnChatInfoHandler(BnetUsername user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user, message, BnetChattingColor.Info);
            }));
        }

        private void OnChatJoinHandler(BnetUsername user)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user, BnetChattingColor.Info, BnetChattingStatus.Join);
            }));
        }

        private void OnChatLeaveHandler(BnetUsername user)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user, BnetChattingColor.Info, BnetChattingStatus.Leave);
            }));
        }

        private void OnChatWhisperHandler(BnetUsername user, String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                this.AddListItem(user, message, BnetChattingColor.Whisper);
            }));
        }

        private void OnChatFriendsUpdateHandler(BnetFriends[] bnetFriends)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                FriendsList.Items.Clear();
                BnetChattingColor bnetChattingColor = new BnetChattingColor();
                for (int i=0; i<bnetFriends.Length; i++)
                {
                    if(bnetFriends[i].name.name == bnetUsername.name)
                    {
                        bnetChattingColor = BnetChattingColor.Me;
                    } else
                    {
                        bnetChattingColor = BnetChattingColor.Plain;
                    }
                    this.AddListFriend(bnetFriends[i].name, bnetFriends[i].locationName, bnetChattingColor);
                }
            }));
        }

        private void Initializing(object sender, RoutedEventArgs e)
        {
            BnetClient.OnChatUser += new BnetClient.OnChatUserDelegate(OnChatUserHandler);
            BnetClient.OnChatError += new BnetClient.OnChatErrorDelegate(OnChatErrorHandler);
            BnetClient.OnChatInfo += new BnetClient.OnChatInfoDelegate(OnChatInfoHandler);
            BnetClient.OnChatJoin += new BnetClient.OnChatJoinDelegate(OnChatJoinHandler);
            BnetClient.OnChatLeave += new BnetClient.OnChatLeaveDelegate(OnChatLeaveHandler);
            BnetClient.OnChatWhisper += new BnetClient.OnChatWhisperDelegate(OnChatWhisperHandler);
            BnetClient.OnChatLogined += new BnetClient.OnChatLoginedDelegate(OnChatLoginHandler);
            BnetClient.OnChatSockError += new BnetClient.OnChatSockErrorDelegate(OnChatSockError);
            BnetClient.OnChatFriendsUpdate += new BnetClient.OnChatFriendsUpdateDelegate(OnChatFriendsUpdateHandler);
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
                    this.AddListItem(bnetUsername, input, BnetChattingColor.Me);
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
