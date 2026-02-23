namespace Day1
{
    class Calculator
    {
        public int Add(int a, int b)
        {
            return a+b;
        }

        public int Subtract(int a,int b)
        {
            if(a>b)return a-b;
            return b-a;
        }

        public int Multiply(int a,int b)
        {
            return a*b;
        }

        public int Division(int a,int b)
        {
            if(b!=0)
            {
                return a/b;
            }
            else
            {
                Console.WriteLine("divisor should not be zero");
                return 0;
            }
        }

        public int Remainder(int a,int b)
        {
            if(b!=0)
            {
                return a%b;
            }
            else
            {
                Console.WriteLine("divisor should not be zero");
                return 0;
            }
        }
    }
}