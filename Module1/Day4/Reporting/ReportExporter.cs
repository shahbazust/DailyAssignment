using System;
using System.IO;
using System.Text;
using StudentManagement.Domain;

namespace StudentManagement.Reporting
{
    public static class ReportExporter
    {
        public static string BuildTextReport(DataSnapshot snap)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== STUDENT MANAGEMENT REPORT ===");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('-', 60));
            sb.AppendLine($"Total Students: {snap.Students.Count}");
            sb.AppendLine($"Total Courses : {snap.Courses.Count}");
            sb.AppendLine($"Total Enrolls : {snap.Enrollments.Count}");
            sb.AppendLine();

            for (int i = 0; i < snap.Students.Count; i++)
            {
                var s = snap.Students[i];
                sb.AppendLine($"Student: {s.FullName} ({s.Email})  Age: {s.Age}");

                bool any = false;

                for (int e = 0; e < snap.Enrollments.Count; e++)
                {
                    var enr = snap.Enrollments[e];
                    if (enr.StudentId != s.Id) continue;

                    Course? crs = null;
                    for (int c = 0; c < snap.Courses.Count; c++)
                    {
                        if (snap.Courses[c].Id == enr.CourseId)
                        {
                            crs = snap.Courses[c];
                            break;
                        }
                    }

                    any = true;

                    if (crs is null)
                    {
                        sb.AppendLine($"  - (course not found) | Grade: {enr.Grade} | Active: {enr.IsActive}");
                    }
                    else
                    {
                        sb.AppendLine($"  - {crs.Code} {crs.Title} ({crs.Credits} cr) | Grade: {enr.Grade} | Active: {enr.IsActive}");
                    }
                }

                if (!any) sb.AppendLine("  (no enrollments)");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static void ExportToFile(DataSnapshot snap, string filePath = "report.txt")
        {
            var content = BuildTextReport(snap);

            var full = Path.GetFullPath(filePath);
            var dir = Path.GetDirectoryName(full);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(full, content, Encoding.UTF8);
        }
    }
}