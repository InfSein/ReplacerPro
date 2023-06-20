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
using static System.Net.Mime.MediaTypeNames;

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
            App.m_window.ExtendsContentIntoTitleBar = true;  // enable custom titlebar
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
    }
}
