﻿#pragma checksum "..\..\..\Views\DateOption.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "3BF6114996287B4C541D984D63B2A5AD8A90EBB7FB1B6D8317182C2481204A88"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Komlink.Views;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace Komlink.Views {
    
    
    /// <summary>
    /// DateOption
    /// </summary>
    public partial class DateOption : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 48 "..\..\..\Views\DateOption.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_Today;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\Views\DateOption.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_Past1Week;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\Views\DateOption.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_Past30Day;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\Views\DateOption.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_DateRange;
        
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
            System.Uri resourceLocater = new System.Uri("/Komlink;component/views/dateoption.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\DateOption.xaml"
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
            this.Btn_Today = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\..\Views\DateOption.xaml"
            this.Btn_Today.Click += new System.Windows.RoutedEventHandler(this.Btn_Today_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Btn_Past1Week = ((System.Windows.Controls.Button)(target));
            
            #line 51 "..\..\..\Views\DateOption.xaml"
            this.Btn_Past1Week.Click += new System.Windows.RoutedEventHandler(this.Btn_Past1Week_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Btn_Past30Day = ((System.Windows.Controls.Button)(target));
            
            #line 54 "..\..\..\Views\DateOption.xaml"
            this.Btn_Past30Day.Click += new System.Windows.RoutedEventHandler(this.Btn_Past30Day_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Btn_DateRange = ((System.Windows.Controls.Button)(target));
            
            #line 57 "..\..\..\Views\DateOption.xaml"
            this.Btn_DateRange.Click += new System.Windows.RoutedEventHandler(this.Btn_DateRange_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

