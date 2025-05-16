using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics; // Add this for Debug.WriteLine
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeacherDashboard
{
    public partial class UserControlDays : UserControl
    {
        private int day, month, year;
        private string connString = "server=localhost;database=school_management;uid=root;pwd=;";
        private string teacherID;

        public UserControlDays(string id, int day, int month, int year)
        {
            try
            {
                Debug.WriteLine($"Constructor called with: id={id}, day={day}, month={month}, year={year}");
                InitializeComponent();
                teacherID = id;
                this.day = day;
                this.month = month;
                this.year = year;
                Debug.WriteLine("Constructor completed successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Constructor Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Constructor Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void UserControlDays_Load(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("UserControlDays_Load started");
                // Any initialization code can go here
                Debug.WriteLine("UserControlDays_Load completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Load Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void AddScheduleInfo(string subject, string room, string timeIn, string timeOut)
        {
            try
            {
                Debug.WriteLine($"AddScheduleInfo called with: subject={subject}, room={room}, timeIn={timeIn}, timeOut={timeOut}");
                Label lblInfo = new Label();
                lblInfo.Text = $"{subject}\nRoom {room}\n{timeIn} - {timeOut}";
                lblInfo.AutoSize = true;
                lblInfo.Font = new Font("Arial", 7); // Adjust para kasya
                lblInfo.Location = new Point(5, 25); // Adjust sa ilalim ng date
                this.Controls.Add(lblInfo);
                Debug.WriteLine("AddScheduleInfo completed successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"AddScheduleInfo Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"AddScheduleInfo Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void days(int numday)
        {
            try
            {
                Debug.WriteLine($"days method called with numday={numday}");
                day = numday; // Update the day for the calendar view
                lbdays.Text = day.ToString();

                // Check if this day has any schedules
                bool hasSchedules = CheckForSchedules();

                // Only display event details if there are schedules
                if (hasSchedules)
                {
                    Debug.WriteLine($"Day {day} has schedules, displaying events");
                    displayEvent(); // Display the event based on this day
                }
                else
                {
                    Debug.WriteLine($"Day {day} has no schedules, clearing lbevents");
                    lbevents.Text = ""; // Clear the events text if no schedules
                }

                Debug.WriteLine("days method completed successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"days Method Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"days Method Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private bool CheckForSchedules()
        {
            Debug.WriteLine($"CheckForSchedules called for date: {year}-{month}-{day}");
            bool hasSchedules = false;

            using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    DateTime currentDate = new DateTime(year, month, day);
                    string formattedDate = currentDate.ToString("yyyy-MM-dd");
                    string dayOfWeek = currentDate.DayOfWeek.ToString();

                    // Get the teacher's full name from id_no
                    string teacherName = GetTeacherName(conn, teacherID);

                    if (string.IsNullOrEmpty(teacherName))
                    {
                        Debug.WriteLine("CheckForSchedules: Teacher not found");
                        return false;
                    }

                    // First check for one-time events on this date
                    string oneTimeQuery = @"
                        SELECT COUNT(*)
                        FROM schedules
                        WHERE teacher LIKE @teacherName
                        AND date = @date
                        AND (is_recurring = 0 OR is_recurring IS NULL)";

                    using (MySqlCommand cmd = new MySqlCommand(oneTimeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@teacherName", "%" + teacherName + "%");
                        cmd.Parameters.AddWithValue("@date", formattedDate);

                        int oneTimeCount = Convert.ToInt32(cmd.ExecuteScalar());
                        Debug.WriteLine($"One-time events count: {oneTimeCount}");

                        if (oneTimeCount > 0)
                        {
                            hasSchedules = true;
                            Debug.WriteLine("One-time schedule found for this date");
                            return true; // Early return if we already found schedules
                        }
                    }

                    // Then check for recurring events on this day of week
                    string recurringQuery = @"
                        SELECT COUNT(*)
                        FROM schedules
                        WHERE teacher LIKE @teacherName
                        AND is_recurring = 1
                        AND start_date <= @date
                        AND end_date >= @date
                        AND (
                            FIND_IN_SET(@dayOfWeek, recurring_days) > 0
                            OR recurring_days LIKE CONCAT('%', @dayOfWeek, '%')
                            OR recurring_days = @dayOfWeek
                        )";

                    using (MySqlCommand cmd = new MySqlCommand(recurringQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@teacherName", "%" + teacherName + "%");
                        cmd.Parameters.AddWithValue("@date", formattedDate);
                        cmd.Parameters.AddWithValue("@dayOfWeek", dayOfWeek);

                        int recurringCount = Convert.ToInt32(cmd.ExecuteScalar());
                        Debug.WriteLine($"Recurring events count: {recurringCount}");

                        if (recurringCount > 0)
                        {
                            hasSchedules = true;
                            Debug.WriteLine("Recurring schedule found for this date");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CheckForSchedules Error: {ex.Message}\n{ex.StackTrace}");
                }
            }

            Debug.WriteLine($"CheckForSchedules result: {hasSchedules}");
            return hasSchedules;
        }

        public void displayEvent()
        {
            Debug.WriteLine("displayEvent started");
            lbevents.Text = "";

            using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
            {
                try
                {
                    Debug.WriteLine("Opening database connection");
                    conn.Open();
                    Debug.WriteLine($"Creating DateTime with year={year}, month={month}, day={day}");

                    // Add date validation to catch invalid date errors
                    if (year < 1900 || year > 2100 || month < 1 || month > 12 || day < 1 || day > 31)
                    {
                        string errorMsg = $"Invalid date values: year={year}, month={month}, day={day}";
                        Debug.WriteLine(errorMsg);
                        MessageBox.Show(errorMsg, "Date Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lbevents.Text = errorMsg;
                        return;
                    }

                    DateTime currentDate = new DateTime(year, month, day);
                    string formattedDate = currentDate.ToString("yyyy-MM-dd");
                    Debug.WriteLine($"Formatted date: {formattedDate}");
                    Debug.WriteLine($"Day of week: {currentDate.DayOfWeek}");

                    // Get the teacher's full name from id_no
                    Debug.WriteLine($"Getting teacher name for ID: {teacherID}");
                    string teacherName = GetTeacherName(conn, teacherID);
                    Debug.WriteLine($"Retrieved teacher name: {teacherName}");

                    if (string.IsNullOrEmpty(teacherName))
                    {
                        Debug.WriteLine("Teacher not found");
                        lbevents.Text = "Teacher not found";
                        return;
                    }

                    // Build the schedule display text
                    StringBuilder eventDetails = new StringBuilder();
                    eventDetails.AppendLine($"Schedule for {currentDate.ToString("MMM d, yyyy")} ({currentDate.DayOfWeek})");
                    eventDetails.AppendLine("---------------------------");

                    // Get one-time events (where date matches exactly)
                    Debug.WriteLine("Getting one-time events");
                    GetOneTimeEvents(conn, teacherName, formattedDate, eventDetails);

                    // Get recurring events that fall on this date
                    Debug.WriteLine("Getting recurring events");
                    GetRecurringEvents(conn, teacherName, currentDate, eventDetails);

                    // Check if any events were added
                    string events = eventDetails.ToString();
                    if (events.Split(new[] { "---------------------------" }, StringSplitOptions.None).Length <= 2)
                    {
                        eventDetails.AppendLine("No schedules for this date.");
                    }

                    // Update the label with all events
                    Debug.WriteLine($"Setting lbevents.Text to: {eventDetails.ToString()}");
                    lbevents.Text = eventDetails.ToString();
                    Debug.WriteLine("displayEvent completed successfully");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"displayEvent Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"displayEvent Error: {ex.Message}\n{ex.StackTrace}");
                    lbevents.Text = "Error: " + ex.Message;
                }
            }
        }

        private string GetTeacherName(MySql.Data.MySqlClient.MySqlConnection conn, string teacherID)
        {
            try
            {
                Debug.WriteLine($"GetTeacherName called with teacherID={teacherID}");
                string nameQuery = "SELECT first_name, last_name FROM faculty WHERE id_no = @id";
                string teacherName = "";

                using (MySqlCommand nameCmd = new MySqlCommand(nameQuery, conn))
                {
                    nameCmd.Parameters.AddWithValue("@id", teacherID);
                    Debug.WriteLine("Executing reader to get teacher name");
                    using (MySqlDataReader reader = nameCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Debug.WriteLine("Reader has data for teacher name");
                            // Add explicit type casting to see if that's the issue
                            Debug.WriteLine($"first_name type: {reader["first_name"].GetType().Name}");
                            Debug.WriteLine($"last_name type: {reader["last_name"].GetType().Name}");

                            string firstName = Convert.ToString(reader["first_name"]);
                            string lastName = Convert.ToString(reader["last_name"]);
                            teacherName = $"{firstName} {lastName}";
                            Debug.WriteLine($"Teacher name constructed: {teacherName}");
                        }
                        else
                        {
                            Debug.WriteLine("No teacher found with the given ID");
                        }
                    }
                }
                return teacherName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetTeacherName Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"GetTeacherName Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        private void GetOneTimeEvents(MySql.Data.MySqlClient.MySqlConnection conn, string teacherName, string formattedDate, StringBuilder eventDetails)
        {
            try
            {
                Debug.WriteLine($"GetOneTimeEvents called with teacherName={teacherName}, formattedDate={formattedDate}");
                string query = @"
                    SELECT subject, room, time_in, time_out
                    FROM schedules
                    WHERE teacher LIKE @teacherName
                    AND date = @date
                    AND (is_recurring = 0 OR is_recurring IS NULL)
                    ORDER BY time_in";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherName", "%" + teacherName + "%");
                    cmd.Parameters.AddWithValue("@date", formattedDate);
                    Debug.WriteLine("Executing reader for one-time events");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Debug.WriteLine($"Found one-time event #{count}");
                            AddEventToDetails(reader, eventDetails);
                        }
                        Debug.WriteLine($"Total one-time events found: {count}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetOneTimeEvents Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"GetOneTimeEvents Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetRecurringEvents(MySql.Data.MySqlClient.MySqlConnection conn, string teacherName, DateTime currentDate, StringBuilder eventDetails)
        {
            try
            {
                Debug.WriteLine($"GetRecurringEvents called with teacherName={teacherName}, currentDate={currentDate}");
                // Get day of week
                string dayOfWeek = currentDate.DayOfWeek.ToString();
                Debug.WriteLine($"Day of week: {dayOfWeek}");

                // First, log the SQL query with actual values for debugging
                // The potential issue could be in the way recurring days are stored
                // Let's check multiple formats (comma-separated list, plain string, etc.)
                string query = @"
                    SELECT subject, room, time_in, time_out, recurring_days
                    FROM schedules
                    WHERE teacher LIKE @teacherName
                    AND is_recurring = 1
                    AND start_date <= @date
                    AND end_date >= @date
                    AND (
                        FIND_IN_SET(@dayOfWeek, recurring_days) > 0
                        OR recurring_days LIKE CONCAT('%', @dayOfWeek, '%')
                        OR recurring_days = @dayOfWeek
                        OR (recurring_days IS NULL AND @dayOfWeek IS NOT NULL)
                    )
                    ORDER BY time_in";

                string dateParam = currentDate.ToString("yyyy-MM-dd");
                string teacherParam = "%" + teacherName + "%";
                Debug.WriteLine($"SQL Query: {query.Replace("@teacherName", $"'{teacherParam}'").Replace("@date", $"'{dateParam}'").Replace("@dayOfWeek", $"'{dayOfWeek}'")}");

                // Execute the query to check for recurring events
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherName", teacherParam);
                    cmd.Parameters.AddWithValue("@date", dateParam);
                    cmd.Parameters.AddWithValue("@dayOfWeek", dayOfWeek);
                    Debug.WriteLine("Executing reader for recurring events");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            string recurring_days = Convert.ToString(reader["recurring_days"]);
                            Debug.WriteLine($"Found recurring event #{count}, recurring_days={recurring_days}");

                            // Check if this event really matches the day of week
                            if (recurring_days.Contains(dayOfWeek))
                            {
                                Debug.WriteLine($"Day {dayOfWeek} found in recurring_days '{recurring_days}', adding event");
                                AddEventToDetails(reader, eventDetails);
                            }
                            else
                            {
                                Debug.WriteLine($"WARNING: Day {dayOfWeek} NOT found in recurring_days '{recurring_days}', SQL FIND_IN_SET may be incorrect");
                            }
                        }
                        Debug.WriteLine($"Total recurring events found: {count}");

                        // If no recurring events found, let's do a broader query to see what recurring events exist
                        if (count == 0)
                        {
                            Debug.WriteLine("No recurring events found. Checking what recurring events exist at all...");
                        }
                    }

                    // For debugging: If no events were found, check all recurring events for this teacher
                    string debugQuery = @"
                        SELECT id, subject, start_date, end_date, recurring_days
                        FROM schedules
                        WHERE teacher LIKE @teacherName
                        AND is_recurring = 1";

                    using (MySqlCommand debugCmd = new MySqlCommand(debugQuery, conn))
                    {
                        debugCmd.Parameters.AddWithValue("@teacherName", teacherParam);
                        Debug.WriteLine("Executing debug query for all recurring events");

                        using (MySqlDataReader reader = debugCmd.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                string id = Convert.ToString(reader["id"]);
                                string subject = Convert.ToString(reader["subject"]);
                                string startDate = reader["start_date"].ToString();
                                string endDate = reader["end_date"].ToString();
                                string recurringDays = Convert.ToString(reader["recurring_days"]);

                                Debug.WriteLine($"Debug: Recurring event exists - ID: {id}, Subject: {subject}, " +
                                               $"Start: {startDate}, End: {endDate}, Days: {recurringDays}");

                                // Check why this event might not be included
                                DateTime start = DateTime.Parse(startDate);
                                DateTime end = DateTime.Parse(endDate);

                                if (currentDate < start)
                                    Debug.WriteLine($"  - Event not included because current date {currentDate.ToShortDateString()} is before start date {start.ToShortDateString()}");
                                else if (currentDate > end)
                                    Debug.WriteLine($"  - Event not included because current date {currentDate.ToShortDateString()} is after end date {end.ToShortDateString()}");
                                else if (!recurringDays.Contains(dayOfWeek))
                                    Debug.WriteLine($"  - Event not included because day of week '{dayOfWeek}' not in recurring days '{recurringDays}'");
                                else
                                    Debug.WriteLine($"  - Event SHOULD be included, investigate further");
                            }
                            Debug.WriteLine($"Total recurring events for this teacher: {count}");

                            if (count == 0)
                            {
                                Debug.WriteLine("No recurring events found for this teacher at all.");
                                eventDetails.AppendLine("No recurring classes scheduled.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetRecurringEvents Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"GetRecurringEvents Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddEventToDetails(MySqlDataReader reader, StringBuilder eventDetails)
        {
            try
            {
                Debug.WriteLine("AddEventToDetails called");
                // Log the actual types and values before conversion
                Debug.WriteLine($"subject type: {reader["subject"].GetType().Name}, value: {reader["subject"]}");
                Debug.WriteLine($"time_in type: {reader["time_in"].GetType().Name}, value: {reader["time_in"]}");
                Debug.WriteLine($"time_out type: {reader["time_out"].GetType().Name}, value: {reader["time_out"]}");
                Debug.WriteLine($"room type: {reader["room"].GetType().Name}, value: {reader["room"]}");

                // Use explicit conversion to avoid casting issues
                string subject = Convert.ToString(reader["subject"]).Trim();

                // Add special handling for DateTime values
                DateTime timeInDt;
                DateTime timeOutDt;

                if (DateTime.TryParse(reader["time_in"].ToString(), out timeInDt) &&
                    DateTime.TryParse(reader["time_out"].ToString(), out timeOutDt))
                {
                    string timeIn = timeInDt.ToString("HH:mm");
                    string timeOut = timeOutDt.ToString("HH:mm");
                    string room = Convert.ToString(reader["room"]).Trim();

                    // Add the event details
                    eventDetails.AppendLine($"{subject}");
                    eventDetails.AppendLine($"{timeIn} - {timeOut}");
                    eventDetails.AppendLine($"Room: {room}");
                    eventDetails.AppendLine("---------------------------");

                    Debug.WriteLine($"Added event: {subject}, {timeIn}-{timeOut}, Room: {room}");
                }
                else
                {
                    Debug.WriteLine("Failed to parse time_in or time_out as DateTime");
                    eventDetails.AppendLine("Error parsing event times");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddEventToDetails Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"AddEventToDetails Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}