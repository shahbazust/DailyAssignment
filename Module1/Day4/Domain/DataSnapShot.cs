using System.Collections.Generic;

namespace StudentManagement.Domain
{
    public sealed class DataSnapshot
    {
        public List<Student> Students { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}