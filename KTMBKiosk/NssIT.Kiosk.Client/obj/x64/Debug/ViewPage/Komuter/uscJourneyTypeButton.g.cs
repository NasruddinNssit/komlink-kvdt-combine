﻿#pragma checksum "..\..\..\..\..\ViewPage\Komuter\uscJourneyTypeButton.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "159942B45D729C3B51CD718D3EC2FB8FC1B87679A9D2DDBE10F9449F16A66772"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NssIT.Kiosk.Client.ViewPage.Komuter;
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


namespace NssIT.Kiosk.Client.ViewPage.Komuter {
    
    
    /// <summary>
    /// uscJourneyTypeButton
    /// </summary>
    public partial class uscJourneyTypeButton : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 27 "..\..\..\..\..\ViewPage\Komuter\uscJourneyTypeButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BdJourneyType;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\..\..\ViewPage\Komuter\uscJourneyTypeButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtTypeDesc;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\..\..\ViewPage\Komuter\uscJourneyTypeButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtAvalableDuration;
        
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
            System.Uri resourceLocater = new System.Uri("/NssIT.Kiosk.Client;component/viewpage/komuter/uscjourneytypebutton.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\ViewPage\Komuter\uscJourneyTypeButton.xaml"
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
            this.BdJourneyType = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\..\..\ViewPage\Komuter\uscJourneyTypeButton.xaml"
            this.BdJourneyType.Click += new System.Windows.RoutedEventHandler(this.BdJourneyType_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.TxtTypeDesc = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.TxtAvalableDuration = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

