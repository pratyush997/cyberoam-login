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
using System.Windows.Shapes;

namespace LoginApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            if (Properties.Settings.Default.AutoLogin)
            {
                loginCheckBox.IsChecked = true;
            }
        }

        private void loginCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (loginCheckBox.IsChecked.Value)
                Properties.Settings.Default.AutoLogin = true;
            else
                Properties.Settings.Default.AutoLogin = false;
            Properties.Settings.Default.Save();
        }

        private void hostButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.URL = hostBox.Text;
            Properties.Settings.Default.Save();
        }

    }
}
