using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Bioparco_Di_Roma
{
    public partial class Form1 : Form
    {
        private SqlConnection connection;
        private DataTable currentTable;

        public Form1()
        {
            InitializeComponent();
            connection = new SqlConnection();
            btnConnect.Click += BtnConnect_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            cboTables.SelectedIndexChanged += CboTables_SelectedIndexChanged;
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
                
                // Load available tables
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to database: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTables()
        {
            try
            {
                cboTables.Items.Clear();
                DataTable tables = connection.GetSchema("Tables");
                foreach (DataRow row in tables.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    cboTables.Items.Add(tableName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tables: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboTables.SelectedItem == null) return;

            try
            {
                string tableName = cboTables.SelectedItem.ToString();
                string query = $"SELECT * FROM {tableName}";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                currentTable = new DataTable();
                adapter.Fill(currentTable);
                dataGridView1.DataSource = currentTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (cboTables.SelectedItem != null)
            {
                CboTables_SelectedIndexChanged(sender, e);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (currentTable == null) return;

            try
            {
                DataRow newRow = currentTable.NewRow();
                currentTable.Rows.Add(newRow);
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new row: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            try
            {
                if (MessageBox.Show("Are you sure you want to delete this row?", "Confirm Delete", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting row: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveChanges()
        {
            if (currentTable == null || cboTables.SelectedItem == null) return;

            try
            {
                string tableName = cboTables.SelectedItem.ToString();
                SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM {tableName}", connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                adapter.Update(currentTable);
                MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
