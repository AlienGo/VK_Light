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

using System.IO.IsolatedStorage;
using VK_Light.for_api;

namespace VK_Light
{
    public partial class Settings : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();

        public Settings()
        {
            InitializeComponent();

        }
        /// <summary>
        /// Обработка выхода из аккаунта:
        /// Очищаем данные о пользователе(Isolate Storage, объект MyUserData)
        /// Переходим на старницу входа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            //Проверка, уверен ли пользователь, что хочет выйти
            MessageBoxResult answer = MessageBox.Show("Вы действиетльно хотите выйти из аккаунта?", "подтверждение выхода", MessageBoxButton.OKCancel);
            if (answer.Equals(MessageBoxResult.OK))
            {
                MyUserData.user_id = 0;
                MyUserData.access_token = "";
                settings.Remove("user_id");
                settings.Remove("access_token");
                //+ удалить остальные данные из isolate storage
                fileStorage.DeleteFile("Data\\FriendsContacts.json");
                NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
            }
            
        }
    }
}
