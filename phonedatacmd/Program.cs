using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phonedata;

namespace phonedatacmd
{
    class Program
    {


        static void Main(string[] args)
        {
            Phonedata.Phonedata pd = new Phonedata.Phonedata();
            pd.Init("phone.dat");
            string output;
            output = pd.Lookup("13822399111").ToString();
            Console.WriteLine(output);
        }
    }
}
