using Console = System.Console;

namespace BusinessCard
{
    /// <summary>
    /// Stores contact info from business cards.
    /// </summary>
    class ContactInfo
    {
        private readonly string _name;
        private readonly string _phoneNumber;
        private readonly string _emailAddress;

        public ContactInfo(string name, string phone, string email)
        {
            _name = name;
            _phoneNumber = phone;
            _emailAddress = email;
        }

        public void print()
        {
            Console.WriteLine("Name: {0}", _name);
            Console.WriteLine("Phone: {0}", _phoneNumber);
            Console.WriteLine("Email: {0}", _emailAddress);
        }

        public string getName()
        {
            return _name;
        }

        public string getPhoneNumber()
        {
            return _phoneNumber;
        }

        public string getEmailAddress()
        {
            return _emailAddress;
        }
    }
}
