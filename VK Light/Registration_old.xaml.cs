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

using System.Xml.Linq;

using Microsoft.Phone.Tasks;

namespace VK_Light
{
    public partial class Registration : PhoneApplicationPage
    {
        public Registration()
        {
            InitializeComponent();
            
        }

        private void buttonRegistration_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Активация кнопки "зарегистрироваться"(валидация данных на стороне клиента)

        /// <summary>
        /// Первый вариант функции проверки на заполненность полей ввода. Возможно не конечный вариант.
        /// Проверяет заполненность 3 полей. В случае успеха открывает доступ к кнопке регистрации. В случае неудачи - закрывает.
        /// </summary>
        private void checkField()
        {
            if (phonenumBox.Text.Length != 0 && firstnameBox.Text.Length != 0 && lastnameBox.Text.Length != 0)
                buttonRegistration.IsEnabled = true;
            else
                buttonRegistration.IsEnabled = false;
        }

        /// <summary>
        /// Проверка номера телефона.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void phonenumBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkField();

        }

        private void phonenumBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //Проверка номера телефона методами API Вконтакте

            //Формируем запрос
            string phone = "phone="+phonenumBox.Text;
            string adr = RequestAPI.Request("auth.checkPhone.xml", phone, ApplicationSettings.client_id, ApplicationSettings.client_secret);

            //Создаем клиент для передачи данных
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            client.DownloadStringAsync(new Uri(adr));

        }
        /// <summary>
        /// Обработчик завершения проверки на сервере API. Если проверка завершилась ошибкой - выводит ошибку и передает фокус текущему полю с номером телефона.
        /// Если успех - 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                string response = e.Result;

                //MessageBox.Show(response);
                XDocument twitterDoc = XDocument.Parse(e.Result);

                string result = "";
                var temp=twitterDoc.Descendants("response");
                foreach (var item in temp)
                {
                   result=item.Value;
                }

                if (result != "1")
                {
                    var postList = twitterDoc.Descendants("error_msg");
                    foreach (var item in postList)
                    {
                        result = item.Value;
                    }
                    MessageBox.Show("Error:"+result);
                    phoneBlock.Foreground = new SolidColorBrush(Colors.Orange);
                    phonenumBox.Focus();
                }
                else
                    phoneBlock.Foreground = new SolidColorBrush(Colors.White);

            }
            else
            {
                MessageBox.Show("error: " + e.Error);
            }
        }

        /// <summary>
        /// Проверка наличия имени.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void firstnameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkField();
            //прочая валидация на клиенте
        }
        /// <summary>
        /// Проверка наличия фамилии.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lastnameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkField();
            //прочая валидация на клиенте
        }
        #endregion


    }
}