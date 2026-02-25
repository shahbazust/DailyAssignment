namespace Day2
{
    public class Program
    {
        static void Main(string[] args)
        {
            Student student = new Student(101,"Deepak","Kumar");
            student.Details();
            Console.WriteLine();

            Course course = new Course(202,".NET");
            course.GetCourseDetails();
            Console.WriteLine();
            

            Enrollment enroll = new Enrollment(303, course,student);
            enroll.Enrolled();
            enroll.GetEntrollmentDetails();
            Console.WriteLine();
            

            course.CourseComplete();
            course.DoAssignment();

        }
    }
}