using Caliburn.Micro;
using Microsoft.Maps.MapControl.WPF;

namespace Genesis.ViewModels
{
    public class PushpinViewModel : PropertyChangedBase
    {
        private readonly Locality locality;
        private string label;

        public PushpinViewModel(Locality locality, string label)
        {
            this.locality = locality;
            this.label = label;
        }

        public Location Location
        {
            get
            {
                if (locality.Location == null)
                    return null;

                return new Location(locality.Location.Latitude.Value, locality.Location.Longitude.Value);
            }
        }

        public string Label
        {
            get
            {
                return label;
            }
            set
            {
                this.Set(() => Label, ref label, value);
            }
        }
    }
}