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
            Phonedata.PhoneData pd = new Phonedata.PhoneData("phone.dat");
            string output;
            //output = pd.Lookup("1892427").ToString();
            output = pd.Lookup("1921892").ToString();
            Console.WriteLine(output);
            Console.ReadKey();
            /*
             * 1344710142
             * 1344711000
             */
        }
    }
}
