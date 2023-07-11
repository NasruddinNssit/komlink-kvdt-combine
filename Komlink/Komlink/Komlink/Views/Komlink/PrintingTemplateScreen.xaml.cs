
using Komlink;
using Komlink.Models;
using System.Windows.Controls;

namespace komlink.Views.komlink
{
    /// <summary>
    /// Interaction logic for PrintingTemplateScreen.xaml
    /// </summary>
    public partial class PrintingTemplateScreen : UserControl
    {
        public PrintingTemplateScreen()
        {
            InitializeComponent();
            LoadLanguage();
        }

        private void LoadLanguage()
        {
            if (App.Language == "ms")
            {
                lblPrinting.Text = "CETAK TIKET ANDA";
                lblTransNo.Text = "No Rujukan: " + UserSession.SessionId;
                lblThankYou.Text = "Terima Kasih Dan Selamat Jalan";
                lblNiceDay.Text = "Semoga Hari Yang Menyenangkan";

                BtnContact.Content = "HUBUNGI BANTUAN";
                BtnPrint.Content = "CETAK RESIT";
            }
            lblTransNo.Text = "Reference No: " + UserSession.SessionId;
        }
    }
}
