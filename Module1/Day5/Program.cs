using System;
using Microsoft.Data.SqlClient;

namespace StudentConsoleApp
{
    class Program
    {

        private static readonly string _connString =
            "Server=5QYZ0J3; Database=StudentManagement; Integrated Security=true; TrustServerCertificate=true;";

        static void Main()
        {
            Console.WriteLine(" Student Management Console (ADO.NET)\n");

            while (true)
            {
                Console.WriteLine("Pick an option:");
                Console.WriteLine("1) Test DB Connection");
                Console.WriteLine("2) Courses - Create");
                Console.WriteLine("3) Courses - List");
                Console.WriteLine("4) Courses - Update Title/Credits");
                Console.WriteLine("5) Courses - Delete");
                Console.WriteLine("6) Students - Create");
                Console.WriteLine("7) Students - List");
                Console.WriteLine("0) Exit");
                Console.Write("Choice: ");
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": TestConnection(); break;
                        case "2": CreateCourse(); break;
                        case "3": ListCourses(); break;
                        case "4": UpdateCourse(); break;
                        case "5": DeleteCourse(); break;
                        case "6": CreateStudent(); break;
                        case "7": ListStudents(); break;
                        case "0": return;
                        default:
                            Console.WriteLine("Invalid choice.\n");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}\n");
                    Console.ResetColor();
                }
            }
        }

        static SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(_connString);
            conn.Open();
            return conn;
        }

        static void TestConnection()
        {
            using var conn = GetOpenConnection();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Connected to SQL Server successfully!\n");
            Console.ResetColor();
        }
        static void CreateCourse()
        {
            Console.Write("Course Code (e.g., CSE101): ");
            var code = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            Console.Write("Course Title: ");
            var title = (Console.ReadLine() ?? "").Trim();

            Console.Write("Credits (int): ");
            int credits = int.TryParse(Console.ReadLine(), out var c) ? c : 0;

            const string sql = @"
                INSERT INTO Courses (Code, Title, Credits)
                VALUES (@Code, @Title, @Credits);
            ";

            using var conn = GetOpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", code);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Credits", credits);

            var rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? " Course created.\n" : " Failed to create course.\n");
        }

        static void ListCourses()
        {
            const string sql = "SELECT Id, Code, Title, Credits FROM Courses ORDER BY Code;";

            using var conn = GetOpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\n-- Courses --");
            while (reader.Read())
            {
                var id = reader.GetGuid(0);
                var code = reader.GetString(1);
                var title = reader.GetString(2);
                var credits = reader.GetInt32(3);
                Console.WriteLine($"{id} | {code} - {title} ({credits} cr)");
            }
            Console.WriteLine();
        }

        static void UpdateCourse()
        {
            Console.Write("Enter Course Code to update (e.g., CSE101): ");
            var code = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            Console.Write("New Title (leave blank to keep): ");
            var newTitle = (Console.ReadLine() ?? "").Trim();

            Console.Write("New Credits (leave blank to keep): ");
            var creditsText = (Console.ReadLine() ?? "").Trim();

            string sql = "UPDATE Courses SET ";
            bool setAdded = false;

            if (!string.IsNullOrWhiteSpace(newTitle))
            {
                sql += "Title = @Title";
                setAdded = true;
            }

            if (int.TryParse(creditsText, out int newCredits))
            {
                if (setAdded) sql += ", ";
                sql += "Credits = @Credits";
                setAdded = true;
            }

            if (!setAdded)
            {
                Console.WriteLine("Nothing to update.\n");
                return;
            }

            sql += " WHERE Code = @Code;";

            using var conn = GetOpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", code);
            if (!string.IsNullOrWhiteSpace(newTitle)) cmd.Parameters.AddWithValue("@Title", newTitle);
            if (int.TryParse(creditsText, out newCredits)) cmd.Parameters.AddWithValue("@Credits", newCredits);

            var rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? " Course updated.\n" : " Course not found.\n");
        }

        static void DeleteCourse()
        {
            Console.Write("Enter Course Code to delete (e.g., CSE101): ");
            var code = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            const string sql = "DELETE FROM Courses WHERE Code = @Code;";

            using var conn = GetOpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", code);

            var rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? " Course deleted.\n" : " Course not found or already deleted.\n");
        }

      
        static void CreateStudent()
        {
            Console.Write("First Name: ");
            var first = (Console.ReadLine() ?? "").Trim();

            Console.Write("Last Name: ");
            var last = (Console.ReadLine() ?? "").Trim();

            Console.Write("Email: ");
            var email = (Console.ReadLine() ?? "").Trim();

            Console.Write("Date of Birth (YYYY-MM-DD): ");
            var dobText = (Console.ReadLine() ?? "").Trim();

            if (!DateTime.TryParse(dobText, out var dob))
            {
                Console.WriteLine("Invalid date.\n");
                return;
            }

            const string sql = @"
                INSERT INTO Students (FirstName, LastName, Email, DateOfBirth)
                VALUES (@FirstName, @LastName, @Email, @DOB);
            ";

            using var conn = GetOpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FirstName", first);
            cmd.Parameters.AddWithValue("@LastName", last);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@DOB", dob);

            var rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? " Student created.\n" : " Failed to create student.\n");
        }

        static void ListStudents()
        {
            const string sql = @"SELECT Id, FirstName, LastName, Email, DateOfBirth, EnrolledOn, IsActive 
                                 FROM Students ORDER BY LastName, FirstName;";

            using var conn = GetOpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\n-- Students --");
            while (reader.Read())
            {
                var id = reader.GetGuid(0);
                var first = reader.GetString(1);
                var last = reader.GetString(2);
                var email = reader.GetString(3);
                var dob = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                var enrolled = reader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm:ss");
                var active = reader.GetBoolean(6) ? "Active" : "Inactive";

                Console.WriteLine($"{id} | {first} {last} | {email} | DOB: {dob} | Enrolled: {enrolled} | {active}");
            }
            Console.WriteLine();
        }
    }
}