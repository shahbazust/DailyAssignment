using System;
namespace Day2
{
    public class Course : IRepository
    {
        public long id;
        public string courseName;
        public Course(long id,string courseName)
        {
           this.id=id;
            this.courseName=courseName;
        }
        public void GetCourseDetails()
        {
            Console.WriteLine(string.Format("Courseid= {0} and courseName = {1}",id,courseName));
        }
        public void CourseComplete()
        {
            Console.WriteLine("Course is Completed");
        }
        public void DoAssignment()
        {
            Console.WriteLine("Assignment Completed");
        }
    }
}