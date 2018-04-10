using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Phonedata;

namespace phonedata_benchmark
{
    class Program
    {
       static  List<string> Data;
        static void Benchmark_add_element()
        {
            Data = new List<string>();
            for (int i = 1; i < 100000;i++)
            {
                string phone_info;
                phone_info = string.Format("{0}{1}{2}{3}", "1897", i & 10000, "45","2678");
                Data.Add(phone_info);
            }
        }
        static void Benchmark_test()
        {
            Phonedata.Phonedata pd = new Phonedata.Phonedata("phone.dat");
            List<string> data = Program.Data;
            DateTime dt1 = DateTime.Now;
            Parallel.ForEach(data, (i) =>
                {
                    pd.Lookup(i);

                });
            DateTime dt2 = DateTime.Now;
            Console.WriteLine("并行计算 100000个手机号码的查找，用时： {0}毫秒。\n", (dt2 - dt1).TotalMilliseconds);


        }
        static void Main(string[] args)
        {
            Benchmark_add_element();
            Benchmark_test();
            Console.ReadKey();
        }
    }
}
