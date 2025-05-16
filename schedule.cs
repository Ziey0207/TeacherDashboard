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

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            teacherdashboard teach = new teacherdashboard(teacherID);
            teach.Show();
            this.Hide();
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
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

            DisplayCalendar();
        }

        private void DisplayCalendar()
        {
            // Clear existing controls
            daycontainers.Controls.Clear();

            DateTime startofthemonth = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);
            int dayoftheweek = Convert.ToInt32(startofthemonth.DayOfWeek.ToString("d")) + 1;

            // Add blank controls for days before the first of the month
            for (int i = 1; i < dayoftheweek; i++)
            {
                UserControlBlank ucb = new UserControlBlank();
                daycontainers.Controls.Add(ucb);
            }

            // Add day controls for each day of the month
            for (int i = 1; i <= days; i++)
            {
                UserControlDays ucd = new UserControlDays(teacherID, i, month, year);
                ucd.days(i);
                daycontainers.Controls.Add(ucd);
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
            month++;

            if (month > 12)
            {
                month = 1;
                year++;
            }

            static_month = month;
            static_year = year;

            String monthname = DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
            lbdate.Text = monthname + " " + year;

            DisplayCalendar();
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
            month--;

            if (month < 1)
            {
                month = 12;
                year--;
            }

            static_month = month;
            static_year = year;

            String monthname = DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
            lbdate.Text = monthname + " " + year;

            DisplayCalendar();
        }
    }
}