﻿#pragma checksum "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B4B59B83C0BA48093126DFEB94DA39499D3B75773DA20921F4F627157B842145"
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
using kvdt_kiosk.Views.PurchaseTicket;


namespace kvdt_kiosk.Views.PurchaseTicket {
    
    
    /// <summary>
    /// GenericStationButton
    /// </summary>
    public partial class GenericStationButton : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 42 "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GridBtnStation;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border StationColorCode;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid BtnGenericStation;
        
        #line default
        #line hidden
        
        /// <summary>
        /// LblStationName Name Field
        /// </summary>
        
        #line 49 "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.TextBlock LblStationName;
        
        #line default
        #line hidden
        
        /// <summary>
        /// LblStationId Name Field
        /// </summary>
        
        #line 51 "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.TextBlock LblStationId;
        
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
            System.Uri resourceLocater = new System.Uri("/kvdt-kiosk;component/views/purchaseticket/genericstationbutton.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml"
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
            this.GridBtnStation = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.StationColorCode = ((System.Windows.Controls.Border)(target));
            return;
            case 3:
            this.BtnGenericStation = ((System.Windows.Controls.Grid)(target));
            
            #line 48 "..\..\..\..\Views\PurchaseTicket\GenericStationButton.xaml"
            this.BtnGenericStation.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.BtnGenericStation_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.LblStationName = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.LblStationId = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

