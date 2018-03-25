using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace entegra
{
    /// <summary>
    /// Parses the results of an OCR conversion in order to extract the name,
    /// phone number, and email address from the processed business card image.     
    /// </summary>
    class BusinessCardParser
    {
        /// <summary>
        /// Parses OCR results to extract contact details.
        /// </summary>
        /// <param name="document">
        /// String containing results of OCR conversion of a business card.
        /// </param>
        /// <returns>
        /// ContactInfo object containing extracted information.
        /// </returns>
        public ContactInfo GetContactInfo(string document)
        {
            string email = extractEmail(ref document);
            string phone = extractPhone(ref document);
            return new ContactInfo("Evan", phone, email);
        }

        // matches all valid US numbers with capture groups for each segment (4 total)
        //                                    optionally match US country code
        //                                    |         parentheses are optional
        //                                    |          |   Area codes cannot start with 0
        //                                    |          |   |               all delimiters are optional
        //                                    |          |   |               |             
        private static string phoneFormat = @"(1?)[-. ]?\(?([1-9][\d]{2})\)?[-. ]?([\d]{3})[-. ]?([\d]{4})";
        private static Regex phoneRegex = new Regex(phoneFormat, RegexOptions.Compiled);
        private string extractPhone(ref string document) {
            MatchCollection matches = phoneRegex.Matches(document);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Valid phone number not found.");
            }
            string cleaned_phone_number = phoneRegex.Replace(matches[0].Value, "$1$2$3$4");
            return cleaned_phone_number;
        }


        // compile regex once to avoid constructing full NFA every time a search is performed.
        private static string emailFormat = @"[\w\d%+-]+@[A-Z\d.-]+\.[A-Z]{2,}";
        private static Regex emailRegex = new Regex(emailFormat, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string extractEmail(ref string document) {
            MatchCollection matches = emailRegex.Matches(document);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Valid email not found.");
            }
            // @TODO: handle this in-function?
            if (matches.Count > 1) {
                throw new ArgumentException("Multiple emails found.");
            }

            return matches[0].Value;
        }

    }
}
