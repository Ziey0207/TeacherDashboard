using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeacherDashboard
{
    public partial class UserControlDays : UserControl
    {
        private int day, month, year;
        string connString = "server=localhost;database=school_management;uid=root;pwd=;";
        private string teacherID;
        public UserControlDays(string id, int day, int month, int year)
        {
            InitializeComponent();
            teacherID = id;
            this.day = day;
            this.month = month;
            this.year = year;
        }

        private void UserControlDays_Load(object sender, EventArgs e)
        {

        }
        public void AddScheduleInfo(string subject, string room, string timeIn, string timeOut)
        {
            Label lblInfo = new Label();
            lblInfo.Text = $"{subject}\nRoom {room}\n{timeIn} - {timeOut}";
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Arial", 7); // Adjust para kasya
            lblInfo.Location = new Point(5, 25); // Adjust sa ilalim ng date
            this.Controls.Add(lblInfo);
        }

        public void days(int numday)
        {
            day = numday; // Update the day for the calendar view
            lbdays.Text = day.ToString();
            displayEvent(); // Now display the event based on this da
        }

        public void displayEvent()
        {
            lbevents.Text = "";

            using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
            {
                conn.Open();

                // Get the full name of the teacher from id_no
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
                string query = "SELECT * FROM schedules WHERE teacher LIKE @firstName AND date = @date";


                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", "%" + firstName + "%");
                    cmd.Parameters.AddWithValue("@date", new DateTime(year, month, day).ToString("yyyy-MM-dd")); // format ng date

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            StringBuilder eventDetails = new StringBuilder();
                            while (reader.Read())
                            {
                                string subject = reader["subject"].ToString().Trim();
                                string timeIn = reader["time_in"].ToString().Trim();
                                string timeOut = reader["time_out"].ToString().Trim();
                                string room = reader["room"].ToString().Trim();

                                eventDetails.AppendLine($"{subject}");
                                eventDetails.AppendLine($"{timeIn} - {timeOut}");
                                eventDetails.AppendLine($"{room}");
                                eventDetails.AppendLine("---------------------------");
                            }
                            lbevents.Text = eventDetails.ToString();
                        }
                        else
                        {
                            lbevents.Text = ""; 
                        }
                    }
                }
            }
        }
    }
}
