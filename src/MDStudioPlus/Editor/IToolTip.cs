using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Editor
{
	public interface ITooltip
	{
		/// <summary> Should the tooltip close when the mouse moves away? </summary>
		bool CloseWhenMouseMovesAway { get; }
	}
}
