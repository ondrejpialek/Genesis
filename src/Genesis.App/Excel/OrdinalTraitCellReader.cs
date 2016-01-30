using System;

namespace Genesis.Excel
{
    public class OrdinalTraitCellReader : CellReader<Mouse, string>
    {
        /// <summary>
        /// Trait to import.
        /// </summary>
        public Trait Trait { get; set; }

        public OrdinalTraitCellReader() : base("Trait")
        {
        }

        protected override void Apply(Mouse entity, string value)
        {
            throw new NotImplementedException();
        }

        //private void ApplyOrdinalGene(Mouse mouse, string value)
        //{
        //    var gene = (OrdinalGene)Gene;

        //    double parsedValue;
        //    var parsed = double.TryParse(value, out parsedValue);

        //    if (!parsed)
        //    {
        //        throw new FormatException($"[{Gene.Name}@{mouse.Id}] '{value}' is not a number.");
        //    }

        //    var record = mouse.Records.OfType<OrdinalRecord>().FirstOrDefault(r => r.Gene == gene);
        //    if (record == null)
        //    {
        //        record = new OrdinalRecord(gene);
        //        mouse.Records.Add(record);
        //    }

        //    record.Value = parsedValue;
        //}

       
    }
}
