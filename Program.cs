using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace entegra
{
    class Program
    {
        static void Main(string[] args)
        {

            BusinessCardParser b = new BusinessCardParser();
            ContactInfo c = b.GetContactInfo("hello evan@evan.com +.(630)-267-4464");
            c.print();
        }
    }
}
