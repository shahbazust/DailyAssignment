namespace Day1
{
    class NumberAnalyzer
    {
       public  void PrimeNumber(int num)
        {
            for(int i=2;i*i<=num;i++)
            {
                if(num%i==0)
                {
                    Console.WriteLine(num +"is not Prime");
                    return;
                }
            }
            Console.WriteLine(num+" Prime");
        }

       public  void EvenOrOdd(int num)
        {
            if(num%2==0)
            {
                Console.WriteLine(num+" is Even");
            }
            else
            {
                Console.WriteLine(num+" is Odd");
            }
        }

       public  void Factorial(int num)
        {
            int fact=1;
            if(num==0 || num ==1)
            {
                Console.WriteLine("Factorial of "+num+" is "+fact);
                return;
            }
            else
            {
                for(int i=num;i>1;i--)
                {
                    fact*=i;
                }
                Console.WriteLine("Factorial of "+num+" is "+fact);

            }
        }
    }
}