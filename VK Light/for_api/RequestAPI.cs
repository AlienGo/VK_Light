//using System;
//using System.Net;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Shapes;

//using System.Collections.Generic;
//using System.Globalization;

namespace VK_Light.for_api
{
    //Здесь храним различные настройки приложения(для сетевой части и для интерфейса)
    struct ApplicationSettings
    {
        #region Сетевые настройки

        public const string adress = "https://api.vk.com/method/";
        public const string client_id = "client_id=3070558";
        public const string client_secret = "client_secret=XgdytvpYS2BRJavEEd1F";

        #endregion

        #region Интерфейс(размеры)
        
        public const int minPasswordLengh = 6;
        public const double inputPortaitSize = 450.0;
        public const double inputLandscapeSize = 630.0;


        public const double conversationPortaitSize = 264.0;
        public const double conversationLandscapeSize = 440;
        #endregion

    }
    struct MyUserData
    {
        public static string access_token;
        public static int user_id;
    }
    public class RequestAPI
    {
        public RequestAPI()
        {
        }
        
        public static string Request(string method, params string[] parametrs)
        {
            string requestReady = "";
            if (parametrs.Length > 0)
            {
                string parametrsReady = parametrs[0];
                for (int i = 1; i < parametrs.Length; i++)
                {
                    parametrsReady = parametrsReady + "&" + parametrs[i];
                }
                requestReady = ApplicationSettings.adress + method + "?" + parametrsReady;
                
            }
            else
            {
                requestReady = ApplicationSettings.adress + method;
            }
            return requestReady;
        }

    }

    //public class ResponseAPI
    //{
        
    //    public string Response{ get; set; }
    //    public string Error_code{ get; set; }
    //    public string Error_msg{ get; set; }
    //}



}
