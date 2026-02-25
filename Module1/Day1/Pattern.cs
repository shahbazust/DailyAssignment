using System.Collections.ObjectModel;
using Microsoft.VisualBasic;

namespace Day1
{
   public class Pattern
    {
        
        
        public void Pattern1()
        {
            Console.Write("Enter number of row = ");
            int n=Convert.ToInt32(Console.ReadLine());
            for(int i=0;i<n ;i++)
            {
                for(int j=0;j<=i;j++)
                     Console.Write("* ");
                Console.WriteLine();
            }
        }

          public void Pattern2()
        {
            Console.Write("Enter number of row = ");
            int n=Convert.ToInt32(Console.ReadLine());
            for(int i=0;i<n ;i++)
            {
                for(int j=n-i;j>0;j--)
                     Console.Write("* ");
                Console.WriteLine();
            }
        }

        public void Pattern3()
        {
            Console.Write("Enter number of row = ");
            int n=Convert.ToInt32(Console.ReadLine());
            int space;
            for(int i=0;i<n ;i++)
            {
                space = n-i;
                for(int j=0;j<space;j++)
                {
                    Console.Write("  ");
                }
                for(int j=0;j<=i;j++)
                     Console.Write("* ");
                Console.WriteLine();
            }
        }

        public void Pattern4()
        {
            Console.Write("Enter number of row = ");
            int n=Convert.ToInt32(Console.ReadLine());
            int space;
            int col;
            for(int i=1;i<=2*n-1;i++)
            {
                col = i<=n?i:2*n-i;
                space = n-col;
                for(int j=1;j<=space;j++)
                {
                    Console.Write("  ");
                }
                for(int j=1;j<=col;j++)
                     Console.Write(" *  ");
                Console.WriteLine();
            }
        }
    }
        
          
}