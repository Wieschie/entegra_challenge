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
            string[] test = {
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
                Tel: +1 (703) 555-1259
                Fax: +1 (703) 555-1200
                bsmith@abctech.com"
            };
            foreach (string t in test)
            {
                ContactInfo c = b.GetContactInfo(t);
                c.print();
            }
        }
    }
}
