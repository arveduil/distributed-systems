using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Send
{
    public class MainClass
    {
         public static void Main(string[] args)
        {
            var skillsSet1 = new string[] { "knee","elbow" };
            var skillsSet2 = new string[] { "knee", "hip" };
            var skillsSet3 = new string[] { "hip" };

            //Task task1 = new Task(() => new Technician(1, skillsSet1));
            //Task task2 = new Task(() => new Technician(2, skillsSet2));
            //Task task3 = new Task(() => new Technician(3, skillsSet3));


            Thread t1 = new Thread(() => new Technician(1, skillsSet1));
            Thread t2 = new Thread(() => new Technician(2, skillsSet2));
            Thread t3 = new Thread(() => new Technician(3, skillsSet3));
            t1.Start();
            t2.Start();
            t3.Start();
            //Technician technician1 = new Technician(1, skillsSet1);
            //Technician technician2 = new Technician(2, skillsSet2);
            //Technician technician3 = new Technician(3, skillsSet3);

            Console.ReadKey();
        }
    }
}
