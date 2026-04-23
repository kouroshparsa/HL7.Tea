using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HL7.Tea.Core
{
    public class Segment
    {
        public string Name { get; set;  }
        public List<string> Fields { get; set; }

        public Segment(string name, List<string> fields)
        {
            this.Name = name;
            Fields = fields ?? new List<string>();
        }

        public List<string> GetFieldAll(string path)
        {
            var hp = new HL7Path(path);
            var res = new List<string>();

            if (hp.Field <= 0)
                return res;

            if (!hp.IsSubField)
            {
                if (hp.Field <= Fields.Count)
                {
                    var val = Fields[hp.Field - 1];
                    res.AddRange(val.Split('~'));
                }
            }
            else
            {
                if (hp.Field <= Fields.Count)
                {
                    var fieldVal = Fields[hp.Field - 1];
                    foreach (var fv in fieldVal.Split('~'))
                    {
                        var subs = fv.Split('^');
                        if (hp.SubField <= subs.Length)
                        {
                            res.Add(subs[hp.SubField.Value - 1]);
                        }
                    }
                }
            }

            return res;
        }

        // Retrieves only the first value of a field/subfield
        public string GetFieldOne(string path)
        {
            if (!path.Contains("-"))
            {
                path = $"{Name}-{path}";
            }

            var res = GetFieldAll(path);
            return res.Count > 0 ? res[0] : null;
        }

        // Sets a field or subfield value
        public void SetField(string path, object newValue, int? repetitionIndex = null)
        {
            if (repetitionIndex.HasValue && repetitionIndex.Value < 1)
            {
                throw new ArgumentException($"Invalid repetitionIndex={repetitionIndex}. It must be greater than zero.");
            }

            string valueToSet;

            if (newValue is List<string> listValue)
            {
                valueToSet = string.Join("~", listValue);
            }
            else if (newValue is string strValue)
            {
                valueToSet = strValue;
            }
            else
            {
                throw new ArgumentException($"Invalid type for newValue. It must be string or List<string>, but got {newValue.GetType().Name}");
            }

            var hp = new HL7Path(path);

            if (hp.IsSubField && valueToSet.Contains("^"))
            {
                throw new ArgumentException("Subfield paths cannot contain '^' in the value.");
            }

            // Expand the field list if needed
            while (hp.Field > this.Fields.Count)
            {
                this.Fields.Add(string.Empty);
            }

            var field = Fields[hp.Field - 1];
            var repetitions = new List<string>();
            int repInd = 1;

            foreach (var rep in field.Split('~'))
            {
                if (repetitionIndex.HasValue && repInd != repetitionIndex.Value)
                {
                    repetitions.Add(rep);
                    repInd++;
                    continue;
                }

                if (hp.IsSubField)
                {
                    var subfields = rep.Split('^').ToList();
                    while (hp.SubField > subfields.Count)
                    {
                        subfields.Add(string.Empty);
                    }

                    subfields[hp.SubField.Value - 1] = valueToSet;
                    repetitions.Add(string.Join("^", subfields));
                }
                else
                {
                    repetitions.Add(valueToSet);
                }

                repInd++;
            }

            Fields[hp.Field - 1] = string.Join("~", repetitions);
        }

        // Removes a field or subfield
        public void RemoveField(string path, int? repetitionIndex = null)
        {
            var hp = new HL7Path(path);

            if (hp.Field < 1 || hp.Field > this.Fields.Count)
            {
                throw new ArgumentException($"Invalid path: {path}");
            }

            if (hp.IsSubField)
            {
                var newFields = new List<string>();
                var repeated = this.Fields[hp.Field - 1].Split('~').ToList();

                if (repetitionIndex.HasValue)
                {
                    if (repetitionIndex.Value < 1 || repetitionIndex.Value > repeated.Count)
                        throw new ArgumentException($"Invalid repetitionIndex={repetitionIndex}");
                    repeated = new List<string> { repeated[repetitionIndex.Value - 1] };
                }

                foreach (var field in repeated)
                {
                    var subfields = field.Split('^').ToList();
                    if (hp.SubField > 0 && hp.SubField <= subfields.Count)
                    {
                        subfields.RemoveAt(hp.SubField.Value - 1);
                    }
                    newFields.Add(string.Join("^", subfields));
                }

               this. Fields[hp.Field - 1] = string.Join("~", newFields);
            }
            else
            {
                this.Fields.RemoveAt(hp.Field - 1);
            }
        }

        public override string ToString(){
            string fieldsStr = string.Join("|", this.Fields);
            return $"{this.Name}-{fieldsStr}";
        }
    }
}
