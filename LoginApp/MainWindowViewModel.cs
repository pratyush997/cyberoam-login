using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.ObjectModel;

namespace LoginApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string message;
        private ObservableCollection<Credential> _credentials;
        
        public ObservableCollection<Credential> Credentials
        {
            get { return this._credentials; }
            set
            {
                _credentials = value;
                NotifyPropertyChanged("Credentials");
            }
        }
      
        public string Message
        {
            set
            {
                message = value;
                NotifyPropertyChanged("Message");
            }
            get
            {
                return message;
            }
        }



        private string fileName = "data.json";
        string settingsDirectory;


        public MainWindowViewModel()
        {
            _credentials = new ObservableCollection<Credential>();

            settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            settingsDirectory = Path.Combine(settingsDirectory, "Enhanced Cyberoam Login Client");
            if (!Directory.Exists(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);
            

            LoadData();
        }

        public void SaveData()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<Credential>));
                
                FileInfo f = new FileInfo(Path.Combine(settingsDirectory, fileName));
                
                using (var stream = f.OpenWrite())
                {
                    serializer.WriteObject(stream, _credentials);
                }
            }
            catch { }
        }

        private void LoadData()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<Credential>));
                FileInfo f = new FileInfo(Path.Combine(settingsDirectory, fileName));
                var stream = f.OpenRead();
                _credentials = (ObservableCollection<Credential>)serializer.ReadObject(stream);
            }
            catch { }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String Message)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(Message));
        }

    }
}
