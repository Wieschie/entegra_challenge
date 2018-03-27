using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.ling;
using java.util;

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
            string name = extractName(ref document);
            string email = extractEmail(ref document);
            string phone = extractPhone(ref document);
            return new ContactInfo(name, phone, email);
        }

        // note: SLF4J warning is a known issue and does not adversely affect anything
        // https://github.com/sergey-tihon/Stanford.NLP.NET/issues/79
        CRFClassifier nameClassifier = CRFClassifier.getClassifierNoExceptions(
            @"english.all.3class.distsim.crf.ser.gz");
        private string extractName(ref string document)
        {
            StringBuilder name = new StringBuilder();

            // Split document into individual lines.  Linguistic context isn't important on a business card
            // and may actually confuse the classifier.
            string[] lines = document.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                List labels = nameClassifier.classify(line);

                foreach (List sentence in labels.toArray())
                {
                    CRFCliqueTree cliqueTree = nameClassifier.getCliqueTree(sentence);
                    Array sentenceArr = sentence.toArray();
                    //foreach (CoreLabel word in sentence.toArray())
                    for (int i = 0; i < sentenceArr.Length; i++)
                    {
                        CoreLabel word = (CoreLabel) sentenceArr.GetValue(i);
                        Console.Write("{0}/{1} ", word.word(), word.get(new CoreAnnotations.AnswerAnnotation().getClass()));
                        if ((string)word.get(new CoreAnnotations.AnswerAnnotation().getClass()) == "PERSON")
                        {
                            if (name.Length > 0)
                                name.Append(" ");
                            name.Append(word.word());
                        }
                        Console.WriteLine('\n');

                        for (Iterator iter = nameClassifier.classIndex.iterator(); iter.hasNext();)
                        {
                            String l = (string)iter.next();
                        //string l = "PERSON";
                            int index = nameClassifier.classIndex.indexOf(l);
                            double probability = cliqueTree.prob(i, index);
                            Console.WriteLine("\t" + l + "(" + probability + ")");
                        }
                    }

                    Console.WriteLine('\n');
                }
            }
            return name.ToString();
        }


        // regexes stored as members so they are compiled once and kept in memory for repeated use.
        // matches all valid US numbers with capture groups for each segment (4 total)
        //                                    optionally match US country code
        //                                    |         parentheses are optional
        //                                    |          |   Area codes cannot start with 0
        //                                    |          |   |               all delimiters are optional
        //                                    |          |   |               |             
        private static string phoneFormat = @"(1?)[-. ]?\(?([1-9][\d]{2})\)?[-. ]?([\d]{3})[-. ]?([\d]{4})";
        private static Regex phoneRegex = new Regex(phoneFormat, RegexOptions.Compiled);

        private string extractPhone(ref string document)
        {
            MatchCollection matches = phoneRegex.Matches(document);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Valid phone number not found.");
            }
            string cleaned_phone_number = phoneRegex.Replace(matches[0].Value, "$1$2$3$4");
            return cleaned_phone_number;
        }


        private static string emailFormat = @"[\w\d%+-.]+@[A-Z\d.-]+\.[A-Z]{2,}";
        private static Regex emailRegex = new Regex(emailFormat, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string extractEmail(ref string document)
        {
            MatchCollection matches = emailRegex.Matches(document);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Valid email not found.");
            }
            // @TODO: handle this in-function?
            if (matches.Count > 1)
            {
                throw new ArgumentException("Multiple emails found.");
            }

            return matches[0].Value.ToLower();
        }

    }
}
