using AvalonDock.Layout;
using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MDStudioPlus
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {
        }

        public DataTemplate FileViewTemplate
        {
            get;
            set;
        }

        public DataTemplate PropertiesViewTemplate
        {
            get;
            set;
        }

        public DataTemplate ExplorerViewTemplate
        {
            get;
            set;
        }

        public DataTemplate ErrorViewTemplate
        {
            get;
            set;
        }

        public DataTemplate OutputViewTemplate
        {
            get;
            set;
        }

        public DataTemplate RegistersViewTemplate
        {
            get;
            set;
        }

        public DataTemplate MemoryViewTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is FileViewModel)
                return FileViewTemplate;

            /*if (item is PropertiesViewModel)
                return PropertiesViewTemplate;*/

            if (item is ExplorerViewModel)
                return ExplorerViewTemplate;

            if (item is ErrorViewModel)
                return ErrorViewTemplate;

            if (item is OutputViewModel)
                return OutputViewTemplate;

            if (item is RegistersViewModel)
                return RegistersViewTemplate;

            if (item is MemoryViewModel)
                return MemoryViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
