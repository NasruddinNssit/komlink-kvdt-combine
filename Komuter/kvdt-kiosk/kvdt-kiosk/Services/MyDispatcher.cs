using System;

namespace kvdt_kiosk.Services
{
    public class MyDispatcher
    {

        public static void Invoke(Action action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(action);
        }


        public static void BeginInvoke(Action action, TimeSpan time)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(action, time);
        }
    }

}
