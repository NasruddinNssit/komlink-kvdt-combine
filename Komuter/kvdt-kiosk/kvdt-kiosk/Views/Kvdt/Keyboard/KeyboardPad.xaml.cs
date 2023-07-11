using System;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.Keyboard
{
    /// <summary>
    /// Interaction logic for KeyboardPad.xaml
    /// </summary>
    public partial class KeyboardPad : UserControl
    {

        public KeyboardPad()
        {
            InitializeComponent();
            TxtTextBox.IsReadOnly = true;

        }

        private void TxtTexbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtTextBox.Text = TxtTextBox.Text.ToUpper();
            TxtTextBox.SelectionStart = TxtTextBox.Text.Length;
        }

        private void BtnClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text = "";
        }

        private void BtnQ_Click(object sender, EventArgs e)
        {
            TxtTextBox.Text += "Q";

        }

        private void BtnW_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "W";

        }

        private void BtnE_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "E";

        }

        private void BtnR_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "R";

        }

        private void BtnT_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "T";

        }

        private void BtnY_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "Y";

        }

        private void BtnU_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "U";

        }

        private void BtnI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "I";

        }

        private void BtnO_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "O";

        }

        private void BtnP_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "P";

        }

        public class CharacterData
        {
            public char Character { get; set; }
        }

        private void BtnA_Click(object sender, EventArgs e)
        {
            TxtTextBox.Text += "A";


        }

        private void BtnS_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "S";

        }

        private void BtnD_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "D";

        }

        private void BtnF_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "F";

        }

        private void BtnG_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "G";

        }

        private void BtnH_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "H";

        }

        private void BtnJ_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "J";

        }

        private void BtnK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "K";

        }


        private void BtnL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "L";

        }

        private void BtnZ_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "Z";

        }

        private void BtnX_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "X";

        }

        private void BtnC_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "C";

        }

        private void BtnV_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "V";

        }

        private void BtnB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "B";

        }

        private void BtnN_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "N";

        }

        private void BtnM_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "M";

        }

        private void BtnQ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnQ.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnQ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnQ.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnW_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnW.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnW_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnW.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnE_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnE.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnE_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnE.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnR_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnR.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnR_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnR.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnT_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnT.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnT_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnT.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnY_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnY.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnY_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnY.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnU_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnU.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnU_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnU.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnI_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnI.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnI_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnI.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnO_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnO.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnO_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnO.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnP_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnP.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnP_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnP.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnA_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnA.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnA_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnA.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnS_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnS.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnS_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnS.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnD_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnD.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnD_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnD.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnF_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnF.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnF_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnF.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnG_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnG.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnG_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnG.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnH_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnH.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnH_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnH.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnJ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnJ.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnJ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnJ.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnK_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnK.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnK_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnK.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnL_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnL.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnL_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnL.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnZ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnZ.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnZ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnZ.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnX_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnX.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnX_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnX.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnC_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnC.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnC_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnC.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnV_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnV.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnV_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnV.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnB_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnB.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnB_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnB.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnN_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnN.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnN_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnN.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnM_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnM.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnM_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnM.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnClear_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnClear.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnClear_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnClear.Background = System.Windows.Media.Brushes.White;
        }

    }
}
