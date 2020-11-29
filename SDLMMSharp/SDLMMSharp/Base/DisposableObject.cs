using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Base
{
    public class DisposableObject : IDisposable
    {
        public event EventHandler OnDisposed;
        public void Dispose()
        {
            OnDisposed(this, EventArgs.Empty);
            foreach(var del in OnDisposed.GetInvocationList())
            {
                Delegate.RemoveAll(del, del);
            }
        }

    }
}
