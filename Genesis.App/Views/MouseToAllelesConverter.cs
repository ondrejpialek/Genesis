using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Genesis.Views
{
    public class MouseToAllelesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mouse = value as Mouse;
            var trait = parameter as Trait;

            if (mouse == null || trait == null)
                return null;


            var ordinalTrait = trait as OrdinalTrait;
            if (ordinalTrait != null)
            {
                var values = mouse.Records.OfType<OrdinalRecord>().Where(r => r.Trait == ordinalTrait).Select(r => $"{r.Value}");
                return string.Join(",", values);
            }

            var gene = trait as Gene;
            if (gene != null)
            {
                var alleles = mouse.Records.OfType<NominalRecord>().Where(r => r.Category.Trait == gene).ToList().OrderBy(a => ((Allele)a.Category).Species.Name).ThenBy(a => a.Category.Value).Select(a => a.Category.Value).ToArray();
                var str = string.Join("/", alleles);
                return str;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
