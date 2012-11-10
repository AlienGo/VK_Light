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

using Coding4Fun.Phone.Controls;

namespace VK_Light.Models
{
    public class Message
    {
        /// <summary>
        /// ID сообщения. Не передаётся для пересланных сообщений.
        /// </summary>
        public int Mid { get; set; }
        /// <summary>
        /// автор сообщения
        /// </summary>
        public int Uid { get; set; }
        /// <summary>
        /// дата отправки сообщения 
        /// Unix формат
        /// </summary>
        public int Date { get; set; }
        public string DateString
        {
            get
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dtDateTime = dtDateTime.AddSeconds(Date).ToLocalTime();
                return dtDateTime.Hour + ":" + dtDateTime.Minute;
            }
        }
        /// <summary>
        /// статус прочтения сообщения (0 – не прочитано, 1 – прочитано) Не передаётся для пересланных сообщений.
        /// </summary>
        public decimal Read_state { get; set; }
        /// <summary>
        /// тип сообщения (0 – полученное сообщение, 1 – отправленное сообщение). Не передаётся для пересланных сообщений.
        /// </summary>
        public decimal Out { get; set; }
        /// <summary>
        /// заголовок сообщения или беседы
        /// </summary>
        public string Title { get; set; }
        public string _body;
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Body
        {
            get
            {
                if (_body != null)
                    return _body.Replace("<br>", "\r\n");
                else
                    return null;
            }
            set
            {
                _body = value;
            }
        }
        //#region Если есть массив медиа-вложений, массив пересланных сообщений

        ///// <summary>
        ///// массив медиа-вложений
        ///// </summary>
        //public string[] Attachments { get; set; }
        ///// <summary>
        ///// массив пересланных сообщений
        ///// </summary>
        //public string[] Fwd_messages { get; set; }

        //#endregion
        //#region Только для групповых бесед
        /// <summary>
        /// ID беседы
        /// </summary>
        public int Chat_id { get; set; }
        /// <summary>
        /// ID последних участников беседы, разделённых запятыми, но не более 6.
        /// </summary>
        public string Chat_active { get; set; }
        /// <summary>
        /// количество участников в беседе
        /// </summary>
        public decimal Users_count { get; set; }
        ///// <summary>
        ///// ID создателя беседы
        ///// </summary>
        //public int admin_id { get; set; }
        //#endregion
        ///// <summary>
        ///// (только для удалённых сообщений) Удалено ли сообщение
        ///// </summary>
        //public string deleted { get; set; }
        ///// <summary>
        ///// если возвращается данное поле — значит текст сообщений содержит
        ///// символы emoji, которые необходимо отобразить.
        ///// http://vk.com/pages?oid=-1&p=%D0%9E%D1%82%D0%BE%D0%B1%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5_Emoji_%D1%81%D0%B8%D0%BC%D0%B2%D0%BE%D0%BB%D0%BE%D0%B2
        ///// </summary>
        //public string emoji { get; set; }

        ////
        //Для отображения вторичных данных по сообщению. Подумать как для бесед
        public string Name
        {
            get;
            set;
        }
        public string Photo
        {
            get;
            set;
        }
        public char Status
        {
            get;
            set;
        }
    }
}
