﻿#pragma checksum "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "18CA6838FE06F61A75D9B668C7EEE04246146C90D2FCDAFA2F1FEFE68C77E090"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NssIT.Kiosk.Client.ViewPage.Trip;
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


namespace NssIT.Kiosk.Client.ViewPage.Trip {
    
    
    /// <summary>
    /// pgTimeFilter
    /// </summary>
    public partial class pgTimeFilter : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GrdMain;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas CvSliderContainer;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ImgThumb1;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ImgThumb2;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtFromTime;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtToTime;
        
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
            System.Uri resourceLocater = new System.Uri("/NssIT.Kiosk.Client;component/viewpage/trip/pgtimefilter.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
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
            
            #line 11 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Trip.pgTimeFilter)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Page_Loaded);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Trip.pgTimeFilter)(target)).Unloaded += new System.Windows.RoutedEventHandler(this.Page_Unloaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.GrdMain = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.CvSliderContainer = ((System.Windows.Controls.Canvas)(target));
            return;
            case 4:
            this.ImgThumb1 = ((System.Windows.Controls.Image)(target));
            return;
            case 5:
            this.ImgThumb2 = ((System.Windows.Controls.Image)(target));
            return;
            case 6:
            this.TxtFromTime = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.TxtToTime = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            
            #line 75 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Reset_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 77 "..\..\..\..\ViewPage\Trip\pgTimeFilter.xaml"
            ((System.Windows.Controls.Border)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.BtnFilter_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

