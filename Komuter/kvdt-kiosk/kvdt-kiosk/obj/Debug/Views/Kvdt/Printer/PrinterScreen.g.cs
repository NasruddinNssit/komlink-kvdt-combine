﻿#pragma checksum "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "A9F4523A399A95B2EFDD4B13C5FEBD2F8195C7DACED1C1991AA1E9EC517B4543"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using kvdt_kiosk.Views.Kvdt.Printer;


namespace kvdt_kiosk.Views.Kvdt.Printer {
    
    
    /// <summary>
    /// PrinterScreen
    /// </summary>
    public partial class PrinterScreen : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox TxtPrinterList;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtPrinterName;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCheck;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCheck_Copy;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/kvdt-kiosk;component/views/kvdt/printer/printerscreen.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.TxtPrinterList = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 2:
            this.TxtPrinterName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.BtnCheck = ((System.Windows.Controls.Button)(target));
            
            #line 24 "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml"
            this.BtnCheck.Click += new System.Windows.RoutedEventHandler(this.BtnCheck_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 25 "..\..\..\..\..\Views\Kvdt\Printer\PrinterScreen.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.BtnCheck_Copy = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
