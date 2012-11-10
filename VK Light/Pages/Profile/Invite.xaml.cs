using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;

namespace VK_Light
{
    public partial class Invite : PhoneApplicationPage
    {
        App myApp = (Application.Current as App);
        public Invite()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                titleName.Text = myApp.Name;
                photoMid.Source = new BitmapImage(new Uri(myApp.Photo, UriKind.RelativeOrAbsolute));
            }
            catch (Exception)
            {
                MessageBox.Show("Error of invite data");
            }
        }

        private void buttonInvite_Click(object sender, RoutedEventArgs e)
        {
            SmsComposeTask smstask = new SmsComposeTask();
            smstask.To = myApp.Phone;
            smstask.Body = "Привет! Предлагаю установить клевую программу для удобного общения Вконтакте на Windows Phone!";
            smstask.Show();
        }
    }
}
