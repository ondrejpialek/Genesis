using System;
using System.Windows.Controls;
using GalaSoft.MvvmLight;

namespace Genesis.ViewModels
{
    public class SectionViewModel : ViewModelBase
    {
        private string title;
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                if (title != value)
                {
                    title = value;
                    Set(() => Title, ref title, value);
                }
            }
        }

        private Func<UserControl> createContent;
        private UserControl control = null;
        public UserControl Content
        {
            get
            {
                if (control == null)
                {
                    control = createContent();
                }
                return control;
            }
        }

        public SectionViewModel(string title, Func<UserControl> createContent)
        {
            this.title = title;
            this.createContent = createContent;
        }
    }
}
