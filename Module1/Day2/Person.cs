namespace Day2
{
    public class Person
    {
        public long id;
        public string firstName;
        public string lastName;
        public Person(long id,string firstName,string lastName)
        {
            this.id = id;
            this.firstName =firstName;
            this.lastName =lastName;
        }

        public void Details()
        {
            Console.WriteLine(string.Format("Sudent id = {0} , FirstName = {1}  LastName = {2} ",id,firstName,lastName));
        }

    }
}