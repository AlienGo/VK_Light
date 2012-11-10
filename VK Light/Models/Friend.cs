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

namespace VK_Light.Models
{
    public class Friend
    {
        public int Uid { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Full_Name
        {
            get
            {
                return First_name + " " + Last_name;
            }
        }

        private string _phone;
        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                _phone = value.TrimStart('+');
            }
        }

        string _photo;
        /// <summary>
        /// Выдаётся url квадратной фотографии пользователя, имеющей ширину 50 пикселей. 
        /// В случае отсутствия у пользователя фотографии выдаётся ответ: "camera_c.jpg" или "deactivated_c.jpg"
        /// </summary>
        public string Photo_rec
        {
            get { return _photo; }
            set
            {
                if (value.EndsWith("camera_c.gif"))
                    _photo = "/Components/From_vk/camera_c.jpg";
                else if (value.EndsWith("deactivated_c.gif"))
                    _photo = "/Components/From_vk/deactivated_c.jpg";
                else
                    _photo = value;
            }
        }

        string _photoMedium;
        /// <summary>
        /// Выдаётся url квадратной фотографии пользователя, имеющей ширину 100 пикселей. 
        /// В случае отсутствия у пользователя фотографии выдаётся ответ: "camera_b.jpg" или "deactivated_b.jpg"
        /// </summary>
        public string Photo_medium_rec
        {
            get { return _photoMedium; }
            set
            {
                if (value.EndsWith("camera_b.gif"))
                    _photoMedium = "/Components/From_vk/camera_b.jpg";
                else if (value.EndsWith("deactivated_b.gif"))
                    _photoMedium = "/Components/From_vk/deactivated_b.jpg";
                else
                    _photoMedium = value;
            }
        }

        string _photoBig;
        /// <summary>
        /// Выдаётся url фотографии пользователя, имеющей ширину 200 пикселей. 
        /// В случае отсутствия у пользователя фотографии выдаётся ответ: "camera_b.jpg" или "deactivated_b.jpg"
        /// </summary>
        public string Photo_big
        {
            get { return _photoBig; }
            set
            {
                if (value.EndsWith("camera_b.gif"))
                    _photoBig = "/Components/From_vk/camera_b.jpg";
                else if (value.EndsWith("deactivated_b.gif"))
                    _photoBig = "/Components/From_vk/deactivated_b.jpg";
                else
                    _photoBig = value;
            }
        }

        /// <summary>
        ///Показывает, находится ли этот пользователь сейчас на сайте. Поле доступно только для метода friends.get. 
        ///Возвращаемые значения: 1 - находится, 0 - не находится.  
        /// </summary>
        public short Online { get; set; }

        public char Online_char
        {
            get
            {
                if (Online == 1)
                {
                    return '•';
                }
                else
                {
                    return ' ';
                }
            }
        }
        ///// <summary>
        ///// Разрешено ли написание личных сообщений данному пользователю.
        ///// </summary>
        //public int can_write_private_message { get; set; }
    }
}
