using System;
using System.Collections.Generic;
using System.IO;
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

    static async Task Main()
    {
        Console.WriteLine("=== Student Management ===");
        IAppLogger logger = new FileLogger(LogFile);

   
        var store = new JsonDataStore(DataFile);

    
        var snap = await store.LoadAsync();

        if (snap.Students.Count == 0 && snap.Courses.Count == 0)
        {
            Console.WriteLine("Seeding initial data...");
            SeedInitialData(snap);
            await store.SaveAsync(snap);
        }

        Console.WriteLine("Running simulation with multiple users...");
        await RunSimulationAsync(snap, logger);
        Console.WriteLine("Simulation finished.");

      
        Console.WriteLine("Saving final snapshot to JSON...");
        await store.SaveAsync(snap);

        
        Console.WriteLine("Exporting report to text file...");
        ReportExporter.ExportToFile(snap, ReportFile);

    
        Console.WriteLine();
        Console.WriteLine("STUDENT MANAGEMENT");
        Console.WriteLine();
        Console.WriteLine("-- Active Enrollments --");
        PrintActiveEnrollments(snap);

        Console.WriteLine();
        Console.WriteLine("Data JSON : " + Path.GetFullPath(DataFile));
        Console.WriteLine("Report TXT: " + Path.GetFullPath(ReportFile));
        Console.WriteLine("Log TXT   : " + Path.GetFullPath(LogFile));
    }

    static void SeedInitialData(DataSnapshot snap)
    {
        snap.Students.Add(new Student("Meraj", "Alam", "meraj@gmail.com", new DateTime(2001, 5, 12)));
        snap.Students.Add(new Student("Rahul", "A", "rahul@gmail.com", new DateTime(1999, 10, 2)));

        snap.Courses.Add(new Course("CS101", "Intro to CS", 3));
        snap.Courses.Add(new Course("MA201", "Discrete Math", 4));
        snap.Courses.Add(new Course("CS201", "Data Structures", 4));
    }

    static async Task RunSimulationAsync(DataSnapshot snap, IAppLogger logger)
    {
        var tasks = new List<Task>();
        var sync = new object(); 

        int users = 5;
        int iterationsPerUser = 8;

        for (int u = 1; u <= users; u++)
        {
            int userId = u;
            tasks.Add(Task.Run(() =>
            {
                var rnd = new Random(Environment.TickCount + userId * 5
                );

                for (int i = 0; i < iterationsPerUser; i++)
                {
                    try
                    {
                        
                        var student = new Student(
                            firstName: $"User{userId}",
                            lastName:  $"No{i}",
                            email:     $"user{userId}_{i}@mail.com",
                            dateOfBirth: RandomDOB(rnd)
                        );

                        lock (sync) { snap.Students.Add(student); }

                        
                        Course chosen;
                        lock (sync)
                        {
                            if (snap.Courses.Count == 0)
                                snap.Courses.Add(new Course("CS999", "Special Topics", 3));
                            chosen = snap.Courses[rnd.Next(snap.Courses.Count)];
                        }

                       
                        var enrollment = new Enrollment(student.Id, chosen.Id);
                        lock (sync) { snap.Enrollments.Add(enrollment); }

                        LogSafe(logger, $"User{userId}: Enrolled {student.FullName} in {chosen.Code}");

                       
                        int action = rnd.Next(0, 3);
                        if (action == 1)
                        {
                            var grade = RandomGrade(rnd); 
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
                        
                        LogSafe(logger, $"[Sim u{userId}] Error: {ex.Message}");
                    }

                    Thread.Sleep(rnd.Next(10, 40)); 
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    static void LogSafe(IAppLogger logger, string message)
    {
        try { logger.Info(message); } catch {  }
    }

    static DateTime RandomDOB(Random rnd)
        => new DateTime(1995, 1, 1).AddDays(rnd.Next(0, 3650));

    static Grade RandomGrade(Random rnd)
    {
     
        var grades = new[] { Grade.A, Grade.B, Grade.C, Grade.D, Grade.F };
        return grades[rnd.Next(grades.Length)];
    }


    static void PrintActiveEnrollments(DataSnapshot snap, int maxCount = 5)
{
 
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