using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Base
{
    public class LineShape : IShape
    {
        Point p1 = new Point(0, 0);
        Point p2 = new Point(0, 0);
        public bool Dashed = false;
        public event EventHandler<Tuple<Point, Point>> PointValueChanged;
        public Point Point1
        {
            get
            {
                return p1;
            }
            set
            {
                p1 = value;
                RefreshFormula();
                PointValueChanged(this,Tuple.Create(p1, p2));
            }
        }
        public Point Point2
        {
            get
            {
                return p2;
            }
            set
            {
                p2 = value;
                RefreshFormula();
                PointValueChanged(this, Tuple.Create(p1, p2));
            }
        }
        class FormulaContext
        {           
            public enum TYPE
            {
                None,
                Horizontal,
                Vertical,
                WithSlope
            }
            public TYPE type = TYPE.None;
            public double A;
            public double B;
            LineShape Owner;
            public FormulaContext(LineShape owner)
            {
                this.Owner = owner;
            }
            public void Evaluate()
            {
                if (type != TYPE.None) return;
                if (Owner.p1.X == Owner.p2.X)
                {
                    type = TYPE.Vertical;
                }
                else if(Owner.p1.Y == Owner.p2.Y)
                {
                    type = TYPE.Horizontal;
                }
                else
                {
                    type = TYPE.WithSlope;
                    // evaluate y=ax+b
                    A = ((double)(Owner.p2.Y - Owner.p1.Y)) / (Owner.p2.X - Owner.p1.X);
                    B = Owner.p1.Y - (Owner.p1.X * A);
                }
            }
        }
        FormulaContext formulaContext;
        private void RefreshFormula()
        {
            if (formulaContext != null)
            {
                formulaContext.type = FormulaContext.TYPE.None;
            }
        }
        public LineShape(Point p1,Point p2, int strokeWidth)
        {
            Init(p1.X, p1.Y, p2.X, p2.Y, strokeWidth);
        }
        private void Init(int x1,int y1,int x2,int y2,int strokeWidth)
        {
            this.p1.X = x1;
            this.p1.Y = y2;
            this.p2.X = x2;
            this.p2.Y = y2;
            this.StrokeWidth = strokeWidth;
        }
        public int StrokeWidth = 1;
        public Color ForeColor=Color.Black;
        public void Dispose()
        {
            this.StrokeWidth = 0;
        }

        public bool IsHit(int x, int y)
        {
            // simple way
            formulaContext.Evaluate();
            switch (formulaContext.type)
            {
                case FormulaContext.TYPE.None: return false;
                case FormulaContext.TYPE.Vertical:
                    if ( (x - p1.X)<=StrokeWidth/2.0 && y >= p1.Y && y <= p2.Y) return true;
                    break;
                case FormulaContext.TYPE.Horizontal:
                    if ((y - p1.Y) <= StrokeWidth / 2.0 && x == p1.X && x <= p2.X) return true;
                    break;
                case FormulaContext.TYPE.WithSlope:
                    return Math.Abs(formulaContext.A * x + formulaContext.B - y) < StrokeWidth/2+1e-6;
            }
            return false;
        }

        public void Paint(IRenderer gc)
        {
            if (StrokeWidth <= 0) return;
            gc.drawLine(p1.X, p1.Y, p2.X, p2.Y, ForeColor.ToArgb(), StrokeWidth);
        }
    }
}
