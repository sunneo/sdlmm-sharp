using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Base
{
    public interface IShape: IDisposable
    {
        bool IsHit(int x,int y);
        void Paint(IRenderer gc);
    }
}
