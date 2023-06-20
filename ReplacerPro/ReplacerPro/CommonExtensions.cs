using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReplacerPro
{
    internal static class CommonExtensions
    {
        /// <summary>
        /// 将给定的 <paramref name="input"/> 字符串以指定的 <paramref name="spliter"/> 分割
        /// </summary>
        /// <param name="spliter">分隔符</param>
        /// <returns>以分隔符切割形成的string list</returns>
        public static List<string> Split(this string input, string spliter)
        {
            return Regex.Split(input, spliter, RegexOptions.IgnoreCase).ToList();
        }

        public static void TrySetText(this RichEditBox reb, string text)
        {
            try
            {
                bool _ori_readonly = reb.IsReadOnly;
                reb.IsReadOnly = false;
                reb.TextDocument.SetText(Microsoft.UI.Text.TextSetOptions.None, text);
                reb.IsReadOnly = _ori_readonly;
            }
            catch(Exception ex)
            {
                Console.WriteLine("TrySetText fail: " + ex.Message);
            }
        }
    }
}
