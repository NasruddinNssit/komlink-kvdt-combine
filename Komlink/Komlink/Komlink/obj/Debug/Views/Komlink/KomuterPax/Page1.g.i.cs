﻿#pragma checksum "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "AD4479CD147EB2DEB5470382A31039C64C4098E63241AA9BB04DDB5620A120D8"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Komlink.Views.Komlink.KomuterPax;
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


namespace Komlink.Views.Komlink.KomuterPax {
    
    
    /// <summary>
    /// Page1
    /// </summary>
    public partial class Page1 : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 80 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtJourneyDesc;
        
        #line default
        #line hidden
        
        
        #line 81 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtAvailableDuration;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtMaxPax;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer SvTicketTypePax;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel StkTicketTypeContainer;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtTotalAmount;
        
        #line default
        #line hidden
        
        
        #line 104 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtCurrency;
        
        #line default
        #line hidden
        
        
        #line 106 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtTicketQty;
        
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
            System.Uri resourceLocater = new System.Uri("/Komlink;component/views/komlink/komuterpax/page1.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
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
            this.TxtJourneyDesc = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.TxtAvailableDuration = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.TxtMaxPax = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            
            #line 88 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ResetPax_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SvTicketTypePax = ((System.Windows.Controls.ScrollViewer)(target));
            
            #line 93 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
            this.SvTicketTypePax.ScrollChanged += new System.Windows.Controls.ScrollChangedEventHandler(this.SvTicketTypePax_ScrollChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.StkTicketTypeContainer = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 7:
            this.TxtTotalAmount = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.TxtCurrency = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.TxtTicketQty = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 10:
            
            #line 111 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Ok_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 114 "..\..\..\..\..\Views\Komlink\KomuterPax\Page1.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

