namespace Day2
{
    public class Student : Person 
    {
        public Student(long id,string firstName,string lastName) //calling constructor of parent class
         : base(id,firstName,lastName)
        {
        }
    }
}