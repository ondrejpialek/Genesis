using System.Linq;

namespace Genesis.Excel
{
    public class NominalTraitCellReader : CellReader<Mouse, string>
    {
        public NominalTraitCellReader() : base("Nominal Trait")
        {
        }

        /// <summary>
        /// The gene this Trait represents.
        /// </summary>
        public NominalTrait Trait { get; set; }

        protected override void Apply(Mouse mouse, string value)
        {
            var records = mouse.Records.Where(r => r.Trait == Trait).ToList();
            foreach (var record in records)
            {
                mouse.Records.Remove(record);
            }

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var newRecords = (from str in value.Split('/')
                let s = str.Trim().ToLowerInvariant()
                join category in Trait.Categories on s equals category.Value.ToLowerInvariant()
                select category).ToList();

            foreach (var category in newRecords)
                mouse.Records.Add(new NominalRecord(category, mouse));
        }
    }
}