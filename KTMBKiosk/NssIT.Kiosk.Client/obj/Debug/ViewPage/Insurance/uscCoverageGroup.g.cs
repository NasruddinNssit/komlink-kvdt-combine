﻿#pragma checksum "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "D7E7EB1E011DF1110E503A9FB6E9D652BEE48E7141C1C9220EFE7FCB7160742F"
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
    /// uscCoverageGroup
    /// </summary>
    public partial class uscCoverageGroup : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border BdBase;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ImgSelected;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtCoverageDesc;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtCoveragePrice;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.WrapPanel WpnCoverageGroup;
        
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
            System.Uri resourceLocater = new System.Uri("/NssIT.Kiosk.Client;component/viewpage/insurance/usccoveragegroup.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
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
            
            #line 10 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Insurance.uscCoverageGroup)(target)).Loaded += new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\ViewPage\Insurance\uscCoverageGroup.xaml"
            ((NssIT.Kiosk.Client.ViewPage.Insurance.uscCoverageGroup)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.UserControl_MouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 2:
            this.BdBase = ((System.Windows.Controls.Border)(target));
            return;
            case 3:
            this.ImgSelected = ((System.Windows.Controls.Image)(target));
            return;
            case 4:
            this.TxtCoverageDesc = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.TxtCoveragePrice = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.WpnCoverageGroup = ((System.Windows.Controls.WrapPanel)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

