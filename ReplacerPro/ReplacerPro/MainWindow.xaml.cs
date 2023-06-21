using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReplacerPro
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Event_MainWindowLoad(object sender, RoutedEventArgs e)
        {
            TrySetMicaBackdrop();
            //App.m_window.ExtendsContentIntoTitleBar = true;  // enable custom titlebar
            //App.m_window.SetTitleBar(AppTitleBar);      // set user ui element as titlebar
        }

        private void Menu_Opening(object sender, object e)
        {
            CommandBarFlyout myFlyout = sender as CommandBarFlyout;
            var SupportedTargets = new List<RichEditBox>() { ReplaceRuleEditBox, TargetTextEditBox };
            if (SupportedTargets.Contains(myFlyout.Target))
            {
                AppBarButton myButton = new AppBarButton();
                myButton.Command = new StandardUICommand(StandardUICommandKind.Share);
                myFlyout.PrimaryCommands.Add(myButton);
            }
        }
        #region ReplaceRuleEditBox Events
        private void Event_ReplaceRuleEditBox_Loaded(object sender, RoutedEventArgs e)
        {
            ReplaceRuleEditBox.SelectionFlyout.Opening += Menu_Opening;
            ReplaceRuleEditBox.ContextFlyout.Opening += Menu_Opening;

        }
        private void Event_ReplaceRuleEditBox_Unloaded(object sender, RoutedEventArgs e)
        {
            ReplaceRuleEditBox.SelectionFlyout.Opening -= Menu_Opening;
            ReplaceRuleEditBox.ContextFlyout.Opening -= Menu_Opening;

        }
        private async void Event_ReplaceRuleEditBox_FillWithClipboard(object sender, RoutedEventArgs e)
        {
            var package = Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.Text))
            {
                var text = await package.GetTextAsync();
                ReplaceRuleEditBox.TextDocument.SetText(Microsoft.UI.Text.TextSetOptions.None, text);
            }
        }
        #endregion

        #region TargetTextEditBox Events
        private void Event_TargetTextEditBox_Loaded(object sender, RoutedEventArgs e)
        {
            TargetTextEditBox.SelectionFlyout.Opening += Menu_Opening;
            TargetTextEditBox.ContextFlyout.Opening += Menu_Opening;

        }
        private void Event_TargetTextEditBox_Unloaded(object sender, RoutedEventArgs e)
        {
            TargetTextEditBox.SelectionFlyout.Opening -= Menu_Opening;
            TargetTextEditBox.ContextFlyout.Opening -= Menu_Opening;

        }
        private async void Event_TargetTextEditBox_FillWithClipboard(object sender, RoutedEventArgs e)
        {
            var package = Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.Text))
            {
                var text = await package.GetTextAsync();
                TargetTextEditBox.TextDocument.SetText(Microsoft.UI.Text.TextSetOptions.None, text);
            }
        }
        #endregion

        private void Event_TbxTextChanged(object sender, RoutedEventArgs e)
        {
            ReplaceRuleEditBox.TextDocument.GetText(Microsoft.UI.Text.TextGetOptions.None, out string rule);
            TargetTextEditBox.TextDocument.GetText(Microsoft.UI.Text.TextGetOptions.None, out string target);

            var rules = rule.Split('\r');
            foreach (var r in rules)
            {
                string _RULE = r?.Trim(new char[] { '\n', '\t', ' ' });
                if (_RULE != null && _RULE.Length > 0 && _RULE.Contains("->"))
                {
                    string spliter = _RULE.Contains(" -> ") ? " -> " : "->";
                    string _ORI = _RULE.Split(spliter)[0];
                    string _TAR = _RULE.Replace(_ORI + spliter, "");
                    _ORI = MyFormString(_ORI);
                    _TAR = MyFormString(_TAR);

                    target = target.Replace(_ORI, _TAR);
                }
            }

            ModifiedTextEditBox.TrySetText(target);

            string MyFormString(string str)
            {
                if (str.StartsWith('!'))
                {
                    str = str[1..];
                    str = str.Replace("@", GlobalVars.Enter);
                }
                return str;
            }
        }
        private void Event_ModifiedTextEditBox_CopyToClipboard(object sender, RoutedEventArgs e)
        {
            ModifiedTextEditBox.TextDocument.GetText(Microsoft.UI.Text.TextGetOptions.None, out string text);
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            //myButton.Content = "Clicked";
        }

        #region Mica Ui Support Code

        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See separate sample below for implementation
        Microsoft.UI.Composition.SystemBackdrops.MicaController m_micaController;
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configurationSource;

        bool TrySetMicaBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Hooking up the policy object
                m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
            }
        } 
        #endregion
    }
}
