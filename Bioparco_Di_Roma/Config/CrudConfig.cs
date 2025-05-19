namespace Bioparco_Di_Roma.Config
{
    public class TableCrudConfig
    {
        public string Select { get; set; }
        public string Insert { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
    }

    public class CrudConfig
    {
        public TableCrudConfig Master { get; set; }
        public TableCrudConfig Detail { get; set; }
    }
} 