﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class LocalitiesImportColumnProvider : IImportColumnProvider<Locality>
    {
        public IEnumerable<ICellReader<Locality>> GetColumns()
        {
            return new List<ICellReader<Locality>> { new CodeColumn(), new LatitudeColumn(), new LongitudeColumn(), new LocalityNameColumn() };
        }
    }
}
