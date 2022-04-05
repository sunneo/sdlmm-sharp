using SDLMMSharp.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Controls
{
    public class SpriteObject : DraggableTarget
    {
        public ResizerClazz Resizer;
        public SpriteObject()
        {
            Resizer = new ResizerClazz(this);
        }
        public IDraggableTarget AdditionalHitTest(int x, int y)
        {
            if (this.label != null && (this.label is IDraggableTarget)) 
            {
                if (this.label.IsHit(x, y))
                {
                    return (IDraggableTarget)this.label;
                }
            }
            if(this.Resizer != null)
            {
                if (this.Resizer.WidthAdjuster!=null && this.Resizer.WidthAdjuster.IsHit(x, y))
                {
                    return this.Resizer.WidthAdjuster;
                }
                if (this.Resizer.HeightAdjuster != null && this.Resizer.HeightAdjuster.IsHit(x, y))
                {
                    return this.Resizer.HeightAdjuster;
                }
                if (this.Resizer.BothAdjuster != null && this.Resizer.BothAdjuster.IsHit(x, y))
                {
                    return this.Resizer.BothAdjuster;
                }
            }

            return null;
        }
       
    }
}
