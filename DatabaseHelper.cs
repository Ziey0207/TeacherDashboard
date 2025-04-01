using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherDashboard
{
    public class DatabaseHelper : IDisposable
    {
        // Database connection object
        private MySqlConnection _connection;

        // Flag to track disposal status
        private bool _disposed = false;

        /// <summary>
        /// Gets the connection string from app.config
        /// Configure this in your app.config under connectionStrings section
        /// Example:
        /// <connectionStrings>
        ///   <add name="SchoolManagementDB"
        ///        connectionString="Server=localhost;Database=school_management;Uid=root;Pwd=;"
        ///        providerName="MySql.Data.MySqlClient"/>
        /// </connectionStrings>
        /// </summary>
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["SchoolManagementDB"].ConnectionString;

        /// <summary>
        /// Initializes a new instance of the DatabaseHelper class
        /// </summary>
        public DatabaseHelper()
        {
            // Create new MySQL connection using the connection string
            _connection = new MySqlConnection(ConnectionString);
        }

        #region Connection Management

        /// <summary>
        /// Opens the database connection if it's not already open
        /// </summary>
        public void OpenConnection()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// Closes the database connection if it's not already closed
        /// </summary>
        public void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        #endregion Connection Management

        #region CRUD Operations (Core Database Methods)

        /// <summary>
        /// Executes a SELECT query and returns the results as a DataTable
        /// </summary>
        /// <param name="query">SQL query string</param>
        /// <param name="parameters">Optional parameters for parameterized queries</param>
        /// <returns>DataTable containing query results</returns>
        public DataTable ExecuteQuery(string query, params MySqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();

            try
            {
                OpenConnection();
                using (MySqlCommand command = new MySqlCommand(query, _connection))
                {
                    // Add parameters if provided
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    // Use DataAdapter to fill DataTable
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            finally
            {
                // Ensure connection is closed even if an exception occurs
                CloseConnection();
            }

            return dataTable;
        }

        /// <summary>
        /// Executes INSERT, UPDATE, DELETE queries and returns number of affected rows
        /// </summary>
        /// <param name="query">SQL query string</param>
        /// <param name="parameters">Optional parameters for parameterized queries</param>
        /// <returns>Number of rows affected by the operation</returns>
        public int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            int rowsAffected = 0;

            try
            {
                OpenConnection();
                using (MySqlCommand command = new MySqlCommand(query, _connection))
                {
                    // Add parameters if provided
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    // Execute the command
                    rowsAffected = command.ExecuteNonQuery();
                }
            }
            finally
            {
                // Ensure connection is closed even if an exception occurs
                CloseConnection();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Executes a query that returns a single value (first column of first row)
        /// </summary>
        /// <param name="query">SQL query string</param>
        /// <param name="parameters">Optional parameters for parameterized queries</param>
        /// <returns>Object containing the result (may need casting)</returns>
        public object ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            object result = null;

            try
            {
                OpenConnection();
                using (MySqlCommand command = new MySqlCommand(query, _connection))
                {
                    // Add parameters if provided
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    // Execute and get single value
                    result = command.ExecuteScalar();
                }
            }
            finally
            {
                // Ensure connection is closed even if an exception occurs
                CloseConnection();
            }

            return result;
        }

        #endregion CRUD Operations (Core Database Methods)

        #region Admin Operations

        /// <summary>
        /// Validates admin login credentials
        /// </summary>
        /// <param name="username">Admin username</param>
        /// <param name="password">Admin password</param>
        /// <returns>True if credentials are valid, false otherwise</returns>
        public bool ValidateAdminLogin(string username, string password)
        {
            // Note: In production, you should hash passwords and compare hashes
            string query = "SELECT COUNT(*) FROM admins WHERE username = @username AND password = @password";
            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@username", username),
            new MySqlParameter("@password", password)
            };

            int count = Convert.ToInt32(ExecuteScalar(query, parameters));
            return count > 0;
        }

        /// <summary>
        /// Gets all admin records (excluding passwords for security)
        /// </summary>
        /// <returns>DataTable containing admin records</returns>
        public DataTable GetAllAdmins()
        {
            // Never select passwords in general queries
            string query = "SELECT id, username, email, contact_number FROM admins";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Adds a new admin to the database
        /// </summary>
        /// <param name="username">Admin username</param>
        /// <param name="email">Admin email</param>
        /// <param name="contactNumber">Admin contact number</param>
        /// <param name="password">Admin password (should be hashed before calling this method)</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int AddAdmin(string username, string email, string contactNumber, string password)
        {
            string query = @"INSERT INTO admins (username, email, contact_number, password)
                         VALUES (@username, @email, @contactNumber, @password)";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@username", username),
            new MySqlParameter("@email", email),
            new MySqlParameter("@contactNumber", contactNumber),
            new MySqlParameter("@password", password)
            };

            return ExecuteNonQuery(query, parameters);
        }

        #endregion Admin Operations

        #region Faculty Operations

        /// <summary>
        /// Creates a new faculty session record
        /// </summary>
        public int CreateFacultySession(int facultyId, DateTime loginTime, string sessionToken, bool rememberMe)
        {
            string query = @"INSERT INTO faculty_sessions
                    (faculty_id, login_time, session_token, remember_me)
                    VALUES (@facultyId, @loginTime, @sessionToken, @rememberMe)";

            var parameters = new MySqlParameter[]
            {
        new MySqlParameter("@facultyId", facultyId),
        new MySqlParameter("@loginTime", loginTime),
        new MySqlParameter("@sessionToken", sessionToken),
        new MySqlParameter("@rememberMe", rememberMe ? 1 : 0)
            };

            return ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Updates a faculty session with logout information
        /// </summary>
        public int UpdateFacultySessionLogout(int sessionId, DateTime logoutTime, string logoutType = "system")
        {
            string query = @"UPDATE faculty_sessions SET
                    logout_time = @logoutTime,
                    logout_type = @logoutType
                    WHERE id = @sessionId";

            var parameters = new MySqlParameter[]
            {
        new MySqlParameter("@sessionId", sessionId),
        new MySqlParameter("@logoutTime", logoutTime),
        new MySqlParameter("@logoutType", logoutType)
            };

            return ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Gets active faculty session by token
        /// </summary>
        public DataTable GetFacultySessionByToken(string sessionToken)
        {
            string query = @"SELECT * FROM faculty_sessions
                    WHERE session_token = @sessionToken
                    AND logout_time IS NULL";

            var parameter = new MySqlParameter("@sessionToken", sessionToken);
            return ExecuteQuery(query, parameter);
        }

        /// <summary>
        /// Gets all active faculty members
        /// </summary>
        /// <returns>DataTable containing active faculty records</returns>
        public DataTable GetAllFaculty()
        {
            string query = "SELECT * FROM faculty WHERE is_active = 1";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Gets a faculty member by their ID number
        /// </summary>
        /// <param name="idNo">Faculty ID number</param>
        /// <returns>DataTable with the faculty record (empty if not found)</returns>
        public DataTable GetFacultyById(string idNo)
        {
            string query = "SELECT * FROM faculty WHERE id_no = @idNo";
            var parameter = new MySqlParameter("@idNo", idNo);
            return ExecuteQuery(query, parameter);
        }

        /// <summary>
        /// Adds a new faculty member to the database
        /// </summary>
        /// <param name="idNo">Faculty ID number</param>
        /// <param name="lastName">Last name</param>
        /// <param name="firstName">First name</param>
        /// <param name="middleName">Middle name (optional)</param>
        /// <param name="email">Email address</param>
        /// <param name="contactNumber">Contact number</param>
        /// <param name="gender">Gender (Male/Female/Other)</param>
        /// <param name="address">Physical address</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int AddFaculty(string idNo, string lastName, string firstName, string middleName,
                             string email, string contactNumber, string gender, string address)
        {
            string query = @"INSERT INTO faculty
                        (id_no, last_name, first_name, middle_name, email, contact_number, gender, address, is_active)
                        VALUES (@idNo, @lastName, @firstName, @middleName, @email, @contactNumber, @gender, @address, 1)";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@idNo", idNo),
            new MySqlParameter("@lastName", lastName),
            new MySqlParameter("@firstName", firstName),
            new MySqlParameter("@middleName", middleName ?? (object)DBNull.Value), // Handle null middle name
            new MySqlParameter("@email", email),
            new MySqlParameter("@contactNumber", contactNumber),
            new MySqlParameter("@gender", gender),
            new MySqlParameter("@address", address)
            };

            return ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Updates an existing faculty member's information
        /// </summary>
        /// <param name="id">Faculty database ID</param>
        /// <param name="idNo">Faculty ID number</param>
        /// <param name="lastName">Last name</param>
        /// <param name="firstName">First name</param>
        /// <param name="middleName">Middle name (optional)</param>
        /// <param name="email">Email address</param>
        /// <param name="contactNumber">Contact number</param>
        /// <param name="gender">Gender (Male/Female/Other)</param>
        /// <param name="address">Physical address</param>
        /// <param name="isActive">Active status flag</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int UpdateFaculty(int id, string idNo, string lastName, string firstName, string middleName,
                                string email, string contactNumber, string gender, string address, bool isActive)
        {
            string query = @"UPDATE faculty SET
                        id_no = @idNo,
                        last_name = @lastName,
                        first_name = @firstName,
                        middle_name = @middleName,
                        email = @email,
                        contact_number = @contactNumber,
                        gender = @gender,
                        address = @address,
                        is_active = @isActive
                        WHERE id = @id";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@id", id),
            new MySqlParameter("@idNo", idNo),
            new MySqlParameter("@lastName", lastName),
            new MySqlParameter("@firstName", firstName),
            new MySqlParameter("@middleName", middleName ?? (object)DBNull.Value), // Handle null middle name
            new MySqlParameter("@email", email),
            new MySqlParameter("@contactNumber", contactNumber),
            new MySqlParameter("@gender", gender),
            new MySqlParameter("@address", address),
            new MySqlParameter("@isActive", isActive ? 1 : 0) // Convert bool to tinyint
            };

            return ExecuteNonQuery(query, parameters);
        }

        #endregion Faculty Operations

        #region Course Operations

        /// <summary>
        /// Gets all courses from the database
        /// </summary>
        /// <returns>DataTable containing all course records</returns>
        public DataTable GetAllCourses()
        {
            string query = "SELECT * FROM courses";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Gets a specific course by its course code
        /// </summary>
        /// <param name="courseCode">Course code to search for</param>
        /// <returns>DataTable with the course record (empty if not found)</returns>
        public DataTable GetCourseByCode(string courseCode)
        {
            string query = "SELECT * FROM courses WHERE course_code = @courseCode";
            var parameter = new MySqlParameter("@courseCode", courseCode);
            return ExecuteQuery(query, parameter);
        }

        /// <summary>
        /// Adds a new course to the database
        /// </summary>
        /// <param name="courseCode">Course code</param>
        /// <param name="courseName">Course name</param>
        /// <param name="description">Course description (optional)</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int AddCourse(string courseCode, string courseName, string description)
        {
            string query = @"INSERT INTO courses (course_code, course_name, description)
                         VALUES (@courseCode, @courseName, @description)";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@courseCode", courseCode),
            new MySqlParameter("@courseName", courseName),
            new MySqlParameter("@description", description ?? (object)DBNull.Value) // Handle null description
            };

            return ExecuteNonQuery(query, parameters);
        }

        #endregion Course Operations

        #region Subject Operations

        /// <summary>
        /// Gets all subjects from the database
        /// </summary>
        /// <returns>DataTable containing all subject records</returns>
        public DataTable GetAllSubjects()
        {
            string query = "SELECT * FROM subjects";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Gets a specific subject by its subject code
        /// </summary>
        /// <param name="subjectCode">Subject code to search for</param>
        /// <returns>DataTable with the subject record (empty if not found)</returns>
        public DataTable GetSubjectByCode(string subjectCode)
        {
            string query = "SELECT * FROM subjects WHERE subject_code = @subjectCode";
            var parameter = new MySqlParameter("@subjectCode", subjectCode);
            return ExecuteQuery(query, parameter);
        }

        #endregion Subject Operations

        #region Schedule Operations

        /// <summary>
        /// Gets all schedules from the database
        /// </summary>
        /// <returns>DataTable containing all schedule records</returns>
        public DataTable GetAllSchedules()
        {
            string query = "SELECT * FROM schedules";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Adds a new schedule to the database
        /// </summary>
        /// <param name="subject">Subject name</param>
        /// <param name="teacher">Teacher name</param>
        /// <param name="date">Schedule date</param>
        /// <param name="time">Schedule time</param>
        /// <param name="room">Room number/location</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int AddSchedule(string subject, string teacher, DateTime date, string time, string room)
        {
            string query = @"INSERT INTO schedules (subject, teacher, date, time, room)
                         VALUES (@subject, @teacher, @date, @time, @room)";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@subject", subject),
            new MySqlParameter("@teacher", teacher),
            new MySqlParameter("@date", date),
            new MySqlParameter("@time", time),
            new MySqlParameter("@room", room)
            };

            return ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Gets schedules within a specific date range
        /// </summary>
        /// <param name="startDate">Start date of range</param>
        /// <param name="endDate">End date of range</param>
        /// <returns>DataTable with schedules between the specified dates</returns>
        public DataTable GetSchedulesByDateRange(DateTime startDate, DateTime endDate)
        {
            string query = "SELECT * FROM schedules WHERE date BETWEEN @startDate AND @endDate ORDER BY date, time";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@startDate", startDate),
            new MySqlParameter("@endDate", endDate)
            };

            return ExecuteQuery(query, parameters);
        }

        #endregion Schedule Operations

        #region Calendar Operations

        /// <summary>
        /// Gets all calendar events (excluding invalid dates)
        /// </summary>
        /// <returns>DataTable containing calendar events</returns>
        public DataTable GetCalendarEvents()
        {
            // Filter out invalid dates (0000-00-00)
            string query = "SELECT * FROM tbl_calendar WHERE date > '0000-00-00'";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Adds a new calendar event
        /// </summary>
        /// <param name="subject">Event subject</param>
        /// <param name="teacher">Teacher name</param>
        /// <param name="date">Event date</param>
        /// <param name="time">Event time (optional)</param>
        /// <param name="room">Room number/location (optional)</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int AddCalendarEvent(string subject, string teacher, DateTime date, string time, string room)
        {
            string query = @"INSERT INTO tbl_calendar (subject, teacher, date, time, room)
                         VALUES (@subject, @teacher, @date, @time, @room)";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@subject", subject),
            new MySqlParameter("@teacher", teacher),
            new MySqlParameter("@date", date),
            new MySqlParameter("@time", time ?? (object)DBNull.Value), // Handle null time
            new MySqlParameter("@room", room ?? (object)DBNull.Value)  // Handle null room
            };

            return ExecuteNonQuery(query, parameters);
        }

        #endregion Calendar Operations

        #region User Session Operations

        /// <summary>
        /// Creates a new user session record
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="loginTime">Login timestamp</param>
        /// <param name="sessionToken">Session token/identifier</param>
        /// <param name="rememberMe">Remember me flag</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int CreateUserSession(int userId, DateTime loginTime, string sessionToken, bool rememberMe)
        {
            string query = @"INSERT INTO user_sessions
                        (user_id, login_time, session_token, remember_me)
                        VALUES (@userId, @loginTime, @sessionToken, @rememberMe)";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@userId", userId),
            new MySqlParameter("@loginTime", loginTime),
            new MySqlParameter("@sessionToken", sessionToken),
            new MySqlParameter("@rememberMe", rememberMe ? 1 : 0) // Convert bool to tinyint
            };

            return ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Updates a user session with logout information
        /// </summary>
        /// <param name="sessionId">Session ID to update</param>
        /// <param name="logoutTime">Logout timestamp</param>
        /// <param name="logoutType">Type of logout (system/user)</param>
        /// <returns>Number of rows affected (1 if successful)</returns>
        public int UpdateUserSessionLogout(int sessionId, DateTime logoutTime, string logoutType = "system")
        {
            string query = @"UPDATE user_sessions SET
                        logout_time = @logoutTime,
                        logout_type = @logoutType
                        WHERE id = @sessionId";

            var parameters = new MySqlParameter[]
            {
            new MySqlParameter("@sessionId", sessionId),
            new MySqlParameter("@logoutTime", logoutTime),
            new MySqlParameter("@logoutType", logoutType)
            };

            return ExecuteNonQuery(query, parameters);
        }

        #endregion User Session Operations

        #region IDisposable Implementation (Resource Cleanup)

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources (connection)
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                // No unmanaged resources to clean up in this class
                _disposed = true;
            }
        }

        /// <summary>
        /// Public implementation of Dispose pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer (safety net in case Dispose wasn't called)
        /// </summary>
        ~DatabaseHelper()
        {
            Dispose(false);
        }

        #endregion IDisposable Implementation (Resource Cleanup)
    }
}