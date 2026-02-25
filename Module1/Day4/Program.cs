using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // only for Distinct/HashSet convenience, can remove if you don't want it
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StudentManagement.Domain;
using StudentManagement.Logging;
using StudentManagement.Repository;
using StudentManagement.Reporting;

class Program
{
    private const string DataFile   = "sms.json";
    private const string LogFile    = "log.txt";
    private const string ReportFile = "report.txt";

    // Global cap
    private const int MAX_ITEMS = 5;

    static async Task Main()
    {
        Console.WriteLine("=== Student Management (JSON + TXT Report) ===");

        IAppLogger logger = new FileLogger(LogFile);
        var store = new JsonDataStore(DataFile);

        // Safe-load (ensure your JsonDataStore.LoadAsync handles empty/corrupt files gracefully)
        var snap = await store.LoadAsync();

        // Seed initial records if empty
        if (snap.Students.Count == 0 && snap.Courses.Count == 0)
        {
            Console.WriteLine("Seeding initial data...");
            SeedInitialData(snap);
            await store.SaveAsync(snap);
        }

        // --- Multithreaded simulation ---
        Console.WriteLine("Running simulation with multiple users...");
        await RunSimulationAsync(snap, logger);
        Console.WriteLine("Simulation finished.");

        // CAP 1: Keep only last 5 log lines
        TrimFileToLastLines(LogFile, MAX_ITEMS);

        // CAP 2: Save only 5 enrollments (and only referenced students/courses) to JSON
        Console.WriteLine("Saving final snapshot to JSON...");
        var trimmedForSave = BuildTrimmedSnapshot(snap, MAX_ITEMS);
        await store.SaveAsync(trimmedForSave);

        // CAP 3: Export a report with only 5 enrollments (exact format requested)
        Console.WriteLine("Exporting report to text file...");
        ExportLimitedReport(trimmedForSave, ReportFile, MAX_ITEMS);

        // Print the same “Active Enrollments” block to console (max 5)
        Console.WriteLine();
        Console.WriteLine("STUDENT MANAGEMENT");
        Console.WriteLine();
        Console.WriteLine("-- Active Enrollments --");
        PrintActiveEnrollments(trimmedForSave, MAX_ITEMS);

        Console.WriteLine();
        Console.WriteLine("Data JSON : " + Path.GetFullPath(DataFile));
        Console.WriteLine("Report TXT: " + Path.GetFullPath(ReportFile));
        Console.WriteLine("Log TXT   : " + Path.GetFullPath(LogFile));
    }

    // ---------------------------------------------------------
    // INITIAL SEED DATA
    // ---------------------------------------------------------
    static void SeedInitialData(DataSnapshot snap)
    {
        snap.Students.Add(new Student("Meraj", "Alam", "meraj@gmail.com", new DateTime(2001, 5, 12)));
        snap.Students.Add(new Student("Rahul", "A", "rahul@gmail.com", new DateTime(1999, 10, 2)));

        snap.Courses.Add(new Course("CS101", "Intro to CS", 3));
        snap.Courses.Add(new Course("MA201", "Discrete Math", 4));
        snap.Courses.Add(new Course("CS201", "Data Structures", 4));
    }

    // ---------------------------------------------------------
    // MULTITHREADED SIMULATION (thread-safe on snapshot)
    // ---------------------------------------------------------
    static async Task RunSimulationAsync(DataSnapshot snap, IAppLogger logger)
    {
        var tasks = new List<Task>();
        var sync = new object(); // one lock to protect all snapshot lists

        int users = 5;
        int iterationsPerUser = 8;

        for (int u = 1; u <= users; u++)
        {
            int userId = u;
            tasks.Add(Task.Run(() =>
            {
                var rnd = new Random(Environment.TickCount + userId * 7919);

                for (int i = 0; i < iterationsPerUser; i++)
                {
                    try
                    {
                        // 1) Add a student
                        var student = new Student(
                            firstName: $"User{userId}",
                            lastName:  $"No{i}",
                            email:     $"user{userId}_{i}@mail.com",
                            dateOfBirth: RandomDOB(rnd)
                        );

                        lock (sync) { snap.Students.Add(student); }

                        // 2) Ensure at least one course exists & choose one
                        Course chosen;
                        lock (sync)
                        {
                            if (snap.Courses.Count == 0)
                                snap.Courses.Add(new Course("CS999", "Special Topics", 3));
                            chosen = snap.Courses[rnd.Next(snap.Courses.Count)];
                        }

                        // 3) Enroll the student
                        var enrollment = new Enrollment(student.Id, chosen.Id);
                        lock (sync) { snap.Enrollments.Add(enrollment); }

                        LogSafe(logger, $"User{userId}: Enrolled {student.FullName} in {chosen.Code}");

                        // 4) Random action: grade or drop or nothing
                        int action = rnd.Next(0, 3);
                        if (action == 1)
                        {
                            var grade = RandomGrade(rnd); // A/B/C/D/F (not NotGraded)
                            lock (sync) { enrollment.AssignGrade(grade); }
                            LogSafe(logger, $"User{userId}: Assigned {grade} to enrollment {enrollment.Id}");
                        }
                        else if (action == 2)
                        {
                            lock (sync) { enrollment.Drop(); }
                            LogSafe(logger, $"User{userId}: Dropped enrollment {enrollment.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Keep simulation running; log the error safely
                        LogSafe(logger, $"[Sim u{userId}] Error: {ex.Message}");
                    }

                    Thread.Sleep(rnd.Next(10, 40)); // small delay to interleave threads
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    static void LogSafe(IAppLogger logger, string message)
    {
        try { logger.Info(message); } catch { /* swallow to avoid IO contention crashes */ }
    }

    static DateTime RandomDOB(Random rnd)
        => new DateTime(1995, 1, 1).AddDays(rnd.Next(0, 3650));

    static Grade RandomGrade(Random rnd)
    {
        // Your enum: NotGraded=0, A=10, B=8, C=6, D=4, F=-1
        var grades = new[] { Grade.A, Grade.B, Grade.C, Grade.D, Grade.F };
        return grades[rnd.Next(grades.Length)];
    }

    // ---------------------------------------------------------
    // OUTPUT CAPS
    // ---------------------------------------------------------

    // Trim the log file to keep only the last N lines
    static void TrimFileToLastLines(string path, int maxLines)
    {
        try
        {
            if (!File.Exists(path)) return;

            var lines = File.ReadAllLines(path);
            if (lines.Length <= maxLines) return;

            var last = new string[Math.Min(maxLines, lines.Length)];
            Array.Copy(lines, lines.Length - last.Length, last, 0, last.Length);

            File.WriteAllLines(path, last, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch
        {
            // Best-effort; ignore trimming failures
        }
    }

    // Build a trimmed snapshot with at most N enrollments (most recent first),
    // and only the students/courses referenced by those enrollments.
    static DataSnapshot BuildTrimmedSnapshot(DataSnapshot snap, int maxEnrollments)
    {
        // 1) Copy and sort enrollments by EnrolledOn descending (bubble sort to avoid LINQ)
        var enrolls = new List<Enrollment>(snap.Enrollments);
        for (int i = 0; i < enrolls.Count - 1; i++)
        {
            for (int j = i + 1; j < enrolls.Count; j++)
            {
                if (enrolls[j].EnrolledOn > enrolls[i].EnrolledOn)
                {
                    var tmp = enrolls[i];
                    enrolls[i] = enrolls[j];
                    enrolls[j] = tmp;
                }
            }
        }

        // 2) Take up to N enrollments
        var limitedEnrolls = new List<Enrollment>();
        for (int i = 0; i < enrolls.Count && i < maxEnrollments; i++)
            limitedEnrolls.Add(enrolls[i]);

        // 3) Collect referenced student and course IDs
        var studentIds = new HashSet<Guid>();
        var courseIds  = new HashSet<Guid>();

        for (int i = 0; i < limitedEnrolls.Count; i++)
        {
            studentIds.Add(limitedEnrolls[i].StudentId);
            courseIds.Add(limitedEnrolls[i].CourseId);
        }

        // 4) Pick only referenced students/courses
        var limitedStudents = new List<Student>();
        for (int i = 0; i < snap.Students.Count; i++)
        {
            if (studentIds.Contains(snap.Students[i].Id))
                limitedStudents.Add(snap.Students[i]);
        }

        var limitedCourses = new List<Course>();
        for (int i = 0; i < snap.Courses.Count; i++)
        {
            if (courseIds.Contains(snap.Courses[i].Id))
                limitedCourses.Add(snap.Courses[i]);
        }

        return new DataSnapshot
        {
            Students    = limitedStudents,
            Courses     = limitedCourses,
            Enrollments = limitedEnrolls
        };
    }

    // Write a tiny report with at most N enrollments, in your exact format.
    static void ExportLimitedReport(DataSnapshot snap, string filePath, int maxCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("STUDENT MANAGEMENT");
        sb.AppendLine();
        sb.AppendLine("-- Active Enrollments --");

        // Prepare (Enrollment, Student, Course) list for active only
        var active = new List<(Enrollment Enr, Student? Stu, Course? Crs)>();
        for (int i = 0; i < snap.Enrollments.Count; i++)
        {
            var e = snap.Enrollments[i];
            if (!e.IsActive) continue;

            Student? stu = null;
            for (int s = 0; s < snap.Students.Count; s++)
                if (snap.Students[s].Id == e.StudentId) { stu = snap.Students[s]; break; }

            Course? crs = null;
            for (int c = 0; c < snap.Courses.Count; c++)
                if (snap.Courses[c].Id == e.CourseId) { crs = snap.Courses[c]; break; }

            active.Add((e, stu, crs));
        }

        // Sort by newest EnrolledOn
        for (int i = 0; i < active.Count - 1; i++)
        {
            for (int j = i + 1; j < active.Count; j++)
            {
                if (active[j].Enr.EnrolledOn > active[i].Enr.EnrolledOn)
                {
                    var tmp = active[i];
                    active[i] = active[j];
                    active[j] = tmp;
                }
            }
        }

        // Print at most maxCount
        int printed = 0;
        for (int i = 0; i < active.Count && printed < maxCount; i++)
        {
            var (e, stu, crs) = active[i];
            sb.AppendLine($"Enrollment: {e.Id}");
            sb.AppendLine($" Student: {(stu != null ? stu.FullName : "Unknown")}");
            sb.AppendLine($" Course : {(crs != null ? $"{crs.Code} - {crs.Title}" : "Unknown")}");
            sb.AppendLine($" Grade  : {e.Grade}");
            printed++;
        }

        // If fewer than N active, optionally include completed ones to fill up to N
        if (printed < maxCount)
        {
            var completed = new List<(Enrollment Enr, Student? Stu, Course? Crs)>();
            for (int i = 0; i < snap.Enrollments.Count; i++)
            {
                var e = snap.Enrollments[i];
                if (e.IsActive) continue;

                Student? stu = null;
                for (int s = 0; s < snap.Students.Count; s++)
                    if (snap.Students[s].Id == e.StudentId) { stu = snap.Students[s]; break; }

                Course? crs = null;
                for (int c = 0; c < snap.Courses.Count; c++)
                    if (snap.Courses[c].Id == e.CourseId) { crs = snap.Courses[c]; break; }

                completed.Add((e, stu, crs));
            }

            // Sort by newest EnrolledOn
            for (int i = 0; i < completed.Count - 1; i++)
            {
                for (int j = i + 1; j < completed.Count; j++)
                {
                    if (completed[j].Enr.EnrolledOn > completed[i].Enr.EnrolledOn)
                    {
                        var tmp = completed[i];
                        completed[i] = completed[j];
                        completed[j] = tmp;
                    }
                }
            }

            for (int i = 0; i < completed.Count && printed < maxCount; i++)
            {
                var (e, stu, crs) = completed[i];
                sb.AppendLine($"Enrollment: {e.Id}");
                sb.AppendLine($" Student: {(stu != null ? stu.FullName : "Unknown")}");
                sb.AppendLine($" Course : {(crs != null ? $"{crs.Code} - {crs.Title}" : "Unknown")}");
                sb.AppendLine($" Grade  : {e.Grade}");
                printed++;
            }
        }

        var full = Path.GetFullPath(filePath);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(full, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // ---------------------------------------------------------
    // CONSOLE SUMMARY (exact format, capped)
    // ---------------------------------------------------------
    static void PrintActiveEnrollments(DataSnapshot snap, int maxCount)
    {
        // Build list of active with joined student & course
        var active = new List<(Enrollment Enr, Student? Stu, Course? Crs)>();
        for (int i = 0; i < snap.Enrollments.Count; i++)
        {
            var e = snap.Enrollments[i];
            if (!e.IsActive) continue;

            Student? stu = null;
            for (int s = 0; s < snap.Students.Count; s++)
                if (snap.Students[s].Id == e.StudentId) { stu = snap.Students[s]; break; }

            Course? crs = null;
            for (int c = 0; c < snap.Courses.Count; c++)
                if (snap.Courses[c].Id == e.CourseId) { crs = snap.Courses[c]; break; }

            active.Add((e, stu, crs));
        }

        // Sort by newest
        for (int i = 0; i < active.Count - 1; i++)
        {
            for (int j = i + 1; j < active.Count; j++)
            {
                if (active[j].Enr.EnrolledOn > active[i].Enr.EnrolledOn)
                {
                    var tmp = active[i];
                    active[i] = active[j];
                    active[j] = tmp;
                }
            }
        }

        // Print up to maxCount
        int printed = 0;
        for (int i = 0; i < active.Count && printed < maxCount; i++)
        {
            var (e, stu, crs) = active[i];
            Console.WriteLine($"Enrollment: {e.Id}");
            Console.WriteLine($" Student: {(stu != null ? stu.FullName : "Unknown")}");
            Console.WriteLine($" Course : {(crs != null ? $"{crs.Code} - {crs.Title}" : "Unknown")}");
            Console.WriteLine($" Grade  : {e.Grade}");
            printed++;
        }

        if (active.Count > maxCount)
        {
            Console.WriteLine($"...and {active.Count - maxCount} more active enrollment(s).");
        }
    }
}