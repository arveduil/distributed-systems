using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace Doctor
{
    public class Program
    {
       public static void Main(string[] args)
        {
            Doctor doc1 = new Doctor();
            // Doctor doc2 = new Doctor(2);
            //Request request1 = new Request("Boszcz", "knee");
            //Request request2 = new Request("Cfel", "hip");
            //Request request3 = new Request("Janusz", "elbow");
            if (args.Length != 2) return;

            //Request request = new Request(args[0],args[1]);

            while(true)
            {
                var line = Console.ReadLine();
                try
                {
                    var name = line.Split(' ')[0];
                    var type = line.Split(' ')[1];

                    Request request = new Request(name, type);
                }
                catch (IndexOutOfRangeException)
                {
                    return;
                }
            }

          //  ProceedAction(doc1, request);
             //ProceedAction(doc1, request2);
             // ProceedAction(doc1, request3);

           // Console.ReadKey();
        }

        private static void ProceedAction(Doctor doc1, Request request1)
        {
            Console.WriteLine("Doctor {0} makes request {1}", doc1.ID, request1.ToString());
            var response = doc1.Call(request1);
            Console.WriteLine("Doctor {0} received response: {1}", doc1.ID, response);
        }
    }

    public class Request
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Request(string name, string type)
        {
            Name = name;
            Type = type;
        }
        public override string ToString()
        {
            return string.Format(Name + " " + Type);
        }

    }
}
