using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

namespace MDStudioPlus.ViewModels
{
    /// <summary>
    /// Class implements a viewmodel to support the
    /// <seealso cref="AvalonDockLayoutSerializer"/>
    /// attached behavior which is used to implement
    /// load/save of layout information on application
    /// start and shut-down.
    /// </summary>
    public class LayoutViewModel
    {
        #region fields
        private RelayCommand mLoadLayoutCommand = null;
        private RelayCommand mSaveLayoutCommand = null;
        private DockingManager dockingManager;
        #endregion fields

        #region command properties
        /// <summary>
        /// Implement a command to load the layout of an AvalonDock-DockingManager instance.
        /// This layout defines the position and shape of each document and tool window
        /// displayed in the application.
        /// 
        /// Parameter:
        /// The command expects a reference to a <seealso cref="DockingManager"/> instance to
        /// work correctly. Not supplying that reference results in not loading a layout (silent return).
        /// </summary>
        public ICommand LoadLayoutCommand
        {
            get
            {
                if (this.mLoadLayoutCommand == null)
                {
                    this.mLoadLayoutCommand = new RelayCommand((p) =>
                    {
                        DockingManager docManager = p as DockingManager;

                        if (docManager == null)
                            return;

                        if(dockingManager == null)
                            dockingManager = docManager;

                        this.LoadDockingManagerLayout(docManager);
                    });
                }

                return this.mLoadLayoutCommand;
            }
        }

        string defaultLayout = @"<?xml version=""1.0"" encoding=""utf-16""?><LayoutRoot><RootPanel Orientation=""Vertical""><LayoutPanel Orientation=""Horizontal"" DockHeight=""1.4667134351447124*""><LayoutPanel Orientation=""Horizontal"" DockWidth=""0.9188405797101449*""><LayoutDocumentPaneGroup Orientation=""Horizontal""><LayoutDocumentPane /></LayoutDocumentPaneGroup></LayoutPanel><LayoutAnchorablePane Id=""14654b12-32ad-4baa-aba5-b4578276b7e3"" DockWidth=""312""><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Solution Explorer"" IsSelected=""True"" ContentId=""Explorer"" LastActivationTimeStamp=""09/01/2022 16:55:45"" CanShowOnHover=""False"" /></LayoutAnchorablePane></LayoutPanel><LayoutAnchorablePaneGroup Orientation=""Horizontal"" DockHeight=""222.5"" FloatingWidth=""322"" FloatingHeight=""805"" FloatingLeft=""1414"" FloatingTop=""1064""><LayoutAnchorablePane DockHeight=""222.5"" FloatingWidth=""322"" FloatingHeight=""805"" FloatingLeft=""1414"" FloatingTop=""1064""><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Output"" IsSelected=""True"" ContentId=""OutputTool"" FloatingLeft=""1414"" FloatingTop=""1064"" FloatingWidth=""322"" FloatingHeight=""805"" LastActivationTimeStamp=""09/01/2022 11:55:32"" CanShowOnHover=""False"" /><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Error List"" ContentId=""ErrorTool"" FloatingLeft=""1323"" FloatingTop=""964"" FloatingWidth=""322"" FloatingHeight=""619"" LastActivationTimeStamp=""09/01/2022 11:55:32"" CanShowOnHover=""False"" /></LayoutAnchorablePane></LayoutAnchorablePaneGroup></RootPanel><TopSide /><RightSide /><LeftSide /><BottomSide /><FloatingWindows><LayoutAnchorableFloatingWindow><LayoutAnchorablePaneGroup Orientation=""Horizontal"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1234"" FloatingTop=""461""><LayoutAnchorablePane Id=""eafd97d8-afc3-4124-87b5-28110ff3bbad"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1234"" FloatingTop=""461"" /></LayoutAnchorablePaneGroup></LayoutAnchorableFloatingWindow><LayoutAnchorableFloatingWindow><LayoutAnchorablePaneGroup Orientation=""Horizontal"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1267"" FloatingTop=""594""><LayoutAnchorablePane Id=""93962409-ab08-4506-9dd2-7c41649dd2cb"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1267"" FloatingTop=""594"" /></LayoutAnchorablePaneGroup></LayoutAnchorableFloatingWindow><LayoutAnchorableFloatingWindow><LayoutAnchorablePaneGroup Orientation=""Horizontal"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1021"" FloatingTop=""457""><LayoutAnchorablePane Id=""edd87ad0-4c16-46e7-abbc-9ef2839b4612"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1021"" FloatingTop=""457"" /></LayoutAnchorablePaneGroup></LayoutAnchorableFloatingWindow></FloatingWindows><Hidden><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Breakpoints"" IsSelected=""True"" ContentId=""BreakpointsTool"" FloatingLeft=""1267"" FloatingTop=""594"" FloatingWidth=""322"" FloatingHeight=""619"" LastActivationTimeStamp=""09/01/2022 17:17:57"" CanShowOnHover=""False"" PreviousContainerId=""93962409-ab08-4506-9dd2-7c41649dd2cb"" PreviousContainerIndex=""0"" /><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Registers"" IsSelected=""True"" ContentId=""RegistersTool"" FloatingLeft=""1021"" FloatingTop=""457"" FloatingWidth=""322"" FloatingHeight=""619"" LastActivationTimeStamp=""09/01/2022 17:17:57"" CanShowOnHover=""False"" PreviousContainerId=""edd87ad0-4c16-46e7-abbc-9ef2839b4612"" PreviousContainerIndex=""0"" /><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Memory Viewer"" IsSelected=""True"" ContentId=""MemoryTool"" FloatingLeft=""1234"" FloatingTop=""461"" FloatingWidth=""322"" FloatingHeight=""619"" LastActivationTimeStamp=""09/01/2022 17:17:57"" CanShowOnHover=""False"" PreviousContainerId=""eafd97d8-afc3-4124-87b5-28110ff3bbad"" PreviousContainerIndex=""0"" /></Hidden></LayoutRoot>";
        
        string defaultDebugLayout = @"<?xml version=""1.0"" encoding=""utf-16""?><LayoutRoot><RootPanel Orientation=""Horizontal""><LayoutPanel Orientation=""Vertical"" DockWidth=""0.9300960512273213*""><LayoutPanel Orientation=""Horizontal"" DockHeight=""2.24069742627272*""><LayoutPanel Orientation=""Horizontal"" DockWidth=""0.9188405797101449*""><LayoutDocumentPaneGroup Orientation=""Horizontal""><LayoutDocumentPane /></LayoutDocumentPaneGroup></LayoutPanel><LayoutAnchorablePane Id=""14654b12-32ad-4baa-aba5-b4578276b7e3"" DockWidth=""312"" /></LayoutPanel><LayoutAnchorablePaneGroup Orientation=""Horizontal"" DockHeight=""222.5"" FloatingWidth=""322"" FloatingHeight=""805"" FloatingLeft=""1414"" FloatingTop=""1064""><LayoutAnchorablePane Id=""f9f07684-77c1-4e94-9bb1-f10ea027f49d"" DockHeight=""222.5"" FloatingWidth=""322"" FloatingHeight=""805"" FloatingLeft=""1414"" FloatingTop=""1064"" /></LayoutAnchorablePaneGroup><LayoutAnchorablePaneGroup Orientation=""Horizontal"" DockHeight=""257.5"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1108"" FloatingTop=""1182""><LayoutAnchorablePane Id=""edd87ad0-4c16-46e7-abbc-9ef2839b4612"" FloatingWidth=""322"" FloatingHeight=""509"" FloatingLeft=""1061"" FloatingTop=""919""><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Registers"" IsSelected=""True"" ContentId=""RegistersTool"" FloatingLeft=""1061"" FloatingTop=""919"" FloatingWidth=""322"" FloatingHeight=""509"" LastActivationTimeStamp=""09/01/2022 17:39:05"" CanShowOnHover=""False"" /></LayoutAnchorablePane><LayoutAnchorablePane Id=""93962409-ab08-4506-9dd2-7c41649dd2cb"" DockHeight=""538.5"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""1108"" FloatingTop=""1182""><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Breakpoints"" IsSelected=""True"" ContentId=""BreakpointsTool"" FloatingLeft=""1108"" FloatingTop=""1182"" FloatingWidth=""322"" FloatingHeight=""619"" LastActivationTimeStamp=""09/01/2022 17:39:00"" CanShowOnHover=""False"" /></LayoutAnchorablePane></LayoutAnchorablePaneGroup></LayoutPanel><LayoutAnchorablePaneGroup Orientation=""Horizontal"" DockWidth=""451"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""2167"" FloatingTop=""668""><LayoutAnchorablePane Id=""eafd97d8-afc3-4124-87b5-28110ff3bbad"" DockWidth=""451"" FloatingWidth=""322"" FloatingHeight=""619"" FloatingLeft=""2167"" FloatingTop=""668""><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Memory Viewer"" IsSelected=""True"" ContentId=""MemoryTool"" FloatingLeft=""2167"" FloatingTop=""668"" FloatingWidth=""322"" FloatingHeight=""619"" LastActivationTimeStamp=""09/01/2022 17:39:08"" CanShowOnHover=""False"" /></LayoutAnchorablePane></LayoutAnchorablePaneGroup></RootPanel><TopSide /><RightSide><LayoutAnchorGroup PreviousContainerId=""14654b12-32ad-4baa-aba5-b4578276b7e3""><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Solution Explorer"" ContentId=""Explorer"" LastActivationTimeStamp=""09/01/2022 16:55:45"" CanShowOnHover=""False"" /></LayoutAnchorGroup></RightSide><LeftSide /><BottomSide><LayoutAnchorGroup PreviousContainerId=""f9f07684-77c1-4e94-9bb1-f10ea027f49d""><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Output"" ContentId=""OutputTool"" FloatingLeft=""1414"" FloatingTop=""1064"" FloatingWidth=""322"" FloatingHeight=""805"" LastActivationTimeStamp=""09/01/2022 17:38:44"" CanShowOnHover=""False"" /></LayoutAnchorGroup></BottomSide><FloatingWindows /><Hidden><LayoutAnchorable AutoHideWidth=""256"" AutoHideHeight=""128"" AutoHideMinWidth=""100"" AutoHideMinHeight=""100"" Title=""Error List"" IsSelected=""True"" ContentId=""ErrorTool"" FloatingLeft=""1323"" FloatingTop=""964"" FloatingWidth=""322"" FloatingHeight=""619"" LastActivationTimeStamp=""09/01/2022 17:38:44"" CanShowOnHover=""False"" PreviousContainerId=""f9f07684-77c1-4e94-9bb1-f10ea027f49d"" PreviousContainerIndex=""1"" /></Hidden></LayoutRoot>";
        
        public void LoadDebugLayout()
        {
            SetLayouts(defaultDebugLayout);
        }

        public void RestoreLastLayout()
        {
            SetLayouts(defaultLayout);
        }

        private void SetLayouts(string layoutToLoad)
        {
            var layoutSerializer = new XmlLayoutSerializer(dockingManager);
            StringBuilder currentLayout = new StringBuilder();

            layoutSerializer.LayoutSerializationCallback += (s, args) =>
            {
                // This can happen if the previous session was loading a file
                // but was unable to initialize the view ...
                if (args.Model.ContentId == null)
                {
                    args.Cancel = true;
                    return;
                }

                LayoutViewModel.ReloadContentOnStartUp(args);
            };

            // get current layout
            using (TextWriter writer = new StringWriter(currentLayout))
            {
                layoutSerializer.Serialize(writer);
            }

            XmlDocument xmlDefaultLayout = new XmlDocument();
            xmlDefaultLayout.LoadXml(currentLayout.ToString());
            var docPane = xmlDefaultLayout.SelectSingleNode("//LayoutDocumentPane");


            // get the debug layout
            XmlDocument xmlDebugLayout = new XmlDocument();
            xmlDebugLayout.LoadXml(layoutToLoad);
            var parentNode = xmlDebugLayout.SelectSingleNode("//LayoutDocumentPane").ParentNode;   //GetElementsByTagName("RootPanel")[0].ParentNode.ReplaceChild(docPane, xmlDefaultLayout.GetElementsByTagName("LayoutDocumentPane")[0]);

            XmlNode newNode = xmlDebugLayout.CreateNode(docPane.NodeType, docPane.Name, docPane.NamespaceURI);
            // Can't do this since its from a different document, sigh.            
            parentNode.ReplaceChild(newNode, xmlDebugLayout.SelectSingleNode("//LayoutDocumentPane"));

            try
            {
                foreach (XmlNode node in docPane.ChildNodes)
                {
                    var newChild = xmlDebugLayout.CreateNode(node.NodeType, node.Name, node.NamespaceURI);

                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        XmlAttribute newAttribute = xmlDebugLayout.CreateAttribute(attribute.Name);
                        newAttribute.Value = attribute.Value;

                        newChild.Attributes.Append(newAttribute);
                    }

                    newNode.AppendChild(newChild);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //
            using (TextReader treader = new StringReader(xmlDebugLayout.OuterXml))
            using (XmlReader reader = XmlReader.Create(treader))
            {
                Console.WriteLine(xmlDebugLayout.OuterXml);
                layoutSerializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Implements a command to save the layout of an AvalonDock-DockingManager instance.
        /// This layout defines the position and shape of each document and tool window
        /// displayed in the application.
        /// 
        /// Parameter:
        /// The command expects a reference to a <seealso cref="string"/> instance to
        /// work correctly. The string is supposed to contain the XML layout persisted
        /// from the DockingManager instance. Not supplying that reference to the string
        /// results in not saving a layout (silent return).
        /// </summary>
        public ICommand SaveLayoutCommand
        {
            get
            {
                if (this.mSaveLayoutCommand == null)
                {
                    this.mSaveLayoutCommand = new RelayCommand((p) =>
                    {
                        string xmlLayout = p as string;

                        if (xmlLayout == null)
                            return;

                        this.SaveDockingManagerLayout(xmlLayout);
                    });
                }

                return this.mSaveLayoutCommand;
            }
        }
        #endregion command properties

        #region methods
        #region LoadLayout
        /// <summary>
        /// Loads the layout of a particular docking manager instance from persistence
        /// and checks whether a file should really be reloaded (some files may no longer
        /// be available).
        /// </summary>
        /// <param name="docManager"></param>
        private void LoadDockingManagerLayout(DockingManager docManager)
        {
            string layoutFileName = System.IO.Path.Combine(Workspace.Instance.DirAppData, Workspace.Instance.LayoutFileName);

            if (System.IO.File.Exists(layoutFileName) == false)
                return;

            var layoutSerializer = new XmlLayoutSerializer(docManager);
            
            layoutSerializer.LayoutSerializationCallback += (s, args) =>
            {
                // This can happen if the previous session was loading a file
                // but was unable to initialize the view ...
                if (args.Model.ContentId == null)
                {
                    args.Cancel = true;
                    return;
                }

                LayoutViewModel.ReloadContentOnStartUp(args);
            };



            layoutSerializer.Deserialize(layoutFileName);
        }

        private static void ReloadContentOnStartUp(LayoutSerializationCallbackEventArgs args)
        {
            string sId = args.Model.ContentId;

            // Empty Ids are invalid but possible if aaplication is closed with File>New without edits.
            if (string.IsNullOrWhiteSpace(sId) == true)
            {
                args.Cancel = true;
                return;
            }

            switch(args.Model.ContentId)
            {
                case OutputViewModel.ToolContentId:
                    args.Content = Workspace.Instance.Output;
                    break;
                case ErrorViewModel.ToolContentId:
                    args.Content = Workspace.Instance.Errors;
                    break;
                case ExplorerViewModel.ToolContentId:
                    args.Content = Workspace.Instance.Explorer;
                    break;
                case MemoryViewModel.ToolContentId:
                    args.Content = Workspace.Instance.Memory;
                    break;
                case RegistersViewModel.ToolContentId:
                    args.Content = Workspace.Instance.Registers;
                    break;
                case BreakpointsViewModel.ToolContentId:
                    args.Content = Workspace.Instance.Breakpoints;
                    break;
                default:
                    args.Content = LayoutViewModel.ReloadDocument(args.Model.ContentId);

                    if (args.Content == null)
                        args.Cancel = true;
                    break;
            }
        }

        private static object ReloadDocument(string path)
        {
            object ret = null;

            if (!string.IsNullOrWhiteSpace(path))
            {
                switch (path)
                {
                    /***
                              case StartPageViewModel.StartPageContentId: // Re-create start page content
                                if (Workspace.This.GetStartPage(false) == null)
                                {
                                  ret = Workspace.This.GetStartPage(true);
                                }
                                break;
                    ***/
                    default:
                        // Re-create text document
                        ret = Workspace.Instance.Open(path);
                        break;
                }
            }

            return ret;
        }
        #endregion LoadLayout

        #region SaveLayout
        private void SaveDockingManagerLayout(string xmlLayout)
        {
            // Create XML Layout file on close application (for re-load on application re-start)
            if (xmlLayout == null)
                return;

            string fileName = System.IO.Path.Combine(Workspace.Instance.DirAppData, Workspace.Instance.LayoutFileName);

            File.WriteAllText(fileName, xmlLayout);
        }
        #endregion SaveLayout
        #endregion methods
    }
}
