using StudentManagement.Domain;
using StudentManagement.Logging;
using StudentManagement.Exceptions;
using System;
using System.Collections.Generic;

class Program
{
    static List<Student> Students = new List<Student>();
    static List<Course> Courses = new List<Course>();
    static List<Enrollment> Enrollments = new List<Enrollment>();

    static void Main()
    {
        IAppLogger logger = new FileLogger();

        Console.WriteLine("STUDENT MANAGEMENT ");

        // Seed data
        var s1 = new Student("Meraj", "Alam", "meraj@gmail.com", new DateTime(2001, 5, 12));
        var s2 = new Student("Rahul", "A", "rahul@gmail.com", new DateTime(1999, 10, 2));
        Students.Add(s1);
        Students.Add(s2);

        var c1 = new Course("CS101", "Intro to CS", 3);
        var c2 = new Course("MA201", "Discrete Math", 4);
        Courses.Add(c1);
        Courses.Add(c2);

        // Enroll
        var e1 = EnrollStudent(s1.Id, c1.Id, logger);
        var e2 = EnrollStudent(s2.Id, c1.Id, logger);

        // Grade
        AssignGrade(e1.Id, Grade.A, logger);

        // Drop
        var e3 = EnrollStudent(s1.Id, c2.Id, logger);
        DropEnrollment(e3.Id, logger);

        // Show Active Enrollments
        Console.WriteLine("\n-- Active Enrollments --");

        for (int i = 0; i < Enrollments.Count; i++)
        {
            var e = Enrollments[i];

            if (e.IsActive)
            {
                Student stu = null;
                Course crs = null;

                for (int s = 0; s < Students.Count; s++)
                {
                    if (Students[s].Id == e.StudentId)
                    {
                        stu = Students[s];
                        break;
                    }
                }

                for (int c = 0; c < Courses.Count; c++)
                {
                    if (Courses[c].Id == e.CourseId)
                    {
                        crs = Courses[c];
                        break;
                    }
                }

                Console.WriteLine("Enrollment: " + e.Id);
                Console.WriteLine(" Student: " + (stu != null ? stu.FullName : "Unknown"));
                Console.WriteLine(" Course : " + (crs != null ? crs.Code + " - " + crs.Title : "Unknown"));
                Console.WriteLine(" Grade  : " + e.Grade);
                Console.WriteLine();
            }
        }

        Console.WriteLine("=== END ===");
    }

    static Enrollment EnrollStudent(Guid studentId, Guid courseId, IAppLogger logger)
    {
        Student foundStudent = null;

        foreach (var s in Students)
        {
            if (s.Id == studentId)
            {
                foundStudent = s;
                break;
            }
        }

        if (foundStudent == null)
        {
            throw new NotFoundException("Student not found");
        }

        Course foundCourse = null;

        foreach (var c in Courses)
        {
            if (c.Id == courseId)
            {
                foundCourse = c;
                break;
            }
        }

        if (foundCourse == null)
        {
            throw new NotFoundException("Course not found");
        }

        foreach (var e in Enrollments)
        {
            if (e.StudentId == studentId &&
                e.CourseId == courseId &&
                e.IsActive)
            {
                throw new DuplicateEntityException("Already enrolled");
            }
        }

        var newEnrollment = new Enrollment(studentId, courseId);
        Enrollments.Add(newEnrollment);

        logger.Info("Enrolled " + foundStudent.FullName + " in " + foundCourse.Code);

        return newEnrollment;
    }

    static void AssignGrade(Guid enrollmentId, Grade grade, IAppLogger logger)
    {
        Enrollment foundEnrollment = null;

        foreach (var item in Enrollments)
        {
            if (item.Id == enrollmentId)
            {
                foundEnrollment = item;
                break;
            }
        }

        if (foundEnrollment == null)
        {
            throw new NotFoundException("Enrollment not found");
        }

        foundEnrollment.AssignGrade(grade);
        logger.Info("Grade assigned: " + grade);
    }

    static void DropEnrollment(Guid enrollmentId, IAppLogger logger)
    {
        Enrollment foundEnrollment = null;

        foreach (var item in Enrollments)
        {
            if (item.Id == enrollmentId)
            {
                foundEnrollment = item;
                break;
            }
        }

        if (foundEnrollment == null)
        {
            throw new NotFoundException("Enrollment not found");
        }

        foundEnrollment.Drop();
        logger.Info("Enrollment dropped");
    }
}