﻿#pragma checksum "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "E8A8BE4637C6579CD9DD580442ECDDC05F260B588C068213D13A4831B1E08427"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NssIT.Kiosk.Client.ViewPage.Alert;
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


namespace NssIT.Kiosk.Client.ViewPage.Alert {
    
    
    /// <summary>
    /// pgOutofOrder
    /// </summary>
    public partial class pgOutofOrder : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 73 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run TxtMalMsg;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run TxtEngMsg;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtProblemMsg;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtStartTimeStr;
        
        #line default
        #line hidden
        
        
        #line 101 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtTimeStr;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtSysVer;
        
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
            System.Uri resourceLocater = new System.Uri("/NssIT.Kiosk.Client;component/viewpage/alert/pgoutoforder.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
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
            
            #line 11 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Alert.pgOutofOrder)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Page_Loaded);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\..\ViewPage\Alert\pgOutofOrder.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Alert.pgOutofOrder)(target)).Unloaded += new System.Windows.RoutedEventHandler(this.Page_Unloaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.TxtMalMsg = ((System.Windows.Documents.Run)(target));
            return;
            case 3:
            this.TxtEngMsg = ((System.Windows.Documents.Run)(target));
            return;
            case 4:
            this.TxtProblemMsg = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.TxtStartTimeStr = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.TxtTimeStr = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.TxtSysVer = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
