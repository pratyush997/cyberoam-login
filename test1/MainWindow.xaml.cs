using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Timers;
using System.Windows.Threading;
using System.Net.Http;
using System.ComponentModel;
using System.Security.Cryptography;


namespace LoginApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        static bool isTryingToLogIn = true;
        public LoginStatus CurrentStatus = LoginStatus.LoggedOut;
        private string userID;
        private string password = String.Empty;
        static bool isLive = false;
        public event EventHandler ModeChanged;
        public DispatcherTimer RefreshTimer = new DispatcherTimer();
        public MainWindowViewModel VM;
        public string link = Properties.Settings.Default.URL.ToString();

        private List<Credential> userList;
        
        

        public MainWindow()
        {
            InitializeComponent();
            RefreshTimer.Interval = new TimeSpan(0, 0, 5);
            RefreshTimer.Tick += RefreshTimer_Tick;
            ModeChanged += MainWindow_ModeChanged;
            userList = new List<Credential>();
            userList = GetUserData();
            VM = new MainWindowViewModel();
            this.DataContext = VM;


            if (Properties.Settings.Default.AutoLogin)
            {
                this.Loaded += MainWindow_Loaded;       //        Auto Login ON
            }
            //ProfileComboBox.SelectedValue = userID;

       //     this.Icon = LoginApp.Properties.Resources.icon;

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();  // TODO: Fix Jugaad
            ni.Icon = LoginApp.Properties.Resources.icon;
            ni.Visible = true;
            ni.Click +=
               (sender, args) =>
               {
                   this.Show();
                   this.WindowState = WindowState.Normal;
               };
            
            //VM.Message = Convert.ToString(CurrentStatus);

            if (VM.Credentials.Count() != 0)
            {
                Refill();
            }
        }

        void Refill()
        {
            var credential = VM.Credentials.FirstOrDefault();
            UserNameTextBox.Text = credential.UserID;
            PasswordTextBox.Password = credential.Password;
            ProfileComboBox.SelectedValue = credential.UserID;
            this.userID = credential.UserID;
            this.password = credential.Password;
            
            // test MessageBox.Show(ProfileComboBox.SelectedValue.ToString());
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            //?
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();  // TODO: Fix Jugaad
            ni.Icon = LoginApp.Properties.Resources.icon;
            ni.Visible = false;
            ni.Dispose();
            Application.Current.Shutdown();;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)        //Needed for AutoLogin
        {
            this.GoButton_Click(null,null);
        }

        private List<Credential> GetUserData()
        {
            return null;
        }


        void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {

                HttpClient client = new HttpClient();
                String url = String.Format("http://{0}/live?mode=192&username={1}", link, this.userID);
                var response = client.GetAsync(url);
                var responseTask = response.Result.Content.ReadAsStringAsync();
                string responseString = responseTask.Result;

                FilterRefresh(responseString);
            }

            catch (System.AggregateException)
            {
                CurrentStatus = LoginStatus.NoConnection;

                //Refill(); // Resets the selected profile
                MessageBoxResult result = MessageBox.Show("Unable to Connect to Login Server.\n\nRetry", "Connection Error", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                //TODO: Fix this ^^
                if (result == MessageBoxResult.Yes)
                {
                    Login(userID, password);
                    this.WindowState = WindowState.Normal;
                }

                else if (result == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void FilterRefresh(string responseString)
        {
            if (responseString.Contains("ack"))
            {
                CurrentStatus = LoginStatus.LoggedIn;
            }
            else
            {
                CurrentStatus = LoginStatus.LogInAgain;
            }

            VM.Message = Convert.ToString(CurrentStatus);
        }

        void MainWindow_ModeChanged(object sender, EventArgs e)
        {
            if (isLive)
            {
                RefreshTimer.Start();
            }
            else
            {
                RefreshTimer.Stop();
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            
            string userId = UserNameTextBox.Text;
            string password = this.password;          

            if (isTryingToLogIn)
            {
                        //TODO: Any Better way to do it? 
                if (String.IsNullOrEmpty(userId) && String.IsNullOrEmpty(password))
                    MessageBox.Show("Enter Credentials");
                else if (String.IsNullOrEmpty(userId))
                    MessageBox.Show("Enter UserID");
                else if (String.IsNullOrEmpty(password))
                    MessageBox.Show("Enter Password");
                else 
                {
                    Login(userId, password);
                    if (CurrentStatus == LoginStatus.LoggedIn)
                    {
                        isTryingToLogIn = false;
                        GoButton.Content = "Log Out";
                        this.userID = userId;
                        isLive = true;
                        ModeChanged(this, null);
                        if (VM.Credentials.Where(x => x.UserID == this.userID).Count() == 0)
                        {
                            VM.Credentials.Add(new Credential() { UserID = this.userID, Password = this.password });
                            VM.SaveData();
                        }
                        PasswordTextBox.Password = "";
                        this.password = "";

                        ProfileComboBox.SelectedValue = userID;
                    }
                }
            }
                else
                {
                    if (String.IsNullOrEmpty(userId))
                        MessageBox.Show("Enter UserID");
                else
                    {
                        Logout(userId);
                        isTryingToLogIn = true;
                        GoButton.Content = "Log In";
                        isLive = false;
                        ModeChanged(this, null);
                        //    StatusBarText.Text = "Logged Out";
                    }
                }

                VM.Message = Convert.ToString(CurrentStatus);
            }
        

        private void Logout(string userId)
        {            
            var request = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}/logout.xml",link));

            string data = String.Format("mode=193&username={0}", userId);
            var dataByte = Encoding.UTF8.GetBytes(data);

            request.Host = "172.16.100.2:8090";
            request.Method = "POST";
            request.ContentLength = dataByte.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(dataByte, 0, dataByte.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                FilterResult(responseString);
            }

            catch (System.Net.WebException)
            {
                //MessageBox.Show("Unable to connect");
                CurrentStatus = LoginStatus.NoConnection;
            }   
        }

        private void Login(string userId, string password)
        {
                var request = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}/login.xml",link));

                string data = String.Format("mode=191&username={0}&password={1}", userId, password);
                var dataByte = Encoding.UTF8.GetBytes(data);

                request.Host = link;
                request.Method = "POST";
                request.ContentLength = dataByte.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                try
                {
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(dataByte, 0, dataByte.Length);
                    }


                    var response = (HttpWebResponse)request.GetResponse();

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    FilterResult(responseString);
                }
                catch (System.Net.WebException)
                {
                    //MessageBox.Show("Unable to connect");
                    CurrentStatus = LoginStatus.NoConnection;
                    
                }

        }
        public void StatusBarBG(String color){

            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString(color);
            StatusBar.Background = (brush);
        }


        private void FilterResult(string responseString)
        {
           

            if(responseString.Contains("LIVE"))
            {
                CurrentStatus = LoginStatus.LoggedIn;
                StatusBarBG("Green");
            }
            else if(responseString.Contains("password"))
            {
                CurrentStatus = LoginStatus.WrongPassword;
                StatusBarBG("Red");
            }
            else if(responseString.Contains("Maximum"))
            {
                CurrentStatus = LoginStatus.MaxLimit;
                StatusBarBG("Blue");
            }
            else if (responseString.Contains("exceeded"))
            {
                CurrentStatus = LoginStatus.DataLimit;
                StatusBarBG("Yellow");
            }
            else if (responseString.Contains("exceeded"))
            {
                CurrentStatus = LoginStatus.LoggedOut;
                StatusBarBG("White");
            }
        }

        
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.GoButton_Click(null, null);
            }

            //Buggy Restore from Noti bar.
            //else if (e.Key == Key.Escape)
            //{
            //    this.WindowState = WindowState.Minimized;
            //}

        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.password = PasswordTextBox.Password;
        }

       


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Application.Current.Shutdown();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            Window1 configureWindow = new Window1();
            configureWindow.Show();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            //TODO: add About
            MessageBox.Show("todo");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProfileDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileComboBox.SelectedValue != null)
            {
                var credential = VM.Credentials.Where(x => x.UserID == ((Credential)ProfileComboBox.SelectedItem).UserID).FirstOrDefault();
                if (credential != null)
                {
                    VM.Credentials.Remove(credential);
                    VM.SaveData();

                    UserNameTextBox.Text = null;
                    PasswordTextBox.Password = null;

                    if (VM.Credentials.Count() != 0)
                    {
                        credential = VM.Credentials.First();
                        ProfileComboBox.SelectedValue = credential.UserID;
                    }
                }

            }
            else
                MessageBox.Show("No Profiles Found");
        }



        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem != null)
            {

                var credential = VM.Credentials.Where(x => x.UserID == ((Credential)ProfileComboBox.SelectedItem).UserID).FirstOrDefault();
                if (credential != null)
                {
                    //Logout(userID);

                    //isTryingToLogIn = true;
                    //GoButton.Content = "Log In";
                    UserNameTextBox.Text = credential.UserID;
                    PasswordTextBox.Password = credential.Password;
                    this.userID = credential.UserID;
                    this.password = credential.Password;
                }
            }
        }



    }

}


//TODO: notifi icon remove
//TODO: Selection_changed fix
//TODO: Find something better to Notify Connection Failure ( use Notifi baloon?)
//TODO: Revamp credentials load/save
//TODO: Async Login process