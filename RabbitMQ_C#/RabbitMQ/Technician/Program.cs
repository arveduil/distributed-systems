using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technician
{
    class Program
    {
        static void Main(string[] args)
        {

            Technician technician = new Technician(args.ToArray());

            Console.ReadKey();
        }
    }
}
