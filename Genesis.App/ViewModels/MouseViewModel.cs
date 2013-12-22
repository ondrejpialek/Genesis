using System.Dynamic;

namespace Genesis.ViewModels
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
