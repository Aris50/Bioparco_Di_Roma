using System.Collections.Generic;

namespace Bioparco_Di_Roma.Config
{
    public class ValidationConfig
    {
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public List<string> AllowedValues { get; set; }
    }

    public class ColumnConfig
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsEditable { get; set; } = true;
        public bool IsRequired { get; set; }
        public ValidationConfig Validation { get; set; }
    }
} 