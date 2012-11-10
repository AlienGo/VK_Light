
#define SHOWDEBUGINFO

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
using Microsoft.Phone.Tasks;

using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;

//
//http://www.jeffblankenburg.com/category/31daysofwindowsphone/page/4
//и манго
//

//httpwebrequest
//Добавить звуки
//написать всплывающие подсказки
//sid
//перевод
//Exceptions
//scroll
//прогрес бар
// ** http://www.jeffblankenburg.com/2011/11/17/31-days-of-mango-day-17-using-windows-azure/
//-push notification and azure
// live titles http://www.jeffblankenburg.com/2011/11/11/31-days-of-mango-day-11-live-tiles/
namespace VK_Light
{
    public partial class Registration : PhoneApplicationPage
    {
        public Registration()
        {
            InitializeComponent();

            //Применяем нашу тему
            
            imageLogotype.Source = new BitmapImage(new Uri(MyTheme.VK_logotype, UriKind.RelativeOrAbsolute));
        }

        #region Активация кнопки "зарегистрироваться"(валидация данных на стороне клиента)

        /// <summary>
        /// Первый вариант функции проверки на заполненность полей ввода. Возможно не конечный вариант.
        /// Проверяет заполненность 3 полей. В случае успеха открывает доступ к кнопке регистрации. В случае неудачи - закрывает.
        /// </summary>
        private void checkRegistrationField()
        {
            if (phonenumBox.Text.Length > 0 && firstnameBox.Text.Length > 0 && lastnameBox.Text.Length > 0)
                buttonRegistration.IsEnabled = true;
            else
                buttonRegistration.IsEnabled = false;
        }

        private void phonenumBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //Проверка номера телефона методами API Вконтакте
            if (phonenumBox.Text != "")
            {
                //Формируем запрос
                string phone = "phone=" + phonenumBox.Text;
                string adr = RequestAPI.Request("auth.checkPhone", phone, ApplicationSettings.client_id, ApplicationSettings.client_secret);
    
                //Создаем клиент для передачи данных
                WebClient client = new WebClient();
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                client.DownloadStringAsync(new Uri(adr));
            }
            else if(phonenumBox.Text =="")
            {
                phoneBlock.Opacity = 0.6;
                phoneBlock.Foreground = (Brush)Resources["PhoneForegroundBrush"];
            }
        }
        /// <summary>
        /// Обработчик завершения проверки на сервере API. 
        /// Если проверка завершилась ошибкой - выводит ошибку , меняет цвет подписи над полем, меняет состояние кнопки на неактивное.
        /// Если успех - меняет цвет подписи над полем на стандартный, дает возможность набирать данные дальше.
        /// </summary>
        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {


            if (e.Error == null)
            {
                string json = e.Result;
                //Парсим и разбираем наш результат
                JObject jRes = JObject.Parse(json);
                int result;
                try
                {
                    result = (int)jRes["response"];
                    if (result == 1)//Успех
                    {
                        phoneBlock.Foreground = (Brush)Resources["PhoneForegroundBrush"];
                        checkRegistrationField();
                        //переносим введенный телефон в соседний pivot для подтверждения и упрощения работы пользователя
                        phoneConfirmBox.Text = phonenumBox.Text;
                    }
#if SHOWDEBUGINFO
                    MessageBox.Show("response = " + result);
#endif
                }
                catch (ArgumentNullException)
                {//В случае отсутсвия положительного результата выводим содержание ошибки

                    //запрещаем нажимать на кнопку, пока не введем норм данные
                    buttonRegistration.IsEnabled = false;
                    string error = (string)jRes["error"]["error_msg"];
#if SHOWDEBUGINFO
                    MessageBox.Show("Error: " + error);
#endif
                    //смена цвета
                    phoneBlock.Foreground = (Brush)Resources["PhoneAccentBrush"];
                    //throw;
                }
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        #endregion

        #region Первый шаг регистрации
        private void buttonRegistration_Click(object sender, RoutedEventArgs e)
        {
            //Формируем запрос
            string phone = "phone=" + phonenumBox.Text;
            string firstname = "first_name="+firstnameBox.Text;
            string lastname = "last_name="+lastnameBox.Text;
            string test_mode = "test_mode=0";

            string adr = RequestAPI.Request("auth.signup", phone, firstname, lastname, ApplicationSettings.client_id, ApplicationSettings.client_secret, test_mode);

            //Создаем клиент для передачи данных
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_signupCompleted);
            client.DownloadStringAsync(new Uri(adr));

            MessageBox.Show("Данны для регистрации отправлены! Дождитесь прихода SMS с кодом и введите его в окне подтверждения, для завершения регистрации");
        }

        /// <summary>
        /// Обработчик завершения первого шага регистрации.
        /// Если успех - получим sid.+ sms
        /// Если неуспех - сообщение об ошибке.
        /// </summary>
        /// Ключевые переменные : sid,error
        void client_signupCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                string json = e.Result;
                //Парсим и разбираем наш результат
                JObject jRes = JObject.Parse(json);
                string sid;
                string error;
                try
                {
                    sid =(string) jRes["response"]["sid"];
                    //if (sid==null)
                    //{
                    //    error = (string)jRes["error"]["error_msg"];    
                    //    MessageBox.Show(error);
                    //}
#if SHOWDEBUGINFO
                    MessageBox.Show("sid: " + sid);
#endif
                }
                catch (Exception Error)
                {
#if SHOWDEBUGINFO
                    MessageBox.Show("Error: " + Error.Message);
#endif
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
                    //throw;
                }
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }

        #endregion

        #region Второй шаг регистрации
        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            //Формируем запрос
            string phone = "phone=" + phoneConfirmBox.Text;
            string code = "code=" + CodeBox.Text;
            string test_mode = "test_mode=0";
            string password="password="+passwordBox.Password;

            string adr = RequestAPI.Request("auth.confirm", phone, code, password, ApplicationSettings.client_id, ApplicationSettings.client_secret, test_mode);
            //Создаем клиент для передачи данных
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_authConfirm);
            client.DownloadStringAsync(new Uri(adr));

        }
        /// <summary>
        /// Обработчик завершения второго шага регистрации.
        /// Если успех - сообщение об успехе и переход на страницу логина (MainPage.xaml)
        /// Если неуспех - сообщение об ошибке.
        /// </summary>
        /// Ключевые переменные : succes, uid
        void client_authConfirm(object sender, DownloadStringCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                string json = e.Result;
                //Парсим и разбираем наш результат
                JObject jRes = JObject.Parse(json);

                //поля для сохранения
                int succes;
                int uid;
                string error;
                try
                {
                    succes = (int)jRes["response"]["success"];
                    uid = (int)jRes["response"]["uid"];
                    if (succes ==0)
                    {//В случае отсутсвия succes, но наличия сообщения об ошибке с сервера - выводим его
                        error = (string)jRes["error"]["error_msg"];
                        MessageBox.Show(error);
                    }
#if SHOWDEBUGINFO
                    MessageBox.Show("succes: " + succes + "uid: " + uid);
#endif
                    NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                }
                catch (Exception Error)
                {

#if SHOWDEBUGINFO
                    MessageBox.Show("Error: " + Error.Message);
#endif
                    try
                    {
                        error = (string)jRes["error"]["error_msg"];
                        MessageBox.Show(error);
                    }
                    catch (Exception Error2)
                    {
#if SHOWDEBUGINFO
                        MessageBox.Show(Error2.Message);
#endif
                    }

                    //throw;
                }
            }
            else
            {
                MessageBox.Show("Error: " + e.Error);
            }
        }
        #endregion

        #region Активация кнопки "Подтвердить"(валидация данных на стороне клиента)

        /// <summary>
        /// Проверка на заполненность полей ввода подтверждения.
        /// Проверяет заполненность 4-х полей. В случае успеха открывает доступ к кнопке "Подтвердить". В случае неудачи - закрывает.+ подсветка ошибок
        /// </summary>
        private void checkConfirmFields()
        {
            if (CodeBox.Text.Length > 0 &&
                passwordBox.Password.Length >= ApplicationSettings.minPasswordLengh &&
                passwordConfirmBox.Password.Length >= ApplicationSettings.minPasswordLengh &&
                phoneConfirmBox.Text.Length > 0)
            {
                if (passwordConfirmBox.Password == passwordBox.Password)
                {
                    //если пароли совпадают , значит закрашиваем наши подписки в искомый цвет
                    passwordConfirmBlock.Foreground = (Brush)Resources["PhoneForegroundBrush"];
                    passwordBlock.Foreground = (Brush)Resources["PhoneForegroundBrush"];
                    buttonConfirm.IsEnabled = true;
                }
                else
                {
                    //закрашиваем наши подписки в accent цвет
                    passwordConfirmBlock.Foreground = (Brush)Resources["PhoneAccentBrush"];
                    passwordBlock.Foreground = (Brush)Resources["PhoneAccentBrush"];
                    buttonConfirm.IsEnabled = false;

                    MessageBox.Show("Введенные пароли не совпадают");
                }

            }
            else
            {
                buttonConfirm.IsEnabled = false;
                if (passwordBox.Password.Length > 0 &&passwordBox.Password.Length < ApplicationSettings.minPasswordLengh 
                    ||
                    passwordConfirmBox.Password.Length > 0 && passwordConfirmBox.Password.Length < ApplicationSettings.minPasswordLengh)
                    MessageBox.Show("Пароль должен быть больше либо равен 6 символам");
            }
        }

        private void CodeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            checkConfirmFields();
        }

        private void phoneConfirmBox_LostFocus(object sender, RoutedEventArgs e)
        {
            checkConfirmFields();
        }

        private void passwordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            checkConfirmFields();
        }

        private void passwordConfirmBox_LostFocus(object sender, RoutedEventArgs e)
        {
            checkConfirmFields();
        }
        #endregion

        #region Смена ориентации
                private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
                {
                    if ((e.Orientation == PageOrientation.LandscapeRight) || (e.Orientation == PageOrientation.LandscapeLeft))
                    {
                        phonenumBox.Width = ApplicationSettings.inputLandscapeSize;
                        firstnameBox.Width = ApplicationSettings.inputLandscapeSize;
                        lastnameBox.Width = ApplicationSettings.inputLandscapeSize;
                        phoneConfirmBox.Width = ApplicationSettings.inputLandscapeSize;
                        CodeBox.Width = ApplicationSettings.inputLandscapeSize;
                        passwordBox.Width = ApplicationSettings.inputLandscapeSize;
                        passwordConfirmBox.Width = ApplicationSettings.inputLandscapeSize;
                    }
                    else if ((e.Orientation == PageOrientation.PortraitDown) || (e.Orientation == PageOrientation.PortraitUp))
                    {
                        phonenumBox.Width = ApplicationSettings.inputPortaitSize;
                        firstnameBox.Width = ApplicationSettings.inputPortaitSize;
                        lastnameBox.Width = ApplicationSettings.inputPortaitSize;
                        phoneConfirmBox.Width = ApplicationSettings.inputPortaitSize;
                        CodeBox.Width = ApplicationSettings.inputPortaitSize;
                        passwordBox.Width = ApplicationSettings.inputPortaitSize;
                        passwordConfirmBox.Width = ApplicationSettings.inputPortaitSize;
                    }
                }
        #endregion

        #region Подсветка при вводе - Регистрация
        //еще есть в методе phonenumBox_LostFocus(...)
        private void phonenumBox_GotFocus(object sender, RoutedEventArgs e)
        {
            phoneBlock.Opacity = 1.0;
        }

        private void firstnameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            firstnameBlock.Opacity =1.0;
        }

        private void lastnameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            lastnameBlock.Opacity = 1.0;
        }

        private void firstnameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (firstnameBox.Text == "")
            firstnameBlock.Opacity = 0.6;

            checkRegistrationField();
        }

        private void lastnameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (lastnameBox.Text == "")
            lastnameBlock.Opacity = 0.6;

            checkRegistrationField();
        }
        #endregion

        #region Подсветка при вводе - Подтверждение (отключение в LostFocus)

        private void phoneConfirmBox_GotFocus(object sender, RoutedEventArgs e)
        {
            phoneConfirmBlock.Opacity = 1.0;
        }

        private void CodeBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CodeBlock.Opacity = 1.0;
        }

        private void passwordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordBlock.Opacity = 1.0;
        }

        private void passwordConfirmBox_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordConfirmBlock.Opacity = 1.0;
        }

        #endregion

    }
}