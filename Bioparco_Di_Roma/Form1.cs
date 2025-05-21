using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Bioparco_Di_Roma.Config;

namespace Bioparco_Di_Roma
{
    public partial class Form1 : Form
    {
        private SqlConnection connection;
        private DataSet dataSet;
        private Dictionary<string, SqlDataAdapter> adapters;
        private Dictionary<string, DataRelation> relations;
        private ScenarioConfig config;
        private TableConfig masterTable;
        private TableConfig detailTable;
        private SqlDataAdapter masterAdapter;
        private SqlDataAdapter detailAdapter;

        public Form1()
        {
            try
            {
                InitializeComponent();
                
                // Load configuration first
                LoadConfiguration();
                
                // Initialize database connection
                InitializeDatabase();
                
                // Setup event handlers
                SetupEventHandlers();
                
                // Apply form configuration last
                ApplyFormConfiguration();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Initialization Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"Configuration file not found at: {configPath}");
                }

                string jsonString = File.ReadAllText(configPath);
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    throw new InvalidDataException("Configuration file is empty");
                }

                // Deserialize the root object that contains the Scenario property
                var rootConfig = JsonSerializer.Deserialize<ScenarioRootConfig>(jsonString);
                
                if (rootConfig?.Scenario == null)
                {
                    throw new InvalidDataException("Scenario configuration is missing");
                }

                // Assign the Scenario configuration to our config field
                config = rootConfig.Scenario;

                if (config.Form == null)
                {
                    throw new InvalidDataException("Form configuration is missing");
                }

                // Cache table configurations for easier access
                masterTable = config.Tables?.Find(t => t.Alias == "Master");
                detailTable = config.Tables?.Find(t => t.Alias == "Detail");

                if (masterTable == null || detailTable == null)
                {
                    throw new InvalidDataException("Configuration must include both Master and Detail tables.");
                }

                // Validate required configuration sections
                if (config.CrudProcedures == null)
                {
                    throw new InvalidDataException("CRUD procedures configuration is missing");
                }

                if (config.CrudProcedures.Master == null || config.CrudProcedures.Detail == null)
                {
                    throw new InvalidDataException("CRUD procedures configuration must include both Master and Detail tables");
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error loading configuration: {ex.Message}\n\n" +
                                    $"Stack Trace: {ex.StackTrace}\n\n" +
                                    $"Source: {ex.Source}";
                
                MessageBox.Show(errorMessage, "Configuration Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Log the error to a file
                try
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                    File.AppendAllText(logPath, $"\n\n[{DateTime.Now}] {errorMessage}");
                }
                catch
                {
                    // Ignore logging errors
                }

                Application.Exit();
            }
        }

        private void InitializeDatabase()
        {
            connection = new SqlConnection();
            dataSet = new DataSet();
            adapters = new Dictionary<string, SqlDataAdapter>();
            relations = new Dictionary<string, DataRelation>();
        }

        private void SetupEventHandlers()
        {
            btnConnect.Click += BtnConnect_Click;
            parentDataGridView.SelectionChanged += ParentDataGridView_SelectionChanged;
            parentDataGridView.CellValueChanged += DataGridView_CellValueChanged;
            parentDataGridView.UserDeletingRow += DataGridView_UserDeletingRow;
            parentDataGridView.UserAddedRow += DataGridView_UserAddedRow;
            childDataGridView.CellValueChanged += DataGridView_CellValueChanged;
            childDataGridView.UserDeletingRow += DataGridView_UserDeletingRow;
            childDataGridView.UserAddedRow += DataGridView_UserAddedRow;
        }

        private void ApplyFormConfiguration()
        {
            try
            {
                if (config?.Form == null)
                {
                    throw new InvalidOperationException("Form configuration is not available");
                }

                // Apply form properties from configuration with default values
                this.Text = config.Form.Caption ?? "Bioparco Di Roma";
                this.Width = config.Form.Width > 0 ? config.Form.Width : 800;
                this.Height = config.Form.Height > 0 ? config.Form.Height : 600;

                // Update labels using table names from configuration
                if (masterTable != null)
                {
                    lblParent.Text = $"{masterTable.Name}:";
                }
                else
                {
                    lblParent.Text = "Parent Table:";
                }

                if (detailTable != null)
                {
                    lblChild.Text = $"{detailTable.Name}:";
                }
                else
                {
                    lblChild.Text = "Child Table:";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying form configuration: {ex.Message}", "Configuration Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Apply default values
                this.Text = "Bioparco Di Roma";
                this.Width = 800;
                this.Height = 600;
                lblParent.Text = "Parent Table:";
                lblChild.Text = "Child Table:";
            }
        }

        private void InitializeDataAdapters()
        {
            try
            {
                // Clear existing adapters
                adapters.Clear();

                // Create adapters for each table
                foreach (var table in config.Tables)
                {
                    var adapter = new SqlDataAdapter(table.Query, connection);
                    ConfigureAdapterCommands(adapter, table);
                    adapters[table.Alias] = adapter;
                }

                // Separate master and detail adapters
                masterAdapter = adapters["Master"];
                detailAdapter = adapters["Detail"];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing data adapters: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void ConfigureAdapterCommands(SqlDataAdapter adapter, TableConfig table)
        {
            var crudConfig = GetCrudConfigForTable(table);
            if (crudConfig == null) return;

            // Configure Insert Command
            if (!string.IsNullOrEmpty(crudConfig.Insert))
            {
                var insertCmd = new SqlCommand(crudConfig.Insert, connection);
                AddParametersToCommand(insertCmd, table, false);
                adapter.InsertCommand = insertCmd;

                // Set up the mapping for the output parameter
                var keyColumn = table.Columns.Find(c => c.IsPrimaryKey);
                if (keyColumn != null)
                {
                    var param = adapter.InsertCommand.Parameters[$"@{keyColumn.Name}"];
                    param.Direction = ParameterDirection.Output;
                    param.SourceColumn = keyColumn.Name;
                    param.SourceVersion = DataRowVersion.Current;
                }
            }

            // Configure Update Command
            if (!string.IsNullOrEmpty(crudConfig.Update))
            {
                var updateCmd = new SqlCommand(crudConfig.Update, connection);
                AddParametersToCommand(updateCmd, table, true);
                adapter.UpdateCommand = updateCmd;

                // Ensure primary key parameter is properly mapped for updates
                var keyColumn = table.Columns.Find(c => c.IsPrimaryKey);
                if (keyColumn != null)
                {
                    var param = adapter.UpdateCommand.Parameters[$"@{keyColumn.Name}"];
                    param.SourceColumn = keyColumn.Name;
                    param.SourceVersion = DataRowVersion.Original;
                }
            }

            // Configure Delete Command
            if (!string.IsNullOrEmpty(crudConfig.Delete))
            {
                var deleteCmd = new SqlCommand(crudConfig.Delete, connection);
                AddParametersToCommand(deleteCmd, table, true);
                adapter.DeleteCommand = deleteCmd;

                // Ensure primary key parameter is properly mapped for deletes
                var keyColumn = table.Columns.Find(c => c.IsPrimaryKey);
                if (keyColumn != null)
                {
                    var param = adapter.DeleteCommand.Parameters[$"@{keyColumn.Name}"];
                    param.SourceColumn = keyColumn.Name;
                    param.SourceVersion = DataRowVersion.Original;
                }
            }
        }

        private void AddParametersToCommand(SqlCommand command, TableConfig table, bool isUpdate)
        {
            foreach (var column in table.Columns)
            {
                // Skip non-editable columns for updates, but always include primary keys and hid for Animal table
                if (!column.IsEditable && !column.IsPrimaryKey && !(table.Alias == "Detail" && column.Name == "hid") && isUpdate) continue;

                var parameter = new SqlParameter
                {
                    ParameterName = $"@{column.Name}",
                    SqlDbType = GetSqlDbType(column.Type),
                    SourceColumn = column.Name
                };

                if (column.IsPrimaryKey)
                {
                    if (isUpdate)
                    {
                        // For update/delete, use original value and set as Input
                        parameter.SourceVersion = DataRowVersion.Original;
                        parameter.Direction = ParameterDirection.Input;
                    }
                    else
                    {
                        // For insert, set as Output if you want to retrieve the generated key
                        parameter.Direction = ParameterDirection.Output;
                    }
                }

                command.Parameters.Add(parameter);
            }
        }

        private bool IsForeignKey(string columnName, TableConfig table)
        {
            // Check if this column is a foreign key in any relationship
            if (config.Relationships == null) return false;

            return config.Relationships.Any(r => 
                (r.ParentAlias == table.Alias && r.ParentKey == columnName) ||
                (r.ChildAlias == table.Alias && r.ChildForeignKey == columnName)
            );
        }

        private SqlDbType GetSqlDbType(string typeName)
        {
            return typeName.ToLower() switch
            {
                "int" => SqlDbType.Int,
                "bigint" => SqlDbType.BigInt,
                "float" => SqlDbType.Float,
                "decimal" => SqlDbType.Decimal,
                "datetime" => SqlDbType.DateTime,
                "bit" => SqlDbType.Bit,
                "nvarchar" => SqlDbType.NVarChar,
                "varchar" => SqlDbType.VarChar,
                "char" => SqlDbType.Char,
                "nchar" => SqlDbType.NChar,
                "text" => SqlDbType.Text,
                "ntext" => SqlDbType.NText,
                _ => SqlDbType.NVarChar
            };
        }

        private TableCrudConfig GetCrudConfigForTable(TableConfig table)
        {
            if (table.Alias == "Master")
                return (TableCrudConfig)config.CrudProcedures.Master;
            if (table.Alias == "Detail")
                return (TableCrudConfig)config.CrudProcedures.Detail;
            return null;
        }

        private void LoadData()
        {
            try
            {
                // Clear existing data and relationships
                dataSet.Clear();
                dataSet.Relations.Clear();

                // Load master data
                if (masterAdapter != null)
                {
                    masterAdapter.Fill(dataSet, masterTable.Name);
                }

                // Load detail data
                if (detailAdapter != null)
                {
                    detailAdapter.Fill(dataSet, detailTable.Name);
                }

                // Set primary keys for both tables
                if (dataSet.Tables.Contains(masterTable.Name))
                {
                    var masterTableObj = dataSet.Tables[masterTable.Name];
                    var masterKeyColumn = masterTableObj.Columns[masterTable.KeyColumn];
                    masterTableObj.PrimaryKey = new DataColumn[] { masterKeyColumn };
                }

                if (dataSet.Tables.Contains(detailTable.Name))
                {
                    var detailTableObj = dataSet.Tables[detailTable.Name];
                    var detailKeyColumn = detailTableObj.Columns[detailTable.KeyColumn];
                    detailTableObj.PrimaryKey = new DataColumn[] { detailKeyColumn };
                }

                // Configure DataGridViews for editing
                ConfigureDataGridView(parentDataGridView, masterTable);
                ConfigureDataGridView(childDataGridView, detailTable);

                // Bind the DataGridViews to the data
                if (dataSet.Tables.Contains(masterTable.Name))
                {
                    parentDataGridView.DataSource = dataSet.Tables[masterTable.Name];
                }

                if (dataSet.Tables.Contains(detailTable.Name))
                {
                    childDataGridView.DataSource = dataSet.Tables[detailTable.Name];
                }

                // Set up the relationship between master and detail tables
                if (config.Relationships != null && config.Relationships.Any())
                {
                    foreach (var relationship in config.Relationships)
                    {
                        // Get the table names from the aliases
                        var masterTableName = config.Tables.Find(t => t.Alias == relationship.ParentAlias)?.Name;
                        var detailTableName = config.Tables.Find(t => t.Alias == relationship.ChildAlias)?.Name;

                        if (masterTableName != null && detailTableName != null)
                        {
                            var masterColumn = dataSet.Tables[masterTableName].Columns[relationship.ParentKey];
                            var detailColumn = dataSet.Tables[detailTableName].Columns[relationship.ChildForeignKey];

                            var relation = new DataRelation(
                                relationship.Name,
                                masterColumn,
                                detailColumn,
                                true
                            );

                            dataSet.Relations.Add(relation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridView(DataGridView grid, TableConfig table)
        {
            grid.AllowUserToAddRows = true;
            grid.AllowUserToDeleteRows = true;
            grid.AllowUserToOrderColumns = true;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;
            grid.AutoGenerateColumns = false;

            // Clear existing columns
            grid.Columns.Clear();

            // Add columns based on configuration
            foreach (var column in table.Columns)
            {
                var gridColumn = new DataGridViewColumn();
                
                // Set column type based on data type
                switch (column.Type.ToLower())
                {
                    case "int":
                    case "float":
                    case "decimal":
                        gridColumn = new DataGridViewTextBoxColumn();
                        break;
                    case "datetime":
                        gridColumn = new DataGridViewTextBoxColumn();
                        break;
                    case "bool":
                        gridColumn = new DataGridViewCheckBoxColumn();
                        break;
                    default:
                        if (column.Validation?.AllowedValues != null && column.Validation.AllowedValues.Any())
                        {
                            var comboColumn = new DataGridViewComboBoxColumn();
                            comboColumn.Items.AddRange(column.Validation.AllowedValues.ToArray());
                            gridColumn = comboColumn;
                        }
                        else
                        {
                            gridColumn = new DataGridViewTextBoxColumn();
                        }
                        break;
                }

                gridColumn.Name = column.Name;
                gridColumn.HeaderText = column.DisplayName;
                gridColumn.DataPropertyName = column.Name;
                gridColumn.ReadOnly = !column.IsEditable;
                gridColumn.Visible = true;

                grid.Columns.Add(gridColumn);
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    btnConnect.Text = "Connect";
                    lblStatus.Text = "Disconnected";
                    return;
                }

                connection.ConnectionString = txtConnectionString.Text;
                connection.Open();
                btnConnect.Text = "Disconnect";
                lblStatus.Text = "Connected";
                
                // Initialize data adapters
                InitializeDataAdapters();
                
                // Load data
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to database: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParentDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            // Child data (Animals) is automatically filtered through the DataRelation
            // No additional code needed here
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header row

            try
            {
                if (sender is DataGridView grid) // Use pattern matching to ensure 'grid' is in scope
                {
                    var table = grid == parentDataGridView ? masterTable : detailTable;
                    var row = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;

                    if (row == null) return;

                    // Validate the changed value
                    var column = table.Columns[e.ColumnIndex];
                    var value = row[column.Name];

                    if (column.IsRequired && (value == null || value == DBNull.Value))
                    {
                        MessageBox.Show($"{column.DisplayName} is required.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        row.Row.CancelEdit();
                        return;
                    }

                    if (column.Validation != null)
                    {
                        if (column.Validation.MinValue.HasValue && value != null && value != DBNull.Value)
                        {
                            if (Convert.ToDouble(value) < column.Validation.MinValue.Value)
                            {
                                MessageBox.Show($"{column.DisplayName} must be greater than or equal to {column.Validation.MinValue.Value}.",
                                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                row.Row.CancelEdit();
                                return;
                            }
                        }

                        if (column.Validation.MaxValue.HasValue && value != null && value != DBNull.Value)
                        {
                            if (Convert.ToDouble(value) > column.Validation.MaxValue.Value)
                            {
                                MessageBox.Show($"{column.DisplayName} must be less than or equal to {column.Validation.MaxValue.Value}.",
                                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                row.Row.CancelEdit();
                                return;
                            }
                        }

                        if (column.Validation.AllowedValues != null && column.Validation.AllowedValues.Any())
                        {
                            if (!column.Validation.AllowedValues.Contains(value?.ToString()))
                            {
                                MessageBox.Show($"{column.DisplayName} must be one of: {string.Join(", ", column.Validation.AllowedValues)}",
                                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                row.Row.CancelEdit();
                            return;
                            }
                        }
                    }

                    // End the edit and save changes
                    row.Row.EndEdit();
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating value: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (sender is DataGridView grid)
                {
                    var row = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
                    if (row != null)
                    {
                        row.Row.CancelEdit();
                    }
                }
            }
        }

        private void DataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                var grid = sender as DataGridView;
                var table = grid == parentDataGridView ? masterTable : detailTable;
                var row = e.Row.DataBoundItem as DataRowView;

                if (row == null) return;

                // For habitats, check if there are any animals
                if (grid == parentDataGridView)
                {
                    var habitatId = Convert.ToInt32(row[masterTable.KeyColumn]);
                    var animals = dataSet.Tables[detailTable.Name].Select($"{masterTable.KeyColumn} = {habitatId}");
                    
                    if (animals.Length > 0)
                    {
                        if (MessageBox.Show("This habitat contains animals. Deleting it will also delete all animals in it. Continue?", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                // Delete the row and save changes
                row.Row.Delete();
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting row: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        private void DataGridView_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            try
            {
                var grid = sender as DataGridView;
                var table = grid == parentDataGridView ? masterTable : detailTable;
                var row = e.Row.DataBoundItem as DataRowView;

                if (row == null) return;

                // For animals, set the habitat ID if a habitat is selected
                if (grid == childDataGridView && parentDataGridView.CurrentRow != null)
                {
                    var habitatId = Convert.ToInt32(((DataRowView)parentDataGridView.CurrentRow.DataBoundItem)[masterTable.KeyColumn]);
                    row[masterTable.KeyColumn] = habitatId;
                }

                // End the edit and save changes
                row.Row.EndEdit();
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding row: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                var row = e.Row.DataBoundItem as DataRowView;
                if (row != null)
                {
                    row.Row.CancelEdit();
                }
            }
        }

        private void SaveChanges()
        {
            try
            {
                // Update master table
                if (masterAdapter != null)
                {
                    masterAdapter.Update(dataSet, masterTable.Name);
                }

                // Update detail table
                if (detailAdapter != null)
                {
                    detailAdapter.Update(dataSet, detailTable.Name);
                }

                // Only reload if there were actual changes
                if (dataSet.HasChanges())
                {
                LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData(); // Reload only on error to revert changes
            }
        }
    }
}
