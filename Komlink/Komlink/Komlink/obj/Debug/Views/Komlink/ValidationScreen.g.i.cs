﻿#pragma checksum "..\..\..\..\Views\Komlink\ValidationScreen.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "449069381F1BCB6EBC476BD6A1191456B378E7C0FFEA280D1395882508183F2A"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Komlink.Views.Komlink;
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


namespace Komlink.Views.Komlink {
    
    
    /// <summary>
    /// ValidationScreen
    /// </summary>
    public partial class ValidationScreen : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 58 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock KomlinkCardText;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock NameText;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock NameOnCard;
        
        #line default
        #line hidden
        
        
        #line 83 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CardTypeText;
        
        #line default
        #line hidden
        
        
        #line 85 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CardType;
        
        #line default
        #line hidden
        
        
        #line 89 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock PleaseText;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ScanText;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ExitButtonGrid;
        
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
            System.Uri resourceLocater = new System.Uri("/Komlink;component/views/komlink/validationscreen.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
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
            this.KomlinkCardText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.NameText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.NameOnCard = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.CardTypeText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.CardType = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.PleaseText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.ScanText = ((System.Windows.Controls.Button)(target));
            
            #line 91 "..\..\..\..\Views\Komlink\ValidationScreen.xaml"
            this.ScanText.Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.ExitButtonGrid = ((System.Windows.Controls.Grid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
