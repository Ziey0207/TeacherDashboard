using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeacherDashboard
{
    public partial class schedule : Form
    {
        private int day, month, year;
        public static int static_month, static_year;

        private string teacherID;

        public schedule(string id)
        {
            InitializeComponent();
            teacherID = id;
        }

        private void schedule_Load(object sender, EventArgs e)
        {
      
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            teacherdashboard teach = new teacherdashboard(teacherID);
            teach.Show();
            this.Close();
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void schedule_Load_1(object sender, EventArgs e)
        {
            displaDays();
        }

        private void displaDays()
        {
            DateTime now = DateTime.Now;
            month = now.Month;
            year = now.Year;

            String monthname = DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
            lbdate.Text = monthname + " " + year;

            static_month = month;
            static_year = year;
            DateTime startofthemonth = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);
            int dayoftheweek = Convert.ToInt32(startofthemonth.DayOfWeek.ToString("d")) + 1;

            for (int i = 1; i < dayoftheweek; i++)
            {
                UserControlBlank ucb = new UserControlBlank();
                daycontainers.Controls.Add(ucb);
            }

            for (int i = 1; i <= days; i++)
            {
                DateTime currentDate = new DateTime(year, month, i);
                UserControlDays ucd = new UserControlDays(teacherID, day, month, year);  // Ipasok ang eksaktong petsa
                ucd.days(i);
                daycontainers.Controls.Add(ucd);
            }

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            daycontainers.Controls.Clear();

            month++;

            String monthname = DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
            lbdate.Text = monthname + " " + year;

            DateTime startofthemonth = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);
            int dayoftheweek = Convert.ToInt32(startofthemonth.DayOfWeek.ToString("d")) + 1;

            for (int i = 1; i < dayoftheweek; i++)
            {
                UserControlBlank ucb = new UserControlBlank();
                daycontainers.Controls.Add(ucb);
            }

            for (int i = 1; i <= days; i++)
            {
                DateTime currentDate = new DateTime(year, month, i);
                UserControlDays ucd = new UserControlDays(teacherID, day, month, year);  // Ipasok ang eksaktong petsa
                ucd.days(i);
                daycontainers.Controls.Add(ucd);
            }

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            daycontainers.Controls.Clear();

            month--;

            String monthname = DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
            lbdate.Text = monthname + " " + year;

            DateTime startofthemonth = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);
            int dayoftheweek = Convert.ToInt32(startofthemonth.DayOfWeek.ToString("d")) + 1;

            for (int i = 1; i < dayoftheweek; i++)
            {
                UserControlBlank ucb = new UserControlBlank();
                daycontainers.Controls.Add(ucb);
            }

            for (int i = 1; i <= days; i++)
            {
                DateTime currentDate = new DateTime(year, month, i);
                UserControlDays ucd = new UserControlDays(teacherID, day, month, year);  // Ipasok ang eksaktong petsa
                ucd.days(i);
                daycontainers.Controls.Add(ucd);
            }

        }
    }
}
