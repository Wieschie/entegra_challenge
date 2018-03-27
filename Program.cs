using System;
using BusinessCard;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace main
{
    class Program
    {
        static void Main()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            BusinessCardParser b = new BusinessCardParser();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms to initialize BusinessCardParser.");
            string[] testStrings = {
@"Entegra Systems
John Doe
Senior Software Engineer
(410)555-1234
john.doe@entegrasystems.com",
@"Acme Technologies
Analytic Developer
Jane Doe
1234 Roadrunner Way
Columbia, MD 12345
Phone: 410-555-1234
Fax: 410-555-4321
Jane.doe@acmetech.com",
@"Bob Smith
Software Engineer
Decision & Security Technologies
ABC Technologies
123 North 11th Street
Suite 229
Arlington, VA 22209
Fax: +1 (703) 555-1200
Tel: +1 (703) 555-1259
bsmith@abctech.com",
@"Evan Schiewe
Elmhurst College
t: 6307910628
c: 6302580628
evan.schiewe@gmail.com"
            };

            List<long> runtimes = new List<long>();
            for (int i = 0; i < 100; i++)
            {
                foreach (string t in testStrings)
                {
                    //Console.WriteLine("Input: \n\n" + t);
                    //Console.WriteLine("\n==>\n");
                    stopwatch.Restart();
                    ContactInfo c = b.GetContactInfo(t);
                    stopwatch.Stop();
                    runtimes.Add(stopwatch.ElapsedMilliseconds);
                    //c.print();
                    //Console.WriteLine("\n==>\n");
                }
            }
            Console.WriteLine("Average time to get contact info was " + runtimes.Average() + "ms.");
        }
    }
}
