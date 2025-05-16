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
    public partial class changepas : Form
    {
        private string connString = "server=localhost;database=school_management;uid=root;pwd=;";
        private string teacherID;

        public changepas(string id)
        {
            InitializeComponent();
            teacherID = id;
            LoadProfile();
        }

        private void LoadProfile()
        {
            using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
            {
                conn.Open();
                string query = "SELECT * FROM faculty WHERE id_no = @TeacherID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtCurrentId.Text = reader["id_no"].ToString();
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            user sr = new user(teacherID);
            sr.Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            schedule sched = new schedule(teacherID);
            sched.Show();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            teacherdashboard teach = new teacherdashboard(teacherID);
            teach.Show();
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewId.Text))
            {
                MessageBox.Show("Please enter a new ID before saving.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
                {
                    conn.Open();

                    string query = @"UPDATE faculty
                             SET id_no = @idno
                             WHERE id_no = @TeacherID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idno", txtNewId.Text);
                        cmd.Parameters.AddWithValue("@TeacherID", teacherID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("ID updated successfully!");
                            Form1 frm = new Form1();
                            frm.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("No changes were made.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            frm.Show();

            this.Close();
        }
    }
}