﻿#pragma checksum "..\..\..\..\..\ViewPage\Insurance\pgToDisagreeInsurance.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "1A70BB23DFB8FEBE6BDDD5C09D6B52AA0BD1034508B9BB302C76ACF674950C26"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NssIT.Kiosk.Client.ViewPage.Insurance;
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


namespace NssIT.Kiosk.Client.ViewPage.Insurance {
    
    
    /// <summary>
    /// pgToDisagreeInsurance
    /// </summary>
    public partial class pgToDisagreeInsurance : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
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
            System.Uri resourceLocater = new System.Uri("/NssIT.Kiosk.Client;component/viewpage/insurance/pgtodisagreeinsurance.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\ViewPage\Insurance\pgToDisagreeInsurance.xaml"
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
            
            #line 9 "..\..\..\..\..\ViewPage\Insurance\pgToDisagreeInsurance.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Insurance.pgToDisagreeInsurance)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Page_Loaded);
            
            #line default
            #line hidden
            
            #line 9 "..\..\..\..\..\ViewPage\Insurance\pgToDisagreeInsurance.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Insurance.pgToDisagreeInsurance)(target)).Unloaded += new System.Windows.RoutedEventHandler(this.Page_Unloaded);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 48 "..\..\..\..\..\ViewPage\Insurance\pgToDisagreeInsurance.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ConfirmDisagreeInsurance_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 61 "..\..\..\..\..\ViewPage\Insurance\pgToDisagreeInsurance.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.AgreeInsurance_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
