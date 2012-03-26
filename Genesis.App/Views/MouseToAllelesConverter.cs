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
            var gene = parameter as Gene;

            if (mouse == null || gene == null)
                return null;

            var alleles = mouse.Alleles.ToList().Where(a => a.Allele.Gene == gene).OrderBy(a => a.Allele.Species.Name + a.Allele.Value).Select(a => a.Allele.Value).Aggregate((a,b) => a + '/' + b);
            return alleles;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
