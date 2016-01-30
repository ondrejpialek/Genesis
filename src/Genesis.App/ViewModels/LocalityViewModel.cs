using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Maps.MapControl.WPF;

namespace Genesis.ViewModels
{
    public class LocalityViewModel : PropertyChangedBase
    {
  
        public LocalityViewModel(Locality locality) {
            Locality = locality;
        }

        private Locality locality = null;
        public Locality Locality
        {
            get
            {
                return locality;
            }
            protected set
            {
                this.Set(() => Locality, ref locality, value);
            }
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

        
        public string Name
        {
            get
            {
                return locality.Name;
            }
            set
            {
                locality.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Code
        {
            get
            {
                return locality.Code;
            }
            set
            {
                locality.Code = value;
                NotifyOfPropertyChange(() => Code);
            }
        }

        private double? frequency = null;
        public double? Frequency
        {
            get
            {
                if (frequency == null)
                {

                    Task.Factory.StartNew(() =>
                    {
                        using (GenesisContext c = new GenesisContext())
                        {
                            var locality = c.Localities.Single(l => l.Id == this.locality.Id);
                            var alleles = (from mouse in locality.Mice
                                           from allAlleles in mouse.Records.OfType<NominalRecord>()
                                           where allAlleles.Category.Trait.Name == "Btk"                //why is this here???
                                           group allAlleles by allAlleles.Category.Value).ToArray();

                            double d = alleles.Where(a => a.Key == "d").Select(r => r.Count()).SingleOrDefault();
                            double m = alleles.Where(a => a.Key == "m").Select(r => r.Count()).SingleOrDefault();

                            if (d + m == 0)
                            {
                                return -1;
                            }

                            return m / (d + m);
                        }
                    }).ContinueWith(f => Frequency = f.Result, TaskScheduler.Current);
                }

                return frequency;    
            }

            protected set
            {
                this.Set(() => Frequency, ref frequency, value);
            }
        }


    }
}