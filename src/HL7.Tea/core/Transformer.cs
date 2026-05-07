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
            var regex = new Regex(@"\{[A-Z][A-Z][A-Z,1-9]\-\d+(\.\d+)?\}");
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
                if (regex.IsMatch(newVal))
                {
                    SubstituteFields(msg, path, newVal);
                }
                else
                {
                    msg.SetField(path, newVal);
                }

            }
        }
        public static string GetRandomSixDigits() {
            return _random.Next(0, 1_000_000).ToString("D6");
        }

        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public static void SubstituteFields(HL7Message msg, string targetPath, string sub)
        {
            // Example: sub="prefix-{OBX-3}"
            var regex = new Regex(@"\{[A-Z][A-Z][A-Z,1-9]\-\d+(\.\d+)?\}");
            var matches = regex.Matches(sub);

            string targetSegName = targetPath.Substring(0, 3);
            foreach (var seg in msg.GetSegments(targetSegName))
            {
                string newVal = sub;
                foreach (Match match in matches)
                {
                    string fullMatch = match.Value;
                    string path = fullMatch.Substring(1, fullMatch.Length - 2);
                    string segName = path.Substring(0, 3);
                    string field = null;
                    if (segName == targetSegName)
                        field = seg.GetFieldOne(path);
                    else
                        field = msg.GetFieldOne(path);

                    if (field == null)
                    {
                        newVal = newVal.Replace(fullMatch, "");
                    }
                    else
                    {
                        newVal = newVal.Replace(fullMatch, field);
                    }
                }
                seg.SetField(targetPath, newVal);

            }

        }
    }
}
