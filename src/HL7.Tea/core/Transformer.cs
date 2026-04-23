using RandomFriendlyNameGenerator;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HL7.Tea.Core
{
    public class Transformer
    {
        private static readonly Random _random = new Random();

        public static void Transform(HL7Message msg, Dictionary<string, string>specs)
        {
            foreach (var item in specs)
            {
                string path = item.Key;
                string newVal = item.Value;
                newVal = newVal.Replace("{random_num}", GetRandomSixDigits());
                newVal = newVal.Replace("{now.14}", GetCurrentDate());
                newVal = newVal.Replace("{now.12}", GetCurrentDate().Substring(0, 12));
                newVal = newVal.Replace("{now}", GetCurrentDate().Substring(0, 12));
                newVal = newVal.Replace("{random_first_name}", NameGenerator.PersonNames.Get());
                newVal = newVal.Replace("{random_last_name}", NameGenerator.PersonNames.Get());
                newVal = SubstituteFields(msg, newVal);
                msg.SetField(path, newVal);
            }
        }
        public static string GetRandomSixDigits() {
            return _random.Next(0, 1_000_000).ToString("D6");
        }

        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public static string SubstituteFields(HL7Message msg, string val)
        {
            var regex = new Regex(@"\{[A-Z][A-Z][A-Z,1-9]\-\d+(\.\d+)?\}");
            var matches = regex.Matches(val);

            var substitutions = new List<(string Match, string Value)>();

            foreach (Match match in matches)
            {
                string fullMatch = match.Value;
                string path = fullMatch.Substring(1, fullMatch.Length - 2);

                string field = msg.GetFieldOne(path);

                if (field == null)
                {
                    substitutions.Add((fullMatch, ""));
                }
                else
                {
                    substitutions.Add((fullMatch, field));
                }
            }

            foreach (var sub in substitutions)
            {
                val = val.Replace(sub.Match, sub.Value);
            }

            return val;
        }
    }
}
