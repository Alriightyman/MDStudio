using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.ViewModels
{
    internal class ToolViewModel : PaneViewModel
    {
        #region fields
        private bool isVisible = true;
        #endregion fields

        #region constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="name"></param>
        public ToolViewModel(string name)
        {
            Name = name;
            Title = name;
        }
        #endregion constructor

        #region Properties
        public string Name { get; private set; }

        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    RaisePropertyChanged(nameof(IsVisible));
                }
            }
        }
        #endregion Properties
    }
}
