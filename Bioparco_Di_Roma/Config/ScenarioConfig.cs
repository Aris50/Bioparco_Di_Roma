using System.Collections.Generic;

namespace Bioparco_Di_Roma.Config
{
    public class ScenarioRootConfig
    {
        public ScenarioConfig Scenario { get; set; }
    }

    public class ScenarioConfig
    {
        public string Name { get; set; }
        public FormConfig Form { get; set; }
        public List<TableConfig> Tables { get; set; }
        public List<RelationshipConfig> Relationships { get; set; }
        public CrudProceduresConfig CrudProcedures { get; set; }
    }

    public class CrudProceduresConfig
    {
        public TableCrudConfig Master { get; set; }
        public TableCrudConfig Detail { get; set; }
    }
}
