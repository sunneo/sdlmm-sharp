using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Controls
{
	public interface IMouseHandler
	{
		bool mouseMoved(bool on, int x, int y);
		bool mouseAction(bool on, int x, int y);
		bool mouseDoubleClick(int x, int y);
	}
}
