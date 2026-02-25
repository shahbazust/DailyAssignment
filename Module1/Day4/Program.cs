
using System;
using System.Collections.Generic;
using StudentManagement.Domain;
using StudentManagement.Exceptions;
using StudentManagement.Logging;
class Program
{

    static List<Student> Students = new List<Student>();
    static List<Course> Courses = new List<Course>();
    static List<Enrollment> Enrollments = new List<Enrollment>();

    static void Main()
    {
        using var logger = new FileLogger(@"C:\Users\307424\Documents\TrumioUdemy\DailyAssignment\Module1\Day3\Logging"); // writes to

        Console.WriteLine("=== Student Management ===");

        var s1 = new Student("Meraj", "Alam", "meraj@gmail.com", new DateTime(2001, 5, 12));
        var s2 = new Student("Rahul", "A", "rahul@gmail.com", new DateTime(1999, 10, 2));
        Students.Add(s1);
        Students.Add(s2);

        var c1 = new Course("CS101", "Intro to CS", 3);
        var c2 = new Course("MA201", "Discrete Math", 4);
        Courses.Add(c1);
        Courses.Add(c2);

        
        try
        {
            var e1 = EnrollStudent(s1.Id, c1.Id, logger);
            Console.WriteLine("Enrolled: " + e1.Id);

            var e2 = EnrollStudent(s2.Id, c1.Id, logger);
            Console.WriteLine("Enrolled: " + e2.Id);

            // Try duplicate 
            try
            {
                EnrollStudent(s1.Id, c1.Id, logger);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Expected: " + ex.Message);
            }

            // Assign grade
            AssignGrade(e1.Id, Grade.A, logger);

            // Enroll and then drop
            var e3 = EnrollStudent(s1.Id, c2.Id, logger);
            DropEnrollment(e3.Id, logger);
        }
        catch (Exception ex)
        {
            logger.Error("Unexpected error", ex);
            Console.WriteLine("Error: " + ex.Message);
        }

        //  Show active enrollments
        Console.WriteLine();
        Console.WriteLine("-- Active Enrollments --");
        PrintActiveEnrollments();

        Console.WriteLine();
    }

    static Enrollment EnrollStudent(Guid studentId, Guid courseId, IAppLogger logger)
    {
        // Find student
        Student foundStudent = null;
        for (int i = 0; i < Students.Count; i++)
        {
            if (Students[i].Id == studentId)
            {
                foundStudent = Students[i];
                break;
            }
        }
        if (foundStudent == null)
        {
            throw new NotFoundException("Student not found");
        }

        // Find course
        Course foundCourse = null;
        for (int i = 0; i < Courses.Count; i++)
        {
            if (Courses[i].Id == courseId)
            {
                foundCourse = Courses[i];
                break;
            }
        }
        if (foundCourse == null)
        {
            throw new NotFoundException("Course not found");
        }

        // Check already enrolled (active)
        for (int i = 0; i < Enrollments.Count; i++)
        {
            var e = Enrollments[i];
            if (e.StudentId == studentId && e.CourseId == courseId && e.IsActive)
            {
                throw new DuplicateEntityException("Already enrolled");
            }
        }

        // Create and add
        var newEnrollment = new Enrollment(studentId, courseId);
        Enrollments.Add(newEnrollment);

        logger.Info("Enrolled " + foundStudent.FullName + " in " + foundCourse.Code);
        return newEnrollment;
    }

    static void AssignGrade(Guid enrollmentId, Grade grade, IAppLogger logger)
    {
        Enrollment found = null;

        for (int i = 0; i < Enrollments.Count; i++)
        {
            if (Enrollments[i].Id == enrollmentId)
            {
                found = Enrollments[i];
                break;
            }
        }

        if (found == null)
        {
            throw new NotFoundException("Enrollment not found");
        }

        found.AssignGrade(grade);
        logger.Info("Grade assigned: " + grade);
    }

    static void DropEnrollment(Guid enrollmentId, IAppLogger logger)
    {
        Enrollment found = null;

        for (int i = 0; i < Enrollments.Count; i++)
        {
            if (Enrollments[i].Id == enrollmentId)
            {
                found = Enrollments[i];
                break;
            }
        }

        if (found == null)
        {
            throw new NotFoundException("Enrollment not found");
        }

        found.Drop();
        logger.Info("Enrollment dropped");
    }

    static void PrintActiveEnrollments()
    {
        for (int i = 0; i < Enrollments.Count; i++)
        {
            var e = Enrollments[i];
            if (e.IsActive)
            {
                // Find student
                Student stu = null;
                for (int s = 0; s < Students.Count; s++)
                {
                    if (Students[s].Id == e.StudentId)
                    {
                        stu = Students[s];
                        break;
                    }
                }

                // Find course
                Course crs = null;
                for (int c = 0; c < Courses.Count; c++)
                {
                    if (Courses[c].Id == e.CourseId)
                    {
                        crs = Courses[c];
                        break;
                    }
                }

                Console.WriteLine("Enrollment: " + e.Id);
                Console.WriteLine(" Student : " + (stu != null ? stu.FullName : "Unknown"));
                Console.WriteLine(" Course  : " + (crs != null ? crs.Code + " - " + crs.Title : "Unknown"));
                Console.WriteLine(" Grade   : " + e.Grade);
                Console.WriteLine();
            }
        }
    }
}