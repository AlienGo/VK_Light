//#define SHOWDEBUGINFO
//#define TEST
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
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

using Microsoft.Phone.Shell;

using MemoryDiagnostics;
using Microsoft.Phone.Info;

using System.IO.IsolatedStorage;
//Нужна ли капча? вроде работает и без нее
//Обработать действие при нажатии кнопки назад

//progress bar везьде сделать в отдельный метод(app.xaml.cs ?)
//быстрый прогресс бар таки найти(оказалось что в манго он не нужен)
//Проверить еще раз индикатор
//Tap, DoubleTap, and Hold

//WebClient can be easier to use because it returns result data to your application on the UI thread, and therefore your application does not need to manage the marshaling of data to the UI thread itself. However, if your application processes the web service data on the UI thread, the UI will be unresponsive until the processing is complete, causing a poor user experience, especially if the set of data being processed is large.
//Use HttpWebRequest instead of WebClient

//Use HttpWebRequest to make Web service requests. Process the resulting data on the asynchronous request thread and then use BeginInvoke to marshal the data to the UI thread.
namespace VK_Light
{	
    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator prog;

        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        // Конструктор
        public MainPage()
        {
            InitializeComponent();
            prog = new ProgressIndicator();

            //Применяем нашу тему
            imageLogotype.Source = new BitmapImage(new Uri(MyTheme.VK_logotype, UriKind.RelativeOrAbsolute));

        }

        //Перегружаем кнопку, чтобы из данного окна выходить из приложения
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            
            if (NavigationService.CanGoBack)
            {
                while (NavigationService.RemoveBackEntry() != null)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
           // e.Cancel = true;  //Cancels the default behavior.
            base.OnBackKeyPress(e);
        }

        /// <summary>
        /// Действия при смене ориентации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if ((e.Orientation == PageOrientation.LandscapeRight) || (e.Orientation == PageOrientation.LandscapeLeft))
            {
                buttonRegistration.Margin = new Thickness(0, 25, 0, 0);

                loginBox.Width = ApplicationSettings.inputLandscapeSize;
                passwordBox.Width = ApplicationSettings.inputLandscapeSize;
            }
            else if ((e.Orientation == PageOrientation.PortraitDown) || (e.Orientation == PageOrientation.PortraitUp))
            {
                buttonRegistration.Margin = new Thickness(0, 130, 0, 0);

                loginBox.Width = ApplicationSettings.inputPortaitSize;
                passwordBox.Width = ApplicationSettings.inputPortaitSize;

            }
            //throw new NotImplementedException();
        }

        private void buttonRegistration_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Registration.xaml", UriKind.Relative));
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            prog.Text = "Подождите...";

            SystemTray.SetProgressIndicator(this, prog);
            
            //Формируем запрос
            string mainPart="https://api.vk.com/oauth/token?grant_type=password&client_id=3070558&client_secret=XgdytvpYS2BRJavEEd1F";

#if TEST
            string username ="&username=";
            string password = "&password=";
            //int scope = 1 + 2 + 4 + 8 + 4096;
            string scope = "&scope=4111";
            //заменить на получение пароля и телефона с полей
#endif
            string username = "&username="+loginBox.Text;
            string password = "&password="+passwordBox.Password;
            //int scope = 1 + 2 + 4 + 8 + 4096;
            string scope = "&scope=4111";

            string adr = mainPart+username+password+scope;
            //Создаем клиент для передачи данных
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_RequestCompleted);
            try
            {
                client.DownloadStringAsync(new Uri(adr));
            }
            catch (Exception)
            {
                MessageBox.Show("Connection Error");
                prog.IsVisible = false;
                prog.IsIndeterminate = false;

                SystemTray.SetProgressIndicator(this, prog);
            }
            
        }
        void client_RequestCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                string json = e.Result;
                //Парсим и разбираем наш результат
                JObject jRes = JObject.Parse(json);
                //string access_token;
                //int expires_in;
                //int user_id;
                try
                {
                    MyUserData.access_token = (string)jRes["access_token"];
                    MyUserData.user_id =(int)jRes["user_id"];
                    //access_token = (string)jRes["access_token"];
                    //expires_in = (int)jRes["expires_in"];//0, возможно не стоит собирать?
                    //user_id = (int)jRes["user_id"];

                    //УСПЕХ!
                    //Cохраняем данные в IsolateStorage, выводим тестовую инфу, завершаем прогрес бар,  и переходим на диалоги
                    settings["user_id"] = MyUserData.user_id;
                    settings["access_token"] = MyUserData.access_token;
#if SHOWDEBUGINFO
                    MessageBox.Show("access_token = " + MyUserData.access_token + " user_id=" + MyUserData.user_id +"settings:"+(string)settings["access_token"]);
#endif
                    prog.IsVisible = false;
                    prog.IsIndeterminate = false;
                    //prog.Text = "Подождите...";
                    SystemTray.SetProgressIndicator(this, prog);

                    NavigationService.Navigate(new Uri("/Pages/Conversation.xaml", UriKind.Relative));
                }
                catch (Exception)
                {//Разбираем дальше. Смотрим есть ли капча
                    string error;
                    string captcha_sid;
                    string captcha_img;

                    try
                    {
                        error = (string)jRes["error"];
                        captcha_sid = (string)jRes["captcha_sid"];
                        captcha_img = (string)jRes["captcha_img"];

                        //Если понадобится капча
                        //передаем данные о капче в окно, для составления повторного запроса
                        //1.Построить интерфейс(для панорамы и стандартного)
                        //2.Передать параметры в картинку и тп(сделать кнопки видимыми)
                        //3.Сделать еще 1 кнопку для повторного отправления запроса(или эту перегрузить)
                        //4.Оформить в ней новый запрос с данными
                    }
                    catch (Exception Error2)
                    {//нет ни данных об успешной авторизации, ни данных о капче(ошибках)
                        MessageBox.Show(Error2.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }

        #region Активация кнопки "Вход"-->
        /// <summary>
        /// Проверка на заполненость полей
        /// </summary>
        void checkField()
        {
            if (loginBox.Text.Length > 0 && passwordBox.Password.Length >= ApplicationSettings.minPasswordLengh)
                buttonLogin.IsEnabled = true;
            else
                buttonLogin.IsEnabled = false;
        }

        private void loginBox_LostFocus(object sender, RoutedEventArgs e)
        {
            checkField();
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            checkField();
        }

        private void passwordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password.Length < ApplicationSettings.minPasswordLengh)
                MessageBox.Show("Пароль должен быть больше 5 символов");
        }
    }
    #endregion


}