using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.ViewPage.Menu.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Navigator
{
    public interface INav
    {
        event EventHandler<MenuItemPageNavigateEventArgs> OnPageNavigateChanged;
        void SetLanguage(LanguageCode language);
        void AttachToFrame(Frame container, LanguageCode? language = null);
        void ShowNavigator();
        void HideNavigator();
        Visibility IsPreviousVisible { get; set; }
    }
}
