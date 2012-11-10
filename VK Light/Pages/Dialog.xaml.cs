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
using Microsoft.Phone.Shell;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using VK_Light.Models;
using VK_Light.for_api;
using Coding4Fun.Phone.Controls;
using VK_Light.Binders;

namespace VK_Light.Pages
{
    public partial class Dialog : PhoneApplicationPage
    {
        ProgressIndicator prog;
        App myApp = (Application.Current as App);
        Message messageSent;
        public Dialog()
        {
            #region Стартуем индикатор прогресса загрузки
            prog = new ProgressIndicator();
            prog.Text = "Подождите...";
            #endregion

            InitializeComponent();
            TitlePanel.DataContext = myApp;

            Binder.Instance.Messages.Clear();
            LoadDialog();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
        }
        #region Загрузка диалога
        void LoadDialog()
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            //Формируем запрос
            string uid = "uid=" + myApp.Uid;
            //string count = "count=10";
           // string rev = "rev=1";
            string access_token = "access_token=" + MyUserData.access_token;

            ///смещение, необходимое для выборки определенного подмножества сообщений.
            //string offset = "offset=";
            ///идентификатор сообщения, начиная с которго необходимо получить последующие сообщения.
            //string start_mid="start_mid=";

            string adr = RequestAPI.Request("messages.getHistory",uid, access_token);

            //Отправляем запрос, получаем ответ. Запускаем событие обработки.
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadDialogCompleted);
            client.DownloadStringAsync(new Uri(adr));
        }
        private void client_DownloadDialogCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string jsonDialog = e.Result;
                ParseAndShowDialog(jsonDialog);
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        private void ParseAndShowDialog(string jsonDialog)
        {
            //Парсим и разбираем наш результат
            JObject jRes = JObject.Parse(jsonDialog);
            
            string error;
            List<Message> message = new List<Message>();
            try
            {
                // Предствляем все наши контакты как список
                IList<JToken> results = jRes["response"].Children().Skip(1).ToList();

                // serialize JSON results into .NET objects
               // IList<Message> messageResults = new List<Message>();
                foreach (JToken result in results)
                {
                    Message messageResult = JsonConvert.DeserializeObject<Message>(result.ToString());

                    Binder.Instance.Messages.Add(messageResult);
                    //Добавляем софрмировашийся полный объект в список
                    //messageResults.Add(messageResult);
                }
                
                //Отображаем результат
                //listBox1.ItemsSource = messageResults;

                #region Progressbar OFF
                prog.IsVisible = false;
                prog.IsIndeterminate = false;
                SystemTray.SetProgressIndicator(this, prog);
                #endregion
            }
            catch (Exception)
            {
                //В случае отсутсвия sid, но наличия сообщения об ошибке с сервера - выводим его
                try
                {
                    error = (string)jRes["error"]["error_msg"];
                    MessageBox.Show(error);
                }
                catch (Exception Error2)
                {
                    MessageBox.Show(Error2.Message);
                }
            }




            ////Попробуем добавить корректные данные: фото, имя, статус. используюя Uid

            ////Поиск по Uid
            //Friend currentFriend = friends.FirstOrDefault(x => x.Uid == messageResult.Uid);
            //if (currentFriend != null)
            //{
            //    messageResult.Name = currentFriend.Full_Name;
            //    messageResult.Photo = currentFriend.Photo_medium_rec;
            //    messageResult.Status = currentFriend.Online_char;
            //}

        }
        #endregion

        #region Обновление списка сообщений

        #endregion

        #region Отправка сообщения
        void SendMessage()
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            //Формируем запрос
            string uid = "uid=" + myApp.Uid;
            messageSent = new Message() { Body = textSend.Text };
            string message = "message=" + textSend.Text;//настроить форматирование!!! Проверка на длинну!
            string access_token = "access_token=" + MyUserData.access_token;

            ///смещение, необходимое для выборки определенного подмножества сообщений.
            //string offset = "offset=";
            ///идентификатор сообщения, начиная с которго необходимо получить последующие сообщения.
            //string start_mid="start_mid=";

            string adr = RequestAPI.Request("messages.send", uid, message, access_token);

            //Отправляем запрос, получаем ответ. Запускаем событие обработки.
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_SendMessageCompleted);
            client.DownloadStringAsync(new Uri(adr));
        }
        private void client_SendMessageCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ParseAndShowResponse(e.Result);
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        private void ParseAndShowResponse(string jsonDialog)
        {
            //Парсим и разбираем наш результат
            JObject jRes = JObject.Parse(jsonDialog);

            string error;
            try
            {
                // Предствляем все наши контакты как список
                messageSent.Mid = (int)jRes["response"];
                messageSent.Date = System.DateTime.Now.Hour;
                messageSent.Out = 1;
                Binder.Instance.Messages.Add(messageSent);
                //Отображаем результат
                //listBox1.ItemsSource = messageResults;


            }
            catch (Exception)
            {
                //В случае отсутсвия sid, но наличия сообщения об ошибке с сервера - выводим его
                try
                {
                    error = (string)jRes["error"]["error_msg"];
                    MessageBox.Show(error);
                }
                catch (Exception Error2)
                {
                    MessageBox.Show(Error2.Message);
                }
            }
            finally
            {
                #region Progressbar OFF
                prog.IsVisible = false;
                prog.IsIndeterminate = false;
                SystemTray.SetProgressIndicator(this, prog);
                #endregion
            }




            ////Попробуем добавить корректные данные: фото, имя, статус. используюя Uid

            ////Поиск по Uid
            //Friend currentFriend = friends.FirstOrDefault(x => x.Uid == messageResult.Uid);
            //if (currentFriend != null)
            //{
            //    messageResult.Name = currentFriend.Full_Name;
            //    messageResult.Photo = currentFriend.Photo_medium_rec;
            //    messageResult.Status = currentFriend.Online_char;
            //}

        }
        #endregion
        //Очищаем поле ввода, когда получаем фокус
        private void textSend_GotFocus(object sender, RoutedEventArgs e)
        {
            textSend.Text = string.Empty;
        }
        //Обработка нажатия на Enter(очистка поля ввода, отрисовка, отправленеи данных



        private void dialogShow_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show(listBox1.SelectedItems.Count.ToString());
        }

        private void textSend_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
                textSend.Text = "введите сообщение";
                //добавить фокусировку на последнем сообщении
            }
        }



    }
}