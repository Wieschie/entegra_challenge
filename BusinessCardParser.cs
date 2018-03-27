using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.ling;
using jList = java.util.List;

namespace BusinessCard
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
                        CoreLabel word = (CoreLabel)sentenceArr.GetValue(i);
                        if ((string)word.get(new CoreAnnotations.AnswerAnnotation().getClass()) == desiredLabel)
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

      
        // Match any preceding characters for text telephone/fax labels, and optionally US country code
        private static string phonePrefix = @".*?(1)?";
        // all delimiters are optional
        private static string phoneDelimiter = @"[-. ]?";
        // Area codes cannot start with 0
        private static string areaCode = @"\(?([1-9][\d]{2})\)?";
        private static string localNumber = @"([\d]{3})" + phoneDelimiter + @"([\d]{4})";
        // matches all valid US numbers with capture groups for easy digit extraction  
        private static string phoneFormat = phonePrefix + phoneDelimiter + areaCode + phoneDelimiter + localNumber;
        private static Regex phoneRegex = new Regex(phoneFormat, RegexOptions.Compiled);

        private static string extractPhone(ref string document)
        {
            Match numberMatch = Match.Empty;
            MatchCollection matches = phoneRegex.Matches(document);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Valid phone number not found.");
            }
            if (matches.Count > 1)
            {
                // match any variation of telephone, phone, cellphone while avoiding fax
                Regex telephoneRegex = new Regex(@"t|p|c", RegexOptions.IgnoreCase);
                foreach (Match m in matches)
                {
                    if (telephoneRegex.IsMatch(m.Value))
                    {
                        numberMatch = m;
                        break;
                    }
                }
                // no labels found, take the first number.
                if (numberMatch == Match.Empty)
                    numberMatch = matches[0];
            }
            else
            {
                numberMatch = matches[0];
            }
            string cleaned_phone_number = phoneRegex.Replace(numberMatch.Value, "$1$2$3$4");
            return cleaned_phone_number;
        }


        private static string emailLocal = @"[\w\d%+-.]+";
        // @ domain, including any subdomains
        private static string emailDomain = @"@[A-Z\d.-]+";
        private static string emailTLD = @"\.[A-Z]{2,}";
        private static string emailFormat = emailLocal + emailDomain + emailTLD;
        private static Regex emailRegex = new Regex(emailFormat, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string extractEmail(ref string document)
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
