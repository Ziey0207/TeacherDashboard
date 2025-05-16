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
    public partial class user : Form
    {
        private string connString = "server=localhost;database=school_management;uid=root;pwd=;";
        private string teacherID;

        public user(string id)
        {
            InitializeComponent();
            teacherID = id;
            LoadProfile();
        }

        public static class SessionData
        {
            public static string TeacherID { get; set; }
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
                            txtFirstName.Text = reader["first_name"].ToString();
                            txtMiddleInitial.Text = reader["middle_name"].ToString();
                            txtLastName.Text = reader["last_name"].ToString();
                            txtEmail.Text = reader["email"].ToString();
                            txtContactNumber.Text = reader["contact_number"].ToString();
                            string gender = reader["gender"].ToString();
                            if (gender == "Male" || gender == "Female")
                            {
                                cmbGender.SelectedItem = gender;
                            }
                            txtAddress.Text = reader["address"].ToString();
                        }
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            changepas chg = new changepas(teacherID);
            chg.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            teacherdashboard teach = new teacherdashboard(teacherID);
            teach.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            schedule sched = new schedule(teacherID);
            sched.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            frm.Show();
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
                {
                    conn.Open();

                    string query = @"UPDATE faculty
                             SET first_name = @FirstName,
                                 middle_name = @MiddleName,
                                 last_name = @LastName,
                                 email = @Email,
                                 contact_number = @ContactNumber,
                                 gender = @Gender,
                                 address = @Address
                             WHERE id_no = @TeacherID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@MiddleName", txtMiddleInitial.Text);
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@ContactNumber", txtContactNumber.Text);
                        cmd.Parameters.AddWithValue("@Gender", cmbGender.Text); // combo box
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@TeacherID", teacherID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Profile updated successfully!");
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
    }
}