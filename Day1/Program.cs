namespace Day1
{
    class Program
    {
        static void Main(string[] args)
        {
            // var calc = new Calculator();
            // Console.WriteLine("Add = "+calc.Add(2,5));
            // Console.WriteLine("Subtract = "+calc.Subtract(2,5));
            // Console.WriteLine("Multiply = "+calc.Multiply(2,5));
            // Console.WriteLine("Division = "+calc.Division(2,5));
            // Console.WriteLine("Remainder = "+calc.Remainder(2,5));

            var numanalyzer = new NumberAnalyzer();
            // numanalyzer.PrimeNumber(5);
            // numanalyzer.EvenOrOdd(8);
            Console.WriteLine("Factorial = "+numanalyzer.Factorial(5));

            var pattern = new Pattern();
            pattern.Pattern1();
        }
    }
}
