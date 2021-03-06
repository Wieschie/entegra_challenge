﻿using System;
using BusinessCard;

namespace main
{
    class Program
    {
        static void Main()
        {

            BusinessCardParser b = new BusinessCardParser();
            Console.WriteLine(); // add space after warnings
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
            foreach (string t in testStrings)
            {
                Console.WriteLine("Input: \n\n" + t);
                Console.WriteLine("\n==>\n");
                ContactInfo c = b.GetContactInfo(t);
                c.print();
                Console.WriteLine("\n==>\n");
            }
        }
    }
}
