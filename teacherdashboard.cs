using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeacherDashboard
{
    public partial class teacherdashboard : Form
    {
        private string teacherID; // Store the teacher's ID
        private string connString = "server=localhost;database=school_management;uid=root;pwd=;";

        public teacherdashboard(string id)
        {
            InitializeComponent();
            teacherID = id;

            // Configure the DataGridView before loading data
            ConfigureDataGridView();

            // Now load the schedule data
            LoadSchedule();
        }

        /// <summary>
        /// Configures the DataGridView with all desired settings
        /// </summary>
        private void ConfigureDataGridView()
        {
            try
            {
                // Prevent user editing
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.AllowUserToResizeColumns = false;
                dataGridView1.AllowUserToResizeRows = false;
                dataGridView1.AllowUserToOrderColumns = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;

                // Row selection settings
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                // Visual appearance settings
                dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.RowTemplate.Height = 30;
                dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                dataGridView1.GridColor = Color.LightGray;
                dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 9);

                // Header design
                dataGridView1.EnableHeadersVisualStyles = false;
                dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dataGridView1.ColumnHeadersHeight = 35;

                // Alternate row colors for better readability
                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;
                dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightSteelBlue;
                dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error configuring DataGridView: " + ex.Message, "Configuration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method to easily customize DataGridView appearance programmatically
        /// </summary>
        public void CustomizeDataGridView(
            Color headerBackColor = default,
            Color headerForeColor = default,
            Color gridColor = default,
            Color alternateRowColor = default,
            Color selectionBackColor = default,
            Color selectionForeColor = default,
            int rowHeight = 0,
            FontStyle headerFontStyle = FontStyle.Bold,
            string fontFamily = "Segoe UI",
            float fontSize = 9)
        {
            try
            {
                // Set only non-default values
                if (headerBackColor != default)
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;

                if (headerForeColor != default)
                    dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = headerForeColor;

                if (gridColor != default)
                    dataGridView1.GridColor = gridColor;

                if (alternateRowColor != default)
                    dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = alternateRowColor;

                if (selectionBackColor != default)
                    dataGridView1.DefaultCellStyle.SelectionBackColor = selectionBackColor;

                if (selectionForeColor != default)
                    dataGridView1.DefaultCellStyle.SelectionForeColor = selectionForeColor;

                if (rowHeight > 0)
                    dataGridView1.RowTemplate.Height = rowHeight;

                // Set font properties
                Font headerFont = new Font(fontFamily, fontSize, headerFontStyle);
                Font cellFont = new Font(fontFamily, fontSize);

                dataGridView1.ColumnHeadersDefaultCellStyle.Font = headerFont;
                dataGridView1.DefaultCellStyle.Font = cellFont;

                // Apply changes
                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error customizing DataGridView: " + ex.Message, "Customization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method to hide specific columns in the DataGridView
        /// </summary>
        public void HideColumns(params string[] columnNames)
        {
            try
            {
                foreach (string columnName in columnNames)
                {
                    if (dataGridView1.Columns.Contains(columnName))
                    {
                        dataGridView1.Columns[columnName].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error hiding columns: " + ex.Message, "Column Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method to rename column headers for better display
        /// </summary>
        public void RenameColumns(Dictionary<string, string> columnMappings)
        {
            try
            {
                foreach (var mapping in columnMappings)
                {
                    if (dataGridView1.Columns.Contains(mapping.Key))
                    {
                        dataGridView1.Columns[mapping.Key].HeaderText = mapping.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error renaming columns: " + ex.Message, "Column Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSchedule()
        {
            try
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
                {
                    conn.Open();

                    // Step 1: Get the teacher's full name for more accurate matching
                    string nameQuery = "SELECT first_name, last_name FROM faculty WHERE id_no = @id";
                    string firstName = "";
                    string lastName = "";

                    using (MySqlCommand nameCmd = new MySqlCommand(nameQuery, conn))
                    {
                        nameCmd.Parameters.AddWithValue("@id", teacherID);
                        using (MySqlDataReader reader = nameCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                firstName = reader["first_name"].ToString();
                                lastName = reader["last_name"].ToString();
                            }
                        }
                    }

                    // Step 2: Match any schedule where teacher field contains the first name or full name
                    if (!string.IsNullOrEmpty(firstName))
                    {
                        string fullName = $"{firstName} {lastName}".Trim();

                        string query = @"SELECT * FROM schedules
                                        WHERE teacher LIKE @firstName
                                        OR teacher LIKE @fullName
                                        ORDER BY
                                            CASE WHEN date IS NOT NULL THEN date ELSE start_date END ASC,
                                            time_in ASC";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@firstName", "%" + firstName + "%");
                            cmd.Parameters.AddWithValue("@fullName", "%" + fullName + "%");

                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                dataGridView1.DataSource = dt;

                                // Hide id and teacher columns
                                HideColumns("id", "teacher");

                                // Rename columns for better display
                                Dictionary<string, string> columnNames = new Dictionary<string, string>
                                {
                                    { "subject", "Subject" },
                                    { "room", "Room" },
                                    { "date", "Date" },
                                    { "time_in", "Start Time" },
                                    { "time_out", "End Time" },
                                    { "is_recurring", "Recurring" },
                                    { "recurring_days", "Days" },
                                    { "start_date", "Start Date" },
                                    { "end_date", "End Date" }
                                };
                                RenameColumns(columnNames);

                                // Highlight today's schedules
                                HighlightTodaysSchedules();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Teacher information not found.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading schedule: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Highlights schedules for the current day
        /// </summary>
        private void HighlightTodaysSchedules()
        {
            try
            {
                DateTime today = DateTime.Today;
                string todayString = today.ToString("yyyy-MM-dd");
                DayOfWeek todayDayOfWeek = today.DayOfWeek;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    bool isToday = false;

                    // Check one-time events
                    if (row.Cells["date"].Value != DBNull.Value)
                    {
                        DateTime date = Convert.ToDateTime(row.Cells["date"].Value);
                        if (date.Date == today)
                        {
                            isToday = true;
                        }
                    }

                    // Check recurring events
                    if (!isToday && row.Cells["is_recurring"].Value != DBNull.Value &&
                        Convert.ToBoolean(row.Cells["is_recurring"].Value))
                    {
                        // Check if today is within the recurring range
                        if (row.Cells["start_date"].Value != DBNull.Value &&
                            row.Cells["end_date"].Value != DBNull.Value)
                        {
                            DateTime startDate = Convert.ToDateTime(row.Cells["start_date"].Value);
                            DateTime endDate = Convert.ToDateTime(row.Cells["end_date"].Value);

                            if (today >= startDate && today <= endDate)
                            {
                                // Check if today's day of week is in the recurring days
                                if (row.Cells["recurring_days"].Value != DBNull.Value)
                                {
                                    string recurringDays = row.Cells["recurring_days"].Value.ToString();
                                    if (recurringDays.Contains(todayDayOfWeek.ToString()))
                                    {
                                        isToday = true;
                                    }
                                }
                            }
                        }
                    }

                    // Apply highlighting
                    if (isToday)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.DefaultCellStyle.Font = new Font(dataGridView1.DefaultCellStyle.Font, FontStyle.Bold);
                    }
                }
            }
            catch (Exception ex)
            {
                // Just log the error without interrupting the user
                Console.WriteLine("Error highlighting today's schedules: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            user sr = new user(teacherID);
            sr.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            frm.Show();
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            schedule sched = new schedule(teacherID);
            sched.Show();
            this.Hide();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }

        private void teacherdashboard_Load(object sender, EventArgs e)
        {
        }
    }
}