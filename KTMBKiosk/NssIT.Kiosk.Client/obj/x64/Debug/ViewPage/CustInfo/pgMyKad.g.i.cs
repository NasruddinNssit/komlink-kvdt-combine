﻿#pragma checksum "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "F8201A0C2AABE3FCB0FC229896AB7E1770D3EF4A7779CD9E9838811ED6ACCA43"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NssIT.Kiosk.Client.ViewPage.CustInfo;
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


namespace NssIT.Kiosk.Client.ViewPage.CustInfo {
    
    
    /// <summary>
    /// pgMyKad
    /// </summary>
    public partial class pgMyKad : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 40 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtPassengerNo;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtCountDown;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtInsertMyKad;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtRemoveMyKad;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border BdReadLed;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border BdOK;
        
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
            System.Uri resourceLocater = new System.Uri("/NssIT.Kiosk.Client;component/viewpage/custinfo/pgmykad.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
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
            
            #line 10 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
            ((NssIT.Kiosk.Client.ViewPage.CustInfo.pgMyKad)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Page_Loaded);
            
            #line default
            #line hidden
            
            #line 10 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
            ((NssIT.Kiosk.Client.ViewPage.CustInfo.pgMyKad)(target)).Unloaded += new System.Windows.RoutedEventHandler(this.Page_Unloaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.TxtPassengerNo = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.TxtCountDown = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.TxtInsertMyKad = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.TxtRemoveMyKad = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.BdReadLed = ((System.Windows.Controls.Border)(target));
            return;
            case 7:
            
            #line 67 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
            ((System.Windows.Controls.Border)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Button_Cancel);
            
            #line default
            #line hidden
            return;
            case 8:
            this.BdOK = ((System.Windows.Controls.Border)(target));
            
            #line 71 "..\..\..\..\..\ViewPage\CustInfo\pgMyKad.xaml"
            this.BdOK.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Button_OK);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

