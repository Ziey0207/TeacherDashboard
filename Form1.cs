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
            this.FormBorderStyle = FormBorderStyle.None;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 0x1;
            const int HTCAPTION = 0x2;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));
                if (pos.X < 5)
                {
                    if (pos.Y < 5)
                        m.Result = (IntPtr)HTTOPLEFT;
                    else if (pos.Y > ClientSize.Height - 5)
                        m.Result = (IntPtr)HTBOTTOMLEFT;
                    else
                        m.Result = (IntPtr)HTLEFT;
                }
                else if (pos.X > ClientSize.Width - 5)
                {
                    if (pos.Y < 5)
                        m.Result = (IntPtr)HTTOPRIGHT;
                    else if (pos.Y > ClientSize.Height - 5)
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else
                        m.Result = (IntPtr)HTRIGHT;
                }
                else if (pos.Y < 5)
                {
                    m.Result = (IntPtr)HTTOP;
                }
                else if (pos.Y > ClientSize.Height - 5)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                }
                else
                {
                    m.Result = (IntPtr)HTCAPTION;
                }
                return;
            }
            base.WndProc(ref m);
        }

        private void hopeTextBox1_Enter(object sender, EventArgs e)
        {
            if (txtID.Text == "Enter Faculty ID")
            {
                txtID.Text = "";
            }
        }

        private void hopeTextBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                txtID.Text = "Enter Faculty ID";
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
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