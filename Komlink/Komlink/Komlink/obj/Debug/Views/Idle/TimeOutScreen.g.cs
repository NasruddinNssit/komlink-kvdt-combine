﻿#pragma checksum "..\..\..\..\Views\Idle\TimeOutScreen.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "CCBDF082CFB8D200D7E5D6532124A7ED720D8194A9C11E55FE33B2821358426B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Komlink.Views.Idle;
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


namespace Komlink.Views.Idle {
    
    
    /// <summary>
    /// TimeOutScreen
    /// </summary>
    public partial class TimeOutScreen : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 50 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtTimeoutMessage;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TxtCountDown;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border BdExit;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock ExitText;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border BdContinue;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock ContinueText;
        
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
            System.Uri resourceLocater = new System.Uri("/Komlink;component/views/idle/timeoutscreen.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
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
            this.TxtTimeoutMessage = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.TxtCountDown = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.BdExit = ((System.Windows.Controls.Border)(target));
            
            #line 57 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
            this.BdExit.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.BdExit_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ExitText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.BdContinue = ((System.Windows.Controls.Border)(target));
            
            #line 62 "..\..\..\..\Views\Idle\TimeOutScreen.xaml"
            this.BdContinue.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.BdContinue_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ContinueText = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

