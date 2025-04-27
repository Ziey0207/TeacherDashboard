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
        string connString = "server=localhost;database=school_management;uid=root;pwd=;";


        public teacherdashboard(string id)
        {
            InitializeComponent();
            teacherID = id;
            LoadSchedule(); // Load only this teacher’s schedule
        }

        private void LoadSchedule()
        {
            try
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
                {
                    conn.Open();

                    // Step 1: Get the first name of the teacher
                    string nameQuery = "SELECT first_name FROM faculty WHERE id_no = @id";
                    string firstName = "";

                    using (MySqlCommand nameCmd = new MySqlCommand(nameQuery, conn))
                    {
                        nameCmd.Parameters.AddWithValue("@id", teacherID);
                        using (MySqlDataReader reader = nameCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                firstName = reader["first_name"].ToString();
                            }
                        }
                    }

                    // Step 2: Match any schedule where teacher field contains the first name
                    if (!string.IsNullOrEmpty(firstName))
                    {
                        string query = "SELECT * FROM schedules WHERE teacher LIKE @firstName";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@firstName", "%" + firstName + "%");
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                dataGridView1.DataSource = dt;

                                // HIDE id and teacher columns
                                if (dataGridView1.Columns.Contains("id"))
                                {
                                    dataGridView1.Columns["id"].Visible = false;
                                }
                                if (dataGridView1.Columns.Contains("teacher"))
                                {
                                    dataGridView1.Columns["teacher"].Visible = false;
                                }

                                // REMOVE gray row
                                dataGridView1.AllowUserToAddRows = false;

                                // DESIGN SETTINGS

                                // Set all text to center
                                dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                                dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                                // Auto-size the columns to fill
                                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                                // Increase row height (mas mataas)
                                dataGridView1.RowTemplate.Height = 30;

                                // Nice grid design
                                dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                                dataGridView1.GridColor = Color.LightGray;
                                dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 9);

                                // Header design
                                dataGridView1.EnableHeadersVisualStyles = false;
                                dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
                                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);


                                // Optional: Highlight today
                                foreach (DataGridViewRow row in dataGridView1.Rows)
                                {
                                    foreach (DataGridViewCell cell in row.Cells)
                                    {
                                        if (cell.Value != null && cell.Value.ToString().StartsWith(DateTime.Now.Day.ToString()))
                                        {
                                            cell.Style.BackColor = Color.LightBlue;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("First name not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading schedule: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            user sr = new user(teacherID);
            sr.Show();
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            frm.Show();

             this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            schedule sched = new schedule(teacherID);
            sched.Show();
            this.Close();
        }

        private void teacherdashboard_Load(object sender, EventArgs e)
        {

        }
    }
}
