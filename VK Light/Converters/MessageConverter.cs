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

using System.Windows.Data;
using Coding4Fun.Phone.Controls;
using VK_Light.Binders;
using VK_Light.Models;

namespace VK_Light.Converters
{
    public class MessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //int ival = Binder.Instance.Messages.IndexOf((Message)value);
            Message temp = (Message)value;
            decimal check = temp.Out;

            if (check == 1)
            {
                if (parameter == null)
                    return 1; // no parameter - return opacity
                else
                {
                    if (parameter.ToString() == "direction") // return chat "triangle" direction
                    {
                        return ChatBubbleDirection.LowerRight;
                    }
                    else if (parameter.ToString() == "align")
                    {
                        return HorizontalAlignment.Right;
                    }
                    else if (parameter.ToString() == "column")
                    {
                        //column = 1
                        return "1";
                    }
                    else
                    {
                        //windth = 350*
                        return "300";
                    }
                }
            }
            else
            {
                if (parameter == null)
                    return .75;
                else
                {
                    if (parameter.ToString() == "direction")
                    {
                        return ChatBubbleDirection.UpperLeft;
                    }
                    else if (parameter.ToString() == "align")
                    {
                        return HorizontalAlignment.Left;
                    }
                    else if (parameter.ToString() == "column")
                    {
                        return "0";
                    }
                    else
                    {
                        //windth = 50
                        return "100";
                    }
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
