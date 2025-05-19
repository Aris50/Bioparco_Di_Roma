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
            btnAddChild.Click += BtnAddChild_Click;
            btnEditChild.Click += BtnEditChild_Click;
            btnDeleteChild.Click += BtnDeleteChild_Click;
            parentDataGridView.SelectionChanged += ParentDataGridView_SelectionChanged;
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
            }

            // Configure Update Command
            if (!string.IsNullOrEmpty(crudConfig.Update))
            {
                var updateCmd = new SqlCommand(crudConfig.Update, connection);
                AddParametersToCommand(updateCmd, table, true);
                adapter.UpdateCommand = updateCmd;
            }

            // Configure Delete Command
            if (!string.IsNullOrEmpty(crudConfig.Delete))
            {
                var deleteCmd = new SqlCommand(crudConfig.Delete, connection);
                AddParametersToCommand(deleteCmd, table, true);
                adapter.DeleteCommand = deleteCmd;
            }
        }

        private void AddParametersToCommand(SqlCommand command, TableConfig table, bool isUpdate)
        {
            foreach (var column in table.Columns)
            {
                if (!column.IsEditable && isUpdate) continue;

                var parameter = new SqlParameter
                {
                    ParameterName = $"@{column.Name}",
                    SqlDbType = GetSqlDbType(column.Type),
                    SourceColumn = column.Name
                };

                if (isUpdate && column.IsPrimaryKey)
                {
                    parameter.SourceVersion = DataRowVersion.Original;
                }

                command.Parameters.Add(parameter);
            }
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
                // Clear existing data
                dataSet.Clear();

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void BtnAddChild_Click(object sender, EventArgs e)
        {
            // Show selection dialog
            using (Form selectionForm = new Form())
            {
                selectionForm.Text = "Select Operation";
                selectionForm.Size = new System.Drawing.Size(300, 150);
                selectionForm.StartPosition = FormStartPosition.CenterParent;

                Label lblSelect = new Label { Text = "Select table to add to:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                ComboBox cmbTable = new ComboBox { Location = new System.Drawing.Point(20, 50), Width = 240 };
                cmbTable.Items.AddRange(new string[] { "Animals", "Habitats" });

                Button btnProceed = new Button { Text = "Proceed", Location = new System.Drawing.Point(100, 80), DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(200, 80), DialogResult = DialogResult.Cancel };

                selectionForm.Controls.AddRange(new Control[] { lblSelect, cmbTable, btnProceed, btnCancel });

                if (selectionForm.ShowDialog() == DialogResult.OK)
                {
                    if (cmbTable.SelectedItem.ToString() == "Animals")
                    {
                        AddAnimal();
                    }
                    else
                    {
                        AddHabitat();
                    }
                }
            }
        }

        private Dictionary<string, object> ShowInputDialog(TableConfig table, DataRow existingRow = null)
        {
            using (Form inputForm = new Form())
            {
                inputForm.Text = existingRow == null ? $"Add New {table.Name}" : $"Edit {table.Name}";
                inputForm.Size = new System.Drawing.Size(400, 600);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var controls = new Dictionary<string, Control>();
                var yPos = 20;
                var labelWidth = 120;
                var controlWidth = 200;
                var spacing = 40;

                // Create controls for each column
                foreach (var column in table.Columns)
                {
                    // Skip if not editable
                    if (!column.IsEditable && existingRow != null)
                        continue;

                    // Create label
                    var label = new Label
                    {
                        Text = $"{column.DisplayName}:",
                        Location = new System.Drawing.Point(20, yPos),
                        Width = labelWidth,
                        AutoSize = true
                    };

                    // Create input control based on column type
                    Control inputControl;
                    if (column.Type.ToLower() == "bool")
                    {
                        var checkBox = new CheckBox
                        {
                            Location = new System.Drawing.Point(150, yPos),
                            Width = controlWidth,
                            Checked = existingRow != null ? Convert.ToBoolean(existingRow[column.Name]) : false
                        };
                        inputControl = checkBox;
                    }
                    else if (column.Validation?.AllowedValues != null && column.Validation.AllowedValues.Any())
                    {
                        var comboBox = new ComboBox
                        {
                            Location = new System.Drawing.Point(150, yPos),
                            Width = controlWidth,
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        comboBox.Items.AddRange(column.Validation.AllowedValues.ToArray());
                        if (existingRow != null)
                            comboBox.SelectedItem = existingRow[column.Name].ToString();
                        else if (comboBox.Items.Count > 0)
                            comboBox.SelectedIndex = 0;
                        inputControl = comboBox;
                    }
                    else
                    {
                        var textBox = new TextBox
                        {
                            Location = new System.Drawing.Point(150, yPos),
                            Width = controlWidth,
                            Text = existingRow != null ? existingRow[column.Name].ToString() : ""
                        };
                        inputControl = textBox;
                    }

                    // Add controls to form
                    inputForm.Controls.Add(label);
                    inputForm.Controls.Add(inputControl);
                    controls[column.Name] = inputControl;

                    yPos += spacing;
                }

                // Add buttons
                var btnSave = new Button
                {
                    Text = "Save",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(150, yPos)
                };

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(250, yPos)
                };

                inputForm.Controls.Add(btnSave);
                inputForm.Controls.Add(btnCancel);

                // Handle validation and data collection
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    var values = new Dictionary<string, object>();
                    string errorMessage;

                    foreach (var column in table.Columns)
                    {
                        if (!column.IsEditable && existingRow != null)
                        {
                            values[column.Name] = existingRow[column.Name];
                            continue;
                        }

                        if (!ValidateField(column, controls[column.Name], out errorMessage))
                        {
                            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }

                        values[column.Name] = GetControlValue(controls[column.Name], column.Type);
                    }

                    return values;
                }

                return null;
            }
        }

        private bool ValidateField(ColumnConfig column, Control control, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Check if required
            if (column.IsRequired)
            {
                if (control is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    errorMessage = $"{column.DisplayName} is required.";
                    return false;
                }
                if (control is ComboBox comboBox && comboBox.SelectedItem == null)
                {
                    errorMessage = $"{column.DisplayName} is required.";
                    return false;
                }
            }

            // Get the value for further validation
            var value = GetControlValue(control, column.Type);
            if (value == null && !column.IsRequired)
                return true;

            // Validate numeric ranges
            if (column.Validation != null)
            {
                if (column.Validation.MinValue.HasValue)
                {
                    if (value is IComparable comparable && comparable.CompareTo(column.Validation.MinValue.Value) < 0)
                    {
                        errorMessage = $"{column.DisplayName} must be greater than or equal to {column.Validation.MinValue.Value}.";
                        return false;
                    }
                }

                if (column.Validation.MaxValue.HasValue)
                {
                    if (value is IComparable comparable && comparable.CompareTo(column.Validation.MaxValue.Value) > 0)
                    {
                        errorMessage = $"{column.DisplayName} must be less than or equal to {column.Validation.MaxValue.Value}.";
                        return false;
                    }
                }

                // Validate allowed values
                if (column.Validation.AllowedValues != null && column.Validation.AllowedValues.Any())
                {
                    if (control is ComboBox comboBox && !column.Validation.AllowedValues.Contains(comboBox.SelectedItem?.ToString()))
                    {
                        errorMessage = $"{column.DisplayName} must be one of the allowed values.";
                        return false;
                    }
                }
            }

            return true;
        }

        private object GetControlValue(Control control, string type)
        {
            if (control is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                    return null;

                return type.ToLower() switch
                {
                    "int" => int.TryParse(textBox.Text, out int intValue) ? intValue : null,
                    "float" => float.TryParse(textBox.Text, out float floatValue) ? floatValue : null,
                    "decimal" => decimal.TryParse(textBox.Text, out decimal decimalValue) ? decimalValue : null,
                    "datetime" => DateTime.TryParse(textBox.Text, out DateTime dateValue) ? dateValue : null,
                    "bool" => bool.TryParse(textBox.Text, out bool boolValue) ? boolValue : null,
                    _ => textBox.Text
                };
            }
            else if (control is ComboBox comboBox)
            {
                return comboBox.SelectedItem;
            }
            else if (control is CheckBox checkBox)
            {
                return checkBox.Checked;
            }

            return null;
        }

        private void AddAnimal()
        {
            if (parentDataGridView.CurrentRow == null)
            {
                MessageBox.Show("Please select a habitat first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Get the selected habitat
                DataRowView habitatRow = (DataRowView)parentDataGridView.CurrentRow.DataBoundItem;
                int habitatId = Convert.ToInt32(habitatRow[masterTable.KeyColumn]);

                var values = ShowInputDialog(detailTable);
                if (values != null)
                {
                    // Add the foreign key value
                    values[detailTable.KeyColumn] = habitatId;

                    // Create new row
                    DataRow newRow = dataSet.Tables[detailTable.Name].NewRow();
                    foreach (var kvp in values)
                    {
                        newRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    dataSet.Tables[detailTable.Name].Rows.Add(newRow);
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddHabitat()
        {
            try
            {
                var values = ShowInputDialog(masterTable);
                if (values != null)
                {
                    DataRow newRow = dataSet.Tables[masterTable.Name].NewRow();
                    foreach (var kvp in values)
                    {
                        newRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    dataSet.Tables[masterTable.Name].Rows.Add(newRow);
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEditChild_Click(object sender, EventArgs e)
        {
            // Show selection dialog
            using (Form selectionForm = new Form())
            {
                selectionForm.Text = "Select Operation";
                selectionForm.Size = new System.Drawing.Size(300, 150);
                selectionForm.StartPosition = FormStartPosition.CenterParent;

                Label lblSelect = new Label { Text = "Select table to edit:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                ComboBox cmbTable = new ComboBox { Location = new System.Drawing.Point(20, 50), Width = 240 };
                cmbTable.Items.AddRange(new string[] { "Animals", "Habitats" });

                Button btnProceed = new Button { Text = "Proceed", Location = new System.Drawing.Point(100, 80), DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(200, 80), DialogResult = DialogResult.Cancel };

                selectionForm.Controls.AddRange(new Control[] { lblSelect, cmbTable, btnProceed, btnCancel });

                if (selectionForm.ShowDialog() == DialogResult.OK)
                {
                    if (cmbTable.SelectedItem.ToString() == "Animals")
                    {
                        EditAnimal();
                    }
                    else
                    {
                        EditHabitat();
                    }
                }
            }
        }

        private void EditAnimal()
        {
            if (childDataGridView.CurrentRow == null)
            {
                MessageBox.Show("Please select a record to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataRowView rowView = (DataRowView)childDataGridView.CurrentRow.DataBoundItem;
                var values = ShowInputDialog(detailTable, rowView.Row);
                if (values != null)
                {
                    rowView.Row.BeginEdit();
                    foreach (var kvp in values)
                    {
                        rowView.Row[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    rowView.Row.EndEdit();
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditHabitat()
        {
            if (parentDataGridView.CurrentRow == null)
            {
                MessageBox.Show("Please select a record to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataRowView rowView = (DataRowView)parentDataGridView.CurrentRow.DataBoundItem;
                var values = ShowInputDialog(masterTable, rowView.Row);
                if (values != null)
                {
                    rowView.Row.BeginEdit();
                    foreach (var kvp in values)
                    {
                        rowView.Row[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    rowView.Row.EndEdit();
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteChild_Click(object sender, EventArgs e)
        {
            // Show selection dialog
            using (Form selectionForm = new Form())
            {
                selectionForm.Text = "Select Operation";
                selectionForm.Size = new System.Drawing.Size(300, 150);
                selectionForm.StartPosition = FormStartPosition.CenterParent;

                Label lblSelect = new Label { Text = "Select table to delete from:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                ComboBox cmbTable = new ComboBox { Location = new System.Drawing.Point(20, 50), Width = 240 };
                cmbTable.Items.AddRange(new string[] { "Animals", "Habitats" });

                Button btnProceed = new Button { Text = "Proceed", Location = new System.Drawing.Point(100, 80), DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(200, 80), DialogResult = DialogResult.Cancel };

                selectionForm.Controls.AddRange(new Control[] { lblSelect, cmbTable, btnProceed, btnCancel });

                if (selectionForm.ShowDialog() == DialogResult.OK)
                {
                    if (cmbTable.SelectedItem.ToString() == "Animals")
                    {
                        DeleteAnimal();
                    }
                    else
                    {
                        DeleteHabitat();
                    }
                }
            }
        }

        private void DeleteAnimal()
        {
            try
            {
                // Create input form
                using (Form inputForm = new Form())
                {
                    inputForm.Text = "Delete Animal";
                    inputForm.Size = new System.Drawing.Size(300, 150);
                    inputForm.StartPosition = FormStartPosition.CenterParent;

                    // Create input controls
                    Label lblAid = new Label { Text = "Animal ID:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                    TextBox txtAid = new TextBox { Location = new System.Drawing.Point(120, 20), Width = 150 };

                    Button btnSubmit = new Button { Text = "Delete", Location = new System.Drawing.Point(100, 80), DialogResult = DialogResult.OK };
                    Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(200, 80), DialogResult = DialogResult.Cancel };

                    // Add controls to form
                    inputForm.Controls.AddRange(new Control[] { lblAid, txtAid, btnSubmit, btnCancel });

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        int animalId = Convert.ToInt32(txtAid.Text);
                        DataRow[] rows = dataSet.Tables[detailTable.Name].Select($"aid = {animalId}");

                        if (rows.Length > 0)
                        {
                            if (MessageBox.Show("Are you sure you want to delete this animal?", "Confirm Delete", 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                rows[0].Delete();
                                SaveChanges();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Animal ID not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting animal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteHabitat()
        {
            try
            {
                // Create input form
                using (Form inputForm = new Form())
                {
                    inputForm.Text = "Delete Habitat";
                    inputForm.Size = new System.Drawing.Size(300, 150);
                    inputForm.StartPosition = FormStartPosition.CenterParent;

                    // Create input controls
                    Label lblHid = new Label { Text = "Habitat ID:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                    TextBox txtHid = new TextBox { Location = new System.Drawing.Point(120, 20), Width = 150 };

                    Button btnSubmit = new Button { Text = "Delete", Location = new System.Drawing.Point(100, 80), DialogResult = DialogResult.OK };
                    Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(200, 80), DialogResult = DialogResult.Cancel };

                    // Add controls to form
                    inputForm.Controls.AddRange(new Control[] { lblHid, txtHid, btnSubmit, btnCancel });

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        int habitatId = Convert.ToInt32(txtHid.Text);
                        DataRow[] rows = dataSet.Tables[masterTable.Name].Select($"hid = {habitatId}");

                        if (rows.Length > 0)
                        {
                            if (MessageBox.Show("Are you sure you want to delete this habitat? This will also delete all animals in this habitat.", "Confirm Delete", 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                rows[0].Delete();
                                SaveChanges();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Habitat ID not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting habitat: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                // Refresh the data
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
