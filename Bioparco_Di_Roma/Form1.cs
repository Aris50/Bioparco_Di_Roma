using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Bioparco_Di_Roma
{
    public partial class Form1 : Form
    {
        private SqlConnection connection;
        private DataSet dataSet;
        private SqlDataAdapter parentAdapter;
        private SqlDataAdapter childAdapter;
        private DataRelation relation;
        private string parentTableName = "Habitat";
        private string childTableName = "Animal";
        private string parentKeyColumn = "hid";
        private string childKeyColumn = "hid";

        public Form1()
        {
            InitializeComponent();
            connection = new SqlConnection();
            dataSet = new DataSet();
            
            // Set up event handlers
            btnConnect.Click += BtnConnect_Click;
            btnAddChild.Click += BtnAddChild_Click;
            btnEditChild.Click += BtnEditChild_Click;
            btnDeleteChild.Click += BtnDeleteChild_Click;
            parentDataGridView.SelectionChanged += ParentDataGridView_SelectionChanged;

            // Update labels to show actual table names
            lblParent.Text = "Habitats:";
            lblChild.Text = "Animals:";
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

        private void InitializeDataAdapters()
        {
            // Create parent adapter for Habitats
            parentAdapter = new SqlDataAdapter($"SELECT * FROM {parentTableName}", connection);
            SqlCommandBuilder parentBuilder = new SqlCommandBuilder(parentAdapter);

            // Create child adapter for Animals
            childAdapter = new SqlDataAdapter($"SELECT * FROM {childTableName}", connection);
            SqlCommandBuilder childBuilder = new SqlCommandBuilder(childAdapter);
        }

        private void LoadData()
        {
            try
            {
                // Clear existing data
                dataSet.Clear();

                // Fill parent table (Habitats)
                parentAdapter.Fill(dataSet, parentTableName);

                // Fill child table (Animals)
                childAdapter.Fill(dataSet, childTableName);

                // Create relation between Habitats and Animals
                relation = new DataRelation(
                    "HabitatAnimals",
                    dataSet.Tables[parentTableName].Columns[parentKeyColumn],
                    dataSet.Tables[childTableName].Columns[childKeyColumn]
                );
                dataSet.Relations.Add(relation);

                // Bind parent data (Habitats)
                parentDataGridView.DataSource = dataSet;
                parentDataGridView.DataMember = parentTableName;

                // Make parent grid read-only
                parentDataGridView.ReadOnly = true;
                parentDataGridView.AllowUserToAddRows = false;
                parentDataGridView.AllowUserToDeleteRows = false;

                // Bind child data (Animals)
                childDataGridView.DataSource = dataSet;
                childDataGridView.DataMember = $"{parentTableName}.HabitatAnimals";

                // Make child grid read-only except for animal ID column
                childDataGridView.ReadOnly = true;
                childDataGridView.AllowUserToAddRows = false;
                childDataGridView.AllowUserToDeleteRows = false;

                // Find the animal ID column and make it editable
                foreach (DataGridViewColumn column in childDataGridView.Columns)
                {
                    if (column.Name == "aid")
                    {
                        column.ReadOnly = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private bool ValidateAnimalFields(TextBox txtAid, TextBox txtName, TextBox txtSpecies, TextBox txtAge, 
            ComboBox cmbGender, ComboBox cmbVertebrateClass, TextBox txtBodyTemperature, 
            TextBox txtWeight, TextBox txtVid, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validate Animal ID
            if (!int.TryParse(txtAid.Text, out int aid) || aid <= 0)
            {
                errorMessage = "Animal ID must be a positive number.";
                return false;
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorMessage = "Name is required.";
                return false;
            }

            // Validate Species
            if (string.IsNullOrWhiteSpace(txtSpecies.Text))
            {
                errorMessage = "Species is required.";
                return false;
            }

            // Validate Age
            if (!int.TryParse(txtAge.Text, out int age) || age < 0 || age > 100)
            {
                errorMessage = "Age must be a number between 0 and 100.";
                return false;
            }

            // Validate Gender
            if (cmbGender.SelectedItem == null)
            {
                errorMessage = "Gender must be selected.";
                return false;
            }

            // Validate Vertebrate Class
            if (cmbVertebrateClass.SelectedItem == null)
            {
                errorMessage = "Vertebrate Class must be selected.";
                return false;
            }

            // Validate Body Temperature
            if (!double.TryParse(txtBodyTemperature.Text, out double temperature) || temperature < 0 || temperature > 50)
            {
                errorMessage = "Body Temperature must be a number between 0 and 50.";
                return false;
            }

            // Validate Weight
            if (!double.TryParse(txtWeight.Text, out double weight) || weight <= 0)
            {
                errorMessage = "Weight must be a positive number.";
                return false;
            }

            // Validate Vet ID
            if (!int.TryParse(txtVid.Text, out int vid) || vid <= 0)
            {
                errorMessage = "Vet ID must be a positive number.";
                return false;
            }

            return true;
        }

        private bool ValidateHabitatFields(TextBox txtHid, TextBox txtLandscape, TextBox txtSize, 
            TextBox txtTemperature, TextBox txtHumidity, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validate Habitat ID
            if (!int.TryParse(txtHid.Text, out int hid) || hid <= 0)
            {
                errorMessage = "Habitat ID must be a positive number.";
                return false;
            }

            // Validate Landscape
            if (string.IsNullOrWhiteSpace(txtLandscape.Text))
            {
                errorMessage = "Landscape is required.";
                return false;
            }

            // Validate Size
            if (!double.TryParse(txtSize.Text, out double size) || size <= 0)
            {
                errorMessage = "Size must be a positive number.";
                return false;
            }

            // Validate Temperature
            if (!double.TryParse(txtTemperature.Text, out double temperature) || temperature < -50 || temperature > 50)
            {
                errorMessage = "Temperature must be a number between -50 and 50.";
                return false;
            }

            // Validate Humidity
            if (!double.TryParse(txtHumidity.Text, out double humidity) || humidity < 0 || humidity > 100)
            {
                errorMessage = "Humidity must be a number between 0 and 100.";
                return false;
            }

            return true;
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
                int habitatId = Convert.ToInt32(habitatRow[parentKeyColumn]);

                // Create input form
                using (Form inputForm = new Form())
                {
                    inputForm.Text = "Add New Animal";
                    inputForm.Size = new System.Drawing.Size(400, 600);
                    inputForm.StartPosition = FormStartPosition.CenterParent;

                    // Create input controls with mock data
                    int yPos = 20;
                    int labelWidth = 120;
                    int textBoxWidth = 200;
                    int spacing = 40;

                    // Animal ID
                    Label lblAid = new Label { Text = "Animal ID:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtAid = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "1001" };
                    yPos += spacing;

                    // Name
                    Label lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtName = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "Leo" };
                    yPos += spacing;

                    // Species
                    Label lblSpecies = new Label { Text = "Species:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtSpecies = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "Lion" };
                    yPos += spacing;

                    // Age
                    Label lblAge = new Label { Text = "Age:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtAge = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "5" };
                    yPos += spacing;

                    // Gender
                    Label lblGender = new Label { Text = "Gender:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    ComboBox cmbGender = new ComboBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth };
                    cmbGender.Items.AddRange(new string[] { "M", "F" });
                    cmbGender.SelectedIndex = 0;
                    yPos += spacing;

                    // Vertebrate Class
                    Label lblVertebrateClass = new Label { Text = "Vertebrate Class:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    ComboBox cmbVertebrateClass = new ComboBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth };
                    cmbVertebrateClass.Items.AddRange(new string[] { "Mammal", "Bird", "Reptile", "Amphibian", "Fish" });
                    cmbVertebrateClass.SelectedIndex = 0;
                    yPos += spacing;

                    // Body Temperature
                    Label lblBodyTemperature = new Label { Text = "Body Temperature:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtBodyTemperature = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "37.5" };
                    yPos += spacing;

                    // Weight
                    Label lblWeight = new Label { Text = "Weight (kg):", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtWeight = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "190" };
                    yPos += spacing;

                    // Vet ID
                    Label lblVid = new Label { Text = "Vet ID:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtVid = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "1" };
                    yPos += spacing;

                    // Buttons
                    Button btnSubmit = new Button { Text = "Add Animal", Location = new System.Drawing.Point(150, yPos), DialogResult = DialogResult.OK };
                    Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(250, yPos), DialogResult = DialogResult.Cancel };

                    // Add controls to form
                    inputForm.Controls.AddRange(new Control[] {
                        lblAid, txtAid,
                        lblName, txtName,
                        lblSpecies, txtSpecies,
                        lblAge, txtAge,
                        lblGender, cmbGender,
                        lblVertebrateClass, cmbVertebrateClass,
                        lblBodyTemperature, txtBodyTemperature,
                        lblWeight, txtWeight,
                        lblVid, txtVid,
                        btnSubmit, btnCancel
                    });

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        // Validate fields
                        if (!ValidateAnimalFields(txtAid, txtName, txtSpecies, txtAge, cmbGender, 
                            cmbVertebrateClass, txtBodyTemperature, txtWeight, txtVid, out string errorMessage))
                        {
                            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Create new animal row
                        DataRow newAnimalRow = dataSet.Tables[childTableName].NewRow();
                        newAnimalRow["aid"] = Convert.ToInt32(txtAid.Text);
                        newAnimalRow["name"] = txtName.Text;
                        newAnimalRow["species"] = txtSpecies.Text;
                        newAnimalRow["age"] = Convert.ToInt32(txtAge.Text);
                        newAnimalRow["gender"] = cmbGender.SelectedItem.ToString();
                        newAnimalRow["vertebrate_class"] = cmbVertebrateClass.SelectedItem.ToString();
                        newAnimalRow["bodytemperature"] = Convert.ToDouble(txtBodyTemperature.Text);
                        newAnimalRow["weight"] = Convert.ToDouble(txtWeight.Text);
                        newAnimalRow["vid"] = Convert.ToInt32(txtVid.Text);
                        newAnimalRow[childKeyColumn] = habitatId;

                        dataSet.Tables[childTableName].Rows.Add(newAnimalRow);
                        SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding animal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddHabitat()
        {
            try
            {
                // Create input form
                using (Form inputForm = new Form())
                {
                    inputForm.Text = "Add New Habitat";
                    inputForm.Size = new System.Drawing.Size(400, 300);
                    inputForm.StartPosition = FormStartPosition.CenterParent;

                    // Create input controls with mock data
                    int yPos = 20;
                    int labelWidth = 120;
                    int textBoxWidth = 200;
                    int spacing = 40;

                    // Habitat ID
                    Label lblHid = new Label { Text = "Habitat ID:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtHid = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "1" };
                    yPos += spacing;

                    // Landscape
                    Label lblLandscape = new Label { Text = "Landscape:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtLandscape = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "Savannah" };
                    yPos += spacing;

                    // Size
                    Label lblSize = new Label { Text = "Size:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtSize = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "1000" };
                    yPos += spacing;

                    // Temperature
                    Label lblTemperature = new Label { Text = "Temperature:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtTemperature = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "25.5" };
                    yPos += spacing;

                    // Humidity
                    Label lblHumidity = new Label { Text = "Humidity:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtHumidity = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = "60.0" };
                    yPos += spacing;

                    // Buttons
                    Button btnSubmit = new Button { Text = "Add Habitat", Location = new System.Drawing.Point(150, yPos), DialogResult = DialogResult.OK };
                    Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(250, yPos), DialogResult = DialogResult.Cancel };

                    // Add controls to form
                    inputForm.Controls.AddRange(new Control[] {
                        lblHid, txtHid,
                        lblLandscape, txtLandscape,
                        lblSize, txtSize,
                        lblTemperature, txtTemperature,
                        lblHumidity, txtHumidity,
                        btnSubmit, btnCancel
                    });

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        // Validate fields
                        if (!ValidateHabitatFields(txtHid, txtLandscape, txtSize, txtTemperature, 
                            txtHumidity, out string errorMessage))
                        {
                            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Create new habitat row
                        DataRow newHabitatRow = dataSet.Tables[parentTableName].NewRow();
                        newHabitatRow["hid"] = Convert.ToInt32(txtHid.Text);
                        newHabitatRow["landscape"] = txtLandscape.Text;
                        newHabitatRow["size"] = Convert.ToDouble(txtSize.Text);
                        newHabitatRow["temperature"] = Convert.ToDouble(txtTemperature.Text);
                        newHabitatRow["humidity"] = Convert.ToDouble(txtHumidity.Text);

                        dataSet.Tables[parentTableName].Rows.Add(newHabitatRow);
                        SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding habitat: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Please select an animal to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Get the selected animal's DataRow
                DataRowView animalRowView = (DataRowView)childDataGridView.CurrentRow.DataBoundItem;
                DataRow animalRow = animalRowView.Row;

                // Create input form
                using (Form inputForm = new Form())
                {
                    inputForm.Text = "Edit Animal";
                    inputForm.Size = new System.Drawing.Size(400, 600);
                    inputForm.StartPosition = FormStartPosition.CenterParent;

                    // Create input controls with current data
                    int yPos = 20;
                    int labelWidth = 120;
                    int textBoxWidth = 200;
                    int spacing = 40;

                    // Animal ID (read-only)
                    Label lblAid = new Label { Text = "Animal ID:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtAid = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = animalRow["aid"].ToString(), Enabled = false };
                    yPos += spacing;

                    // Name
                    Label lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtName = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = animalRow["name"].ToString() };
                    yPos += spacing;

                    // Species
                    Label lblSpecies = new Label { Text = "Species:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtSpecies = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = animalRow["species"].ToString() };
                    yPos += spacing;

                    // Age
                    Label lblAge = new Label { Text = "Age:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtAge = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = animalRow["age"].ToString() };
                    yPos += spacing;

                    // Gender
                    Label lblGender = new Label { Text = "Gender:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    ComboBox cmbGender = new ComboBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth };
                    cmbGender.Items.AddRange(new string[] { "M", "F" });
                    cmbGender.SelectedItem = animalRow["gender"].ToString();
                    yPos += spacing;

                    // Vertebrate Class
                    Label lblVertebrateClass = new Label { Text = "Vertebrate Class:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    ComboBox cmbVertebrateClass = new ComboBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth };
                    cmbVertebrateClass.Items.AddRange(new string[] { "Mammal", "Bird", "Reptile", "Amphibian", "Fish" });
                    cmbVertebrateClass.SelectedItem = animalRow["vertebrate_class"].ToString();
                    yPos += spacing;

                    // Body Temperature
                    Label lblBodyTemperature = new Label { Text = "Body Temperature:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtBodyTemperature = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = animalRow["bodytemperature"].ToString() };
                    yPos += spacing;

                    // Weight
                    Label lblWeight = new Label { Text = "Weight (kg):", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtWeight = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = animalRow["weight"].ToString() };
                    yPos += spacing;

                    // Vet ID
                    Label lblVid = new Label { Text = "Vet ID:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtVid = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = animalRow["vid"].ToString() };
                    yPos += spacing;

                    // Buttons
                    Button btnSubmit = new Button { Text = "Save Changes", Location = new System.Drawing.Point(150, yPos), DialogResult = DialogResult.OK };
                    Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(250, yPos), DialogResult = DialogResult.Cancel };

                    // Add controls to form
                    inputForm.Controls.AddRange(new Control[] {
                        lblAid, txtAid,
                        lblName, txtName,
                        lblSpecies, txtSpecies,
                        lblAge, txtAge,
                        lblGender, cmbGender,
                        lblVertebrateClass, cmbVertebrateClass,
                        lblBodyTemperature, txtBodyTemperature,
                        lblWeight, txtWeight,
                        lblVid, txtVid,
                        btnSubmit, btnCancel
                    });

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        // Update animal row
                        animalRow.BeginEdit();
                        animalRow["name"] = txtName.Text;
                        animalRow["species"] = txtSpecies.Text;
                        animalRow["age"] = Convert.ToInt32(txtAge.Text);
                        animalRow["gender"] = cmbGender.SelectedItem.ToString();
                        animalRow["vertebrate_class"] = cmbVertebrateClass.SelectedItem.ToString();
                        animalRow["bodytemperature"] = Convert.ToDouble(txtBodyTemperature.Text);
                        animalRow["weight"] = Convert.ToDouble(txtWeight.Text);
                        animalRow["vid"] = Convert.ToInt32(txtVid.Text);
                        animalRow.EndEdit();

                        SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing animal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditHabitat()
        {
            if (parentDataGridView.CurrentRow == null)
            {
                MessageBox.Show("Please select a habitat to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Get the selected habitat's DataRow
                DataRowView habitatRowView = (DataRowView)parentDataGridView.CurrentRow.DataBoundItem;
                DataRow habitatRow = habitatRowView.Row;

                // Create input form
                using (Form inputForm = new Form())
                {
                    inputForm.Text = "Edit Habitat";
                    inputForm.Size = new System.Drawing.Size(400, 300);
                    inputForm.StartPosition = FormStartPosition.CenterParent;

                    // Create input controls with current data
                    int yPos = 20;
                    int labelWidth = 120;
                    int textBoxWidth = 200;
                    int spacing = 40;

                    // Habitat ID (read-only)
                    Label lblHid = new Label { Text = "Habitat ID:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtHid = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = habitatRow["hid"].ToString(), Enabled = false };
                    yPos += spacing;

                    // Landscape
                    Label lblLandscape = new Label { Text = "Landscape:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtLandscape = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = habitatRow["landscape"].ToString() };
                    yPos += spacing;

                    // Size
                    Label lblSize = new Label { Text = "Size:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtSize = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = habitatRow["size"].ToString() };
                    yPos += spacing;

                    // Temperature
                    Label lblTemperature = new Label { Text = "Temperature:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtTemperature = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = habitatRow["temperature"].ToString() };
                    yPos += spacing;

                    // Humidity
                    Label lblHumidity = new Label { Text = "Humidity:", Location = new System.Drawing.Point(20, yPos), Width = labelWidth };
                    TextBox txtHumidity = new TextBox { Location = new System.Drawing.Point(150, yPos), Width = textBoxWidth, Text = habitatRow["humidity"].ToString() };
                    yPos += spacing;

                    // Buttons
                    Button btnSubmit = new Button { Text = "Save Changes", Location = new System.Drawing.Point(150, yPos), DialogResult = DialogResult.OK };
                    Button btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(250, yPos), DialogResult = DialogResult.Cancel };

                    // Add controls to form
                    inputForm.Controls.AddRange(new Control[] {
                        lblHid, txtHid,
                        lblLandscape, txtLandscape,
                        lblSize, txtSize,
                        lblTemperature, txtTemperature,
                        lblHumidity, txtHumidity,
                        btnSubmit, btnCancel
                    });

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        // Update habitat row
                        habitatRow.BeginEdit();
                        habitatRow["landscape"] = txtLandscape.Text;
                        habitatRow["size"] = Convert.ToDouble(txtSize.Text);
                        habitatRow["temperature"] = Convert.ToDouble(txtTemperature.Text);
                        habitatRow["humidity"] = Convert.ToDouble(txtHumidity.Text);
                        habitatRow.EndEdit();

                        SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing habitat: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        DataRow[] rows = dataSet.Tables[childTableName].Select($"aid = {animalId}");

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
                        DataRow[] rows = dataSet.Tables[parentTableName].Select($"hid = {habitatId}");

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
                // Update habitat changes (we no longer need it but it doesnt do any bad)
                parentAdapter.Update(dataSet, parentTableName);

                // Update animal changes
                childAdapter.Update(dataSet, childTableName);

                // Clear and reload the data to refresh the views
                dataSet.Clear();
                parentAdapter.Fill(dataSet, parentTableName);
                childAdapter.Fill(dataSet, childTableName);

                // Remove existing relation if it exists
                if (dataSet.Relations.Contains("HabitatAnimals"))
                {
                    dataSet.Relations.Remove("HabitatAnimals");
                }

                // Create the relation
                relation = new DataRelation(
                    "HabitatAnimals",
                    dataSet.Tables[parentTableName].Columns[parentKeyColumn],
                    dataSet.Tables[childTableName].Columns[childKeyColumn]
                );
                dataSet.Relations.Add(relation);

                // Rebind the data
                parentDataGridView.DataSource = dataSet;
                parentDataGridView.DataMember = parentTableName;

                childDataGridView.DataSource = dataSet;
                childDataGridView.DataMember = $"{parentTableName}.HabitatAnimals";

                MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
