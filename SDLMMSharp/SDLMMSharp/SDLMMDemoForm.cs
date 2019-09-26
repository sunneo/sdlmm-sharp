using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMSharp
{
    internal partial class SDLMMDemoForm : Form
    {
        public SDLMMDemoForm()
        {
            InitializeComponent();
            progressSlider1.SetValueRange(0.0, 100.0, 100);
        }
    }
}
