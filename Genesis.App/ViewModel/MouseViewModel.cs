using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Genesis.ViewModel
{
    public class MouseViewModel : DynamicObject
    {
        private Mouse mouse;

        public MouseViewModel(Mouse mouse)
        {
            this.mouse = mouse;
        }
    }
}
