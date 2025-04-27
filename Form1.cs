using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TeacherDashboard
{

    public partial class Form1 : Form
    {
        string connString = "server=localhost;database=school_management;uid=root;pwd=;";
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (txtIDNumber.Text.Length == 11) 
            {
                CheckIDAndLogin();
            }
        }

        private void CheckIDAndLogin()
        {
            string idNumber = txtIDNumber.Text;

            try
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT * FROM faculty WHERE id_no= @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idNumber);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // ✅ ID is valid
                            {
                                string teacherID = reader["id_no"].ToString(); // Get teacher ID

                                MessageBox.Show("Login Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Hide();

                                schedule mainForm = new schedule(teacherID); // ✅ Pass teacher ID
                                mainForm.Show();
                            }
                            else
                            {
                                MessageBox.Show("Invalid ID!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtIDNumber.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }   

            private void textBox1_Leave(object sender, EventArgs e)
        {

        }
    }
}
