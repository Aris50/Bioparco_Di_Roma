using System.Collections.Generic;

namespace Bioparco_Di_Roma.Config
{
    public class TableConfig
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Query { get; set; }
        public string KeyColumn { get; set; }
        public List<ColumnConfig> Columns { get; set; }
    }
} 