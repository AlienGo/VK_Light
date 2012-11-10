using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VK_Light.for_api
{
    public static class MyTheme
    {
        private static string req_ic;
        public static string Requests_Icon { get { return req_ic; } }

        private static string unread_mes;
        public static string UnreadMessages_Icon { get { return unread_mes; } }

        private static string vk_log;
        public static string VK_logotype { get { return vk_log;} }

        public static void AppTheme()
        {
            Visibility darkBackgroundVisibility =
                (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"];

            if (darkBackgroundVisibility == Visibility.Visible)
            {
                req_ic = "/Components/Requests_Icon.png";
                unread_mes = "/Components/UnreadMessages_Icon.png";
                vk_log = "/Components/VK_logotype.png";
            }
            else
            {
                req_ic = "/Components/Requests_Icon_Light.png";
                unread_mes = "/Components/UnreadMessages_Icon_Light.png";
                vk_log = "/Components/VK_logotype_Light.png";
            }
                
        }

    }
}
