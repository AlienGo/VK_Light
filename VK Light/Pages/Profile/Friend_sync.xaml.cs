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
using VK_Light.for_api;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Shell;

namespace VK_Light
{
    public partial class Friend_sync : PhoneApplicationPage
    {
        ProgressIndicator prog;
        App myApp = (Application.Current as App);
        public Friend_sync()
        {
            prog = new ProgressIndicator();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                titleName.Text = myApp.Name;
                photoBig.Source = new BitmapImage(new Uri(myApp.Photo, UriKind.RelativeOrAbsolute));
                textPhone.Text = myApp.Phone;
            }
            catch (Exception)
            {
                MessageBox.Show("Error of get invite data");
            }
        }

        private void buttonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Method of Send Message! for " + myApp.Uid);
        }

        private void buttonCall_Click(object sender, RoutedEventArgs e)
        {
            PhoneCallTask phoneCallTask = new PhoneCallTask();
            phoneCallTask.PhoneNumber = myApp.Phone;
            phoneCallTask.DisplayName = myApp.Name;
            phoneCallTask.Show();
        }

        private void menuItemDeleteFromFriends_Click(object sender, EventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show("Вы действиетльно хотите удалить пользователя "+myApp.Name+" из списка друзей?","подтверждение удаления",MessageBoxButton.OKCancel);
            if (answer.Equals(MessageBoxResult.OK))
            {
                try
                {
                    if (myApp.Uid != 0)
                    {
                        DeleteFromFriends(myApp.Uid);
                    }
                    else
                    {
                        throw new Exception("Incorrect Uid");
                    }
                }
                catch (Exception Error)
                {
                    MessageBox.Show(Error.Message);
                }

            }
        }

        void DeleteFromFriends(int uid)
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            //Формируем запрос
            string s_uid = "uid=" + uid;
            string access_token = "access_token=" + MyUserData.access_token;
            string adr = RequestAPI.Request("friends.delete", s_uid, access_token);

            //Отправляем запрос, получаем ответ. Запускаем событие обработки.
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DeleteFriendsCompleted);
            client.DownloadStringAsync(new Uri(adr));
        }

        private void client_DeleteFriendsCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string jsonFriend = e.Result;
                ParseAndDeleteFriend(jsonFriend);
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        private void ParseAndDeleteFriend(string json)
        {
            //Парсим и разбираем наш результат
            JObject jRes = JObject.Parse(json);

            try
            {
                // Предствляем все наши контакты как список
                decimal result = (decimal)jRes["response"];

                #region Progressbar OFF
                prog.IsVisible = false;
                prog.IsIndeterminate = false;
                SystemTray.SetProgressIndicator(this, prog);
                #endregion

                if (result == 1)
                {
                    MessageBox.Show("Пользователь успешно удален");
                    NavigationService.Navigate(new Uri("/Pages/Conversation.xaml?answer=delete_friend", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show("Incorrect answer from server");
                }


            }
            catch (Exception)
            {
                try
                {
                    MessageBox.Show((string)jRes["error"]["error_msg"]);
                }
                catch (Exception Error2)
                {
                    MessageBox.Show(Error2.Message);
                }
            }
        }
    }
}
