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

       public  long Factorial(int num)
        {
            if(num==0 || num ==1)
            {
                return 1;
            }
            return num*Factorial(num -1);
        }
    }
}