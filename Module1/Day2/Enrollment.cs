namespace Day2
{
    public class Enrollment
    {
       readonly long enrollentId;
        readonly string courseName;
        readonly long studentId;
        // public Enrollment(){}
        public Enrollment(long id, Course course,Student student)
        {
            this.enrollentId = id;
            this.courseName=course.courseName;
            this.studentId=student.id;
        }

        public void GetEntrollmentDetails()
        {
            Console.WriteLine("id= {0} , courseName = {1} and studnetId = {2}",enrollentId,courseName,studentId);
        }

        public void Enrolled()
        {
            Console.WriteLine("Enrolled successfully");
        }
    }
}