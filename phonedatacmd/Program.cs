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
            if(args.Length == 1 )
            {
                output = pd.Lookup(args[0]).ToString();
                Console.WriteLine(output);
            } else
            {
                Console.WriteLine("Usage:\t\n\tphonedatacmd.exe PhoneNumber\n");
            }
 
        }
    }
}
