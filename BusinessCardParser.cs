using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.ling;
using jList = java.util.List;

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
        private static string modelFilename = @"english.all.3class.distsim.crf.ser.gz";
        CRFClassifier nameClassifier = CRFClassifier.getClassifierNoExceptions(modelFilename);

        private string extractName(ref string document)
        {
            const string desiredLabel = "PERSON";

            // store potential strings and their associated PERSON probabilities 
            List<Tuple<StringBuilder, List<double>>> potentialNames = new List<Tuple<StringBuilder, List<double>>>();

            // Split document into individual lines.  Linguistic context isn't important on a business card
            // and may actually confuse the classifier.
            string[] lines = document.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                jList labels = nameClassifier.classify(line);

                foreach (jList sentence in labels.toArray())
                {
                    // CliqueTree is used to extract probabilities
                    CRFCliqueTree cliqueTree = nameClassifier.getCliqueTree(sentence);
                    StringBuilder name = new StringBuilder();
                    List<double> probabilities = new List<double>();
                    Array sentenceArr = sentence.toArray();
                    for (int i = 0; i < sentenceArr.Length; i++)
                    {
                        CoreLabel word = (CoreLabel) sentenceArr.GetValue(i);
                        if ((string) word.get(new CoreAnnotations.AnswerAnnotation().getClass()) == desiredLabel)
                        {
                            int index = nameClassifier.classIndex.indexOf(desiredLabel);
                            probabilities.Add(cliqueTree.prob(i, index));

                            if (name.Length > 0)
                                name.Append(" ");
                            name.Append(word.word());
                        }
                    }
                    if (name.Length > 0)
                    {
                        potentialNames.Add(new Tuple<StringBuilder, List<double>>(name, probabilities));
                    }
                }
            }

            // choose the line classified as PERSON with the highest confidence
            string finalName = "";
            double max = 0;
            foreach (Tuple<StringBuilder, List<double>> t in potentialNames)
            {
                double a = t.Item2.Average();
                if (a > max)
                {
                    max = a;
                    finalName = t.Item1.ToString();
                }
            }

            return finalName;
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
