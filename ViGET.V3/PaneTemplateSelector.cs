using DothanTech.ViGET.Shell.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DothanTech.ViGET.Shell
{
    public class PaneTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DocTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DocumentViewModel)
                return this.DocTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
