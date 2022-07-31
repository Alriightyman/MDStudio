using AvalonDock.Controls;
using AvalonDock.Layout;
using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus
{
    class LayoutInitializer : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            //AD wants to add the anchorable into destinationContainer
            //just for test provide a new anchorablepane 
            //if the pane is floating let the manager go ahead
            LayoutAnchorablePane destPane = destinationContainer as LayoutAnchorablePane;
            if (destinationContainer != null && destinationContainer.FindParent<LayoutFloatingWindow>() != null)
                return false;

            anchorableToShow.AutoHideWidth = 256;
            anchorableToShow.AutoHideHeight = 128;
            anchorableToShow.CanShowOnHover = false;            

            if (anchorableToShow.Content is ExplorerViewModel)
            {
                var explorerPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ExplorerPane");

                if (explorerPane != null)
                {
                    explorerPane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            if (anchorableToShow.Content is OutputViewModel || anchorableToShow.Content is ErrorViewModel | anchorableToShow.Content is RegistersViewModel )
            {
                var OutputPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "OutputPane");
                if (OutputPane != null)
                {
                    if (anchorableToShow.Content is OutputViewModel outputViewModel)
                    {
                        // we want this first
                        outputViewModel.IsSelected = true;
                        OutputPane.Children.Insert(0,anchorableToShow);
                    }
                    else
                    {
                        ((PaneViewModel)anchorableToShow.Content).IsSelected = false;
                        OutputPane.Children.Add(anchorableToShow);
                    }
                    return true;
                }
            }

            return false;
        }

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {

        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }
    }

}
