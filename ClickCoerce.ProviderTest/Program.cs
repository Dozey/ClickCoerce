using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClickCoerce.Providers;
using ClickCoerce.Providers.Amazon;
using ClickCoerce.Providers.Composite;

namespace ClickCoerce.ProviderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //AmazonContext.AwsAccessKeyId = "1D8KES1NDB2XPSSKCMG2";

            //Console.WriteLine("AsyncItemFetcher");
            //Console.WriteLine();
            //Console.WriteLine();
            //AmazonContext.FetchMode = ResultFetchMode.Async;
            //AmazonQuery<Computer> computers1 = new AmazonQuery<Computer>(true);

            //DateTime start1 = DateTime.Now;
            //var query1 = (from computer in computers1 where computer.Manufacturer == "Asus" select computer);
            //foreach (Computer computer in query1)
            //{
            //    Console.WriteLine(computer.Title);
            //    Console.WriteLine();
            //}
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine(DateTime.Now.Subtract(start1).ToString());
            //Console.WriteLine("Press return to continue...");
            //Console.ReadLine();

            //Console.WriteLine();
            //Console.WriteLine();

            //Console.WriteLine("SyncItemFetcher");
            //Console.WriteLine();
            //Console.WriteLine();
            //AmazonContext.FetchMode = ResultFetchMode.Sync;
            //AmazonQuery<Computer> computers2 = new AmazonQuery<Computer>(true);

            //DateTime start2 = DateTime.Now;
            //var query2 = (from computer in computers2 where computer.Manufacturer == "Asus" select computer);
            //foreach (Computer computer in query2)
            //{
            //    Console.WriteLine(computer.Title);
            //    Console.WriteLine();
            //}
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine(DateTime.Now.Subtract(start2).ToString());
            //Console.WriteLine("Press return to continue...");
            //Console.ReadLine();
        }
    }

    class Computer
    {
        [Binding(AmazonCriteria.Manufacturer, Mode=BindingMode.Read | BindingMode.Write)]
        public string Manufacturer { get; set; }

        [Binding(AmazonCriteria.ProductGroup)]
        public string ProductGroup { get; set; }

        [Binding(AmazonCriteria.Title)]
        public string Title { get; set; }
    }
}
