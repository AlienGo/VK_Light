
//#define DATABASE
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

using VK_Light.for_api;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;

using System.IO;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;

using Microsoft.Phone.UserData;
using Microsoft.Phone.Tasks;
using VK_Light.Models;
//UnixTime.UnixTimeToDateTime("11111"); подумать как таки отобразить время

//Контакты(сохранение и извлечение из IS ) + страницы с приглашением (или возможностью писать пользователю) + метод отправки смс(предложить установить программу)
//Поиск разный
//Мелкие кнопки друзей и сообщений (+ переход, макет страниц)
//Непрочитанные сообщения акцент колором
//переход на страницу диалогов
//getHistory

namespace VK_Light
{
    public partial class Conversation : PhoneApplicationPage
    {
        #region Поля, инициализация
        ProgressIndicator prog;
        App myApp = (Application.Current as App);
        IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
        /// <summary>
        /// Здесь наш ответ по Friend
        /// </summary>
        string jsonFriends;
        /// <summary>
        /// Здесь наш ответ по Message
        /// </summary>
        string jsonMessages;

        List<Friend> friends;
        List<Friend> contactFriends;
        #endregion
        public Conversation()
        {
            InitializeComponent();
            #region Стартуем индикатор прогресса загрузки
            prog = new ProgressIndicator();
            prog.Text = "Подождите...";
            #endregion

            //Применяем нашу тему для иконки
            imageLogotype.Source = new BitmapImage(new Uri(MyTheme.VK_logotype, UriKind.RelativeOrAbsolute));

            //Проверка синхронизированы ли контакты. Если да - отображение
            LoadContactsFromIS();

            friends = new List<Friend>();
            //Запускаем методы для получения и отображения данных(загрузка друзей, внутри нее грузятся сообщения)
            LoadFriends();
            //LoadMessage();
        }
        #region навигация, повороты, app bar

        /// <summary>
        /// (Пользователь заходит на нашу Pivot страницу контактов)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string answer;
            NavigationContext.QueryString.TryGetValue("answer", out answer);

            if (answer == "delete_friend")
            {
                MessageBox.Show("have answer: " + answer);
            }
        }
        //Перегружаем кнопку, чтобы из данного окна выходить из приложения
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //Проверка, уверен ли пользователь, что хочет выйти
            MessageBoxResult answer = MessageBox.Show("Вы действиетльно хотите выйти из приложения?","подтверждение выхода",MessageBoxButton.OKCancel);
            if (answer.Equals(MessageBoxResult.OK))
                {
                if (NavigationService.CanGoBack)
                {
                    while (NavigationService.RemoveBackEntry() != null)
                    {
                        NavigationService.RemoveBackEntry();
                    }
                }
                base.OnBackKeyPress(e);
            }
        }
        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings.xaml", UriKind.Relative));
        }
        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if ((e.Orientation == PageOrientation.LandscapeRight) || (e.Orientation == PageOrientation.LandscapeLeft))
            {
                canvasMessage.Margin = new Thickness(ApplicationSettings.conversationLandscapeSize, 0, 0, 0);
            }
            else if ((e.Orientation == PageOrientation.PortraitDown) || (e.Orientation == PageOrientation.PortraitUp))
            {
                canvasMessage.Margin = new Thickness(ApplicationSettings.conversationPortaitSize, 0, 0, 0);
            }
        }
        #endregion
        #region Загрузка, Обновление списка друзей. Запускает загрузку/обновление списка сообщений
        private void LoadFriends()
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            //Формируем запрос
            string fields = "fields=uid,first_name,last_name,photo_rec,photo_medium_rec,photo_big,online";
            string access_token = "access_token=" + MyUserData.access_token;
            string order = "order=name";
            string adr = RequestAPI.Request("friends.get", fields, order, access_token);

            //Отправляем запрос, получаем ответ. Запускаем событие обработки.
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadFriendsCompleted);
            client.DownloadStringAsync(new Uri(adr));
        }
        private void client_DownloadFriendsCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                jsonFriends = e.Result;
                ParseAndShowFriends(jsonFriends);
                    
                //ЗДЕСЬ вызывает функция подгрузки сообщений(т.к. необходимо получать фото, состояние и имя из друзей, по заданному uid)
                LoadMessage();
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        private void UpdateFriends()
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            //Формируем запрос
            string fields = "fields=uid,first_name,last_name,photo_rec,photo_medium_rec,photo_big,online";
            string access_token = "access_token=" + MyUserData.access_token;
            string order = "order=name";
            string adr = RequestAPI.Request("friends.get", fields, order, access_token);

            //Отправляем запрос, получаем ответ. Запускаем событие обработки.
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_UpdateFriendsCompleted);
            client.DownloadStringAsync(new Uri(adr));

        }
        private void client_UpdateFriendsCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string json = e.Result;
                if (json.Equals(jsonFriends))
                //if(json.GetHashCode()==jsonFriends.GetHashCode())
                {
                    MessageBox.Show("Друзья: Данные не обновлялись");
                    #region Progressbar OFF
                    prog.IsVisible = false;
                    prog.IsIndeterminate = false;
                    SystemTray.SetProgressIndicator(this, prog);
                    #endregion
                }
                else
                {
                    jsonFriends = json;
                    MessageBox.Show("Друзья: Данные обновлялись");

                    friends = new List<Friend>();
                    ParseAndShowFriends(json);
                }
                //Проверяем обновлялись ли сообщения
                UpdateMessage();
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        private void ParseAndShowFriends(string json)
        {
            //Парсим и разбираем наш результат
            JObject jRes = JObject.Parse(json);

            try
            {
                // Предствляем все наши контакты как список
                IList<JToken> results = jRes["response"].Children().ToList();

                // serialize JSON results into .NET objects
                
                foreach (JToken result in results)
                {
                    Friend friendResult = JsonConvert.DeserializeObject<Friend>(result.ToString());
                    friends.Add(friendResult);
                }
                //Отображаем результат
                listBox2.ItemsSource = friends;

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
                    MessageBox.Show((string)jRes["error"]["error_msg"]);
                }
                catch (Exception Error2)
                {
                    MessageBox.Show(Error2.Message);
                }
            }
        }
        #endregion
        #region Загрузка, Обновление списка сообщений.
        private void LoadMessage()
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            short size = 20;
            //Формируем запрос
            string count = "count=" + size;
            string access_token = "access_token=" + MyUserData.access_token;//данные в MyUserData заносятся из словаря
            string adr = RequestAPI.Request("messages.get", count, access_token);

            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadMessagesCompleted);
            client.DownloadStringAsync(new Uri(adr));
        }
        private void client_DownloadMessagesCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                jsonMessages = e.Result;
                ParseAndShowMessages(jsonMessages);
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        private void UpdateMessage()
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            short size = 20;
            //Формируем запрос
            string count = "count=" + size;
            string access_token = "access_token=" + MyUserData.access_token;//данные в MyUserData заносятся из словаря
            string adr = RequestAPI.Request("messages.get", count, access_token);

            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_UpdateMessagesCompleted);
            client.DownloadStringAsync(new Uri(adr));
        }
        private void client_UpdateMessagesCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string json = e.Result;
                if (json.Equals(jsonMessages))
                {
                    MessageBox.Show("Сообщения: Данные не обновлялись");
                    #region Progressbar OFF
                    prog.IsVisible = false;
                    prog.IsIndeterminate = false;
                    SystemTray.SetProgressIndicator(this, prog);
                    #endregion
                }
                else
                {
                    jsonMessages = json;
                    MessageBox.Show("Сообщения: Данные обновлялись");
                    ParseAndShowMessages(json);
                }
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        private void ParseAndShowMessages(string json)
        {
            //Парсим и разбираем наш результат
            JObject jRes = JObject.Parse(jsonMessages);

            string error;
            List<Message> message = new List<Message>();
            try
            {
                // Предствляем все наши контакты как список
                IList<JToken> results = jRes["response"].Children().Skip(1).ToList();
                // JToken count =  jRes["response"].Children().Take(1);

                // serialize JSON results into .NET objects
                IList<Message> messageResults = new List<Message>();
                foreach (JToken result in results)
                {
                    Message messageResult = JsonConvert.DeserializeObject<Message>(result.ToString());

                    //Попробуем добавить корректные данные: фото, имя, статус. используюя Uid

                    //Поиск по Uid
                    Friend currentFriend = friends.FirstOrDefault(x => x.Uid == messageResult.Uid);
                    if (currentFriend != null)
                    {
                        messageResult.Name = currentFriend.Full_Name;
                        messageResult.Photo = currentFriend.Photo_medium_rec;
                        messageResult.Status = currentFriend.Online_char;
                    }
                    
                    //Добавляем софрмировашийся полный объект в списко
                    messageResults.Add(messageResult);
                }

                //Отображаем результат
                listBox1.ItemsSource = messageResults;

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
        }
        #endregion
        #region Загрузка контактов, синхронизация и проверка наличия в IS
        //При нажатии на кнопку синхронизации контактов
        private void buttonSyncContacts_Click(object sender, RoutedEventArgs e)
        {
            SyncBegin.Visibility = Visibility.Collapsed;

            //Выполняем операции по обработке данных
            //Запускаем задачу котактов в телефоне(создаем реальный контакт)
            //SaveContactTask save_girl = new SaveContactTask();
            //save_girl.FirstName = "Liza";
            //save_girl.LastName = "Gaeva";
            //save_girl.MobilePhone = "89216573799";
            //save_girl.Show();

            //SaveContactTask save_boy = new SaveContactTask();
            //save_boy.FirstName = "Stepan";
            //save_boy.LastName = "Risin";
            //save_boy.MobilePhone = "+79025853217";
            //save_boy.Show();

            SaveContactTask save_boy = new SaveContactTask();
            save_boy.FirstName = "Eugen";
            save_boy.LastName = "Test";
            save_boy.MobilePhone = "+79227087963";
            save_boy.Show();

            //Получаем все наши телефоны из телефонной книги, в виде списка
            Contacts cons = new Contacts();
            //Подписываемся на событие завершения получения контактов
            //В самом событии - весь алгоритм
            cons.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(Contacts_SearchCompleted);
            //Начинаем поиск(просто получаем все контакты)
            cons.SearchAsync(String.Empty, FilterKind.None, "Contacts Test #1");

            //Показываем данные
            contactShow.Visibility = Visibility.Visible;

            
        }
        //Выполняется по завершении поиска
        void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            //Наш список контактов из телефона
            List<Contact> myContacts = e.Results.ToList();

            //при помощи API можно получить идентификаторы пользователей, привязанные к этим номерам (friends.getByPhones) http://vk.com/pages?oid=-1&p=friends.getByPhones

            string allPhones="";
            int count=0;
            //Здесь сохраним интересующие нас поля
            contactFriends = new List<Friend>();

            //Получаем список телефонных номеров и заносим их 
            foreach (var contact in myContacts)
            {
                string currentPhone=string.Empty;
               var phoneList = contact.PhoneNumbers.ToList();
               foreach (var item in phoneList)
               {
                   currentPhone = item.PhoneNumber;
               }
                //формируем чать нашего списка(фио, фото, телефон)
               if (currentPhone != string.Empty)
               {
                   contactFriends.Add(new Friend
                   {
                       First_name = contact.DisplayName,
                       Phone = currentPhone,
                       //photo stream
                       Photo_medium_rec = "/Components/From_vk/camera_b.jpg",
                       Photo_big = "/Components/From_vk/camera_b.jpg",
                       Photo_rec = "/Components/From_vk/camera_c.jpg"
                   });
                   
                   //записываем данные в общую строку, увеличиваем счетчик кол-ва телефонов
                   allPhones += currentPhone + ",";
                   count++;
               }
               allPhones.TrimEnd(',');

#region ВАРИАНТ ДЛЯ ВСЕХ ТЕЛЕФОНОВ(СЛОЖНО ДЕЛАТЬ, возможно реализую позже)
                ////Текущие телефоны контакта
                //ContactPhoneNumber[] temp = contact.PhoneNumbers.ToArray();
                //foreach (var item in temp)
                //{
                //   allPhones+=","+item.PhoneNumber;
                //   count++;
                //}
#endregion
            }
            //В итоге у нас строка из всех телефонов пользователей подряд(телефоны одинакового пользователя рядом)
            //+ наш недосформировашийся список. Осталось добавит в него результат поиска по телефонам
            //Отправим запрос
            LoadContact(allPhones, count);

           // MessageBox.Show(e.Results.Count().ToString());
        }
        void LoadContact(string allPhones, int count)
        {
            #region Progress ON
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, prog);
            #endregion

            if (count < 1000)
            {
                //Формируем запрос

                allPhones = "phones=" + allPhones;
                string fields = "fields=photo_rec,photo_medium_rec";
                string access_token = "access_token=" + MyUserData.access_token;//данные в MyUserData заносятся из словаря
                string adr = RequestAPI.Request("friends.getByPhones", allPhones, fields, access_token);

                WebClient client = new WebClient();
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadContactsCompleted);
                client.DownloadStringAsync(new Uri(adr));
            }
            else
            {
                //реализовать разбиение на запросы
                MessageBox.Show("To big size of contact list. Sorry.");
            }
        }
        private void client_DownloadContactsCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string jsonContacts = e.Result;
                ParseContacts(jsonContacts);
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        void ParseContacts(string jsonContacts)
        {
            //Парсим и разбираем наш результат
            JObject jRes = JObject.Parse(jsonContacts);

            try
            {
                // Предствляем все наши контакты как список
                IList<JToken> results = jRes["response"].Children().ToList();

                // serialize JSON results into .NET objects
               // List<Friend> friendsVK = new List<Friend>();
                foreach (JToken result in results)
                {
                    //Получаем результат результат ответа из VK - объект Friend (Uid, Phone, Name)
                    Friend friendResult = JsonConvert.DeserializeObject<Friend>(result.ToString());

                    //friendsVK.Add(friendResult);

                    
                    //Friend currentFriend = contactFriends.FirstOrDefault(x => x.Phone == friendResult.Phone);

                    
                    //if (currentFriend != null)
                    //{
                    //    currentFriend.Last_Name = friendResult.Full_Name;
                    //    currentFriend.Uid = friendResult.Uid;
                    //}

                    //Ищем  в наших контактах соответсвие по телефону - получит еще 1 объект Friend
                    //Записываев в списко контактов всю нужную информацию
                    foreach (var item in contactFriends)
                    {
                        if (item.Phone == friendResult.Phone)
                        {
                            item.Last_name = friendResult.Full_Name;
                            item.Uid = friendResult.Uid;
                            item.Photo_medium_rec = friendResult.Photo_medium_rec;
                            item.Photo_rec = friendResult.Photo_rec;
                        }
                    }
                }
                //Cериализуем в строку
                string jsonContactsFriends = JsonConvert.SerializeObject(contactFriends, Formatting.Indented);
                //сохранение в IS
                //Создаем новый поток записи, для записи в заданную дирректорию
                try
                {
                    StreamWriter fileWriter = new StreamWriter(new IsolatedStorageFileStream(
                        "Data\\FriendsContacts.json", FileMode.OpenOrCreate, fileStorage));
                    fileWriter.WriteLine(jsonContactsFriends);
                    fileWriter.Close();
                }
                catch (Exception Error)
                {
                    MessageBox.Show(Error.Message);
                }

                //Отрисовка!
                listBox3.ItemsSource = contactFriends;

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
                    MessageBox.Show((string)jRes["error"]["error_msg"]);
                }
                catch (Exception Error2)
                {
                    MessageBox.Show(Error2.Message);
                }
            }
        }
        void LoadContactsFromIS()
        {
            if (fileStorage.FileExists("Data\\FriendsContacts.json"))
            {
                SyncBegin.Visibility = Visibility.Collapsed;

                StreamReader fileReader = null;
                try
                {
                    //Считываем файл
                    fileReader = new StreamReader(new IsolatedStorageFileStream("Data\\FriendsContacts.json", FileMode.Open, fileStorage));
                    string jsonContactsFriends = fileReader.ReadToEnd();

                    //{
                    //    fileStorage.OpenFile(,,,
                    //}

                    List<Friend> contactsFriends = JsonConvert.DeserializeObject<List<Friend>>(jsonContactsFriends);

                    listBox3.ItemsSource = contactsFriends;
                    fileReader.Close();
                }
                catch
                {
                    MessageBox.Show("error of load sync contact-friends");
                }

                contactShow.Visibility = Visibility.Visible;
            }
        }
        #endregion

        private void Update_Click(object sender, EventArgs e)
        {
            //Метод обновляет друзей и сообщения
            UpdateFriends();
        }

        //Здесь проверяется выбранный элемент и осущеустляется переход на соответсвующую ему страницу(синхронизирован он или нет)
        private void contactName_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MessageBox.Show("Tap, index=" + listBox3.SelectedIndex);
            Friend friend =  listBox3.SelectedItem as Friend;

            //if (friend.Last_name != string.Empty)
            //{
            //    MessageBox.Show("Elemet is Sync! Name = " + friend.Last_name);
            //}
            
            if (friend.Uid != 0)
            {
                myApp.Name = friend.Last_name;
                myApp.Photo = friend.Photo_big;
                myApp.Phone = friend.Phone;
                myApp.Uid = friend.Uid;
               NavigationService.Navigate(new Uri("/Pages/Profile/Friend_sync.xaml", UriKind.Relative));
            }
            else
            {
                myApp.Name = friend.First_name;
                myApp.Photo = friend.Photo_big;
                myApp.Phone = friend.Phone;
                NavigationService.Navigate(new Uri("/Pages/Profile/Invite.xaml", UriKind.Relative));
            }
        }

        //Переход к диалогу с пользователем(из сообщений)
        private void dialogShow_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //Создаем экземпляр выбранного объекта
            Message message = listBox1.SelectedItem as Message;

            //Проверка диалог это, или беседа с несколькими людьми
            if(message.Chat_id==0)
            {
               myApp.Uid = message.Uid;
               myApp.Name = message.Name;
               if (message.Status == '•')
               myApp.Status ="Online";
                else
                    myApp.Status = "Offline";
               NavigationService.Navigate(new Uri("/Pages/Dialog.xaml", UriKind.Relative));
            }
            else
            {   //пока не реализовано
                MessageBox.Show("Беседа");
                myApp.Chat_id = message.Chat_id;
                NavigationService.Navigate(new Uri("/Pages/DialogMulti.xaml", UriKind.Relative));
            }
        }

        //Переход к просмотру профиля друга(и возможность дальше общаться)
        private void friendsShow_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

    }

    
}
