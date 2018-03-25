using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    class Program
    {
        public static void Main(string[] args)
        {
            Tests.TestClass.Join().Wait();
            //TestClass.WhereTests().Wait();
        }
    }
}
