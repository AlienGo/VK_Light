using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using VK_Light.Models;

namespace VK_Light.Binders
{
    public class Binder : INotifyPropertyChanged
    {
        static Binder instance = null;
        static readonly object padlock = new object();

        public Binder()
        {
            Messages = new ObservableCollection<Message>();
        }

        public static Binder Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Binder();
                    }
                    return instance;
                }
            }
        }

        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    NotifyPropertyChanged("Messages");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => { PropertyChanged(this, new PropertyChangedEventArgs(info)); });
            }
        }
    }
}
