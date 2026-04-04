using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HL7.Tea
{
    public class HL7Path
    {
        public string Path { get; }
        public string SegmentName { get; }
        public int Field { get; }
        public int? SubField { get; }

        public bool IsField { get
            {
                return SubField == null;
            }
        }

        public bool IsSubField {
            get
            {
                return SubField != null;
            }
        }
        public static void Validate(string path)
        {
            var regex = new Regex(@"^[A-Z][A-Z][A-Z,1-9]-\d+(\.\d+)?$");

            if (!regex.IsMatch(path))
            {
                throw new ArgumentException($"Invalid HL7 path {path}. It must be like AAA-1.3");
            }
        }

        public HL7Path(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            HL7Path.Validate(path);

            // Split into segment name and field/subfield
            var parts = path.Split('-');
            SegmentName = parts[0];
            var fieldSubfield = parts[1];

            string fieldPart = fieldSubfield;
            int? subfieldPart = null;

            // Handle subfield if present
            if (fieldSubfield.Contains("."))
            {
                var fs = fieldSubfield.Split('.');
                fieldPart = fs[0];

                if (!int.TryParse(fs[1], out int parsedSubfield))
                    throw new ArgumentException($"Invalid non-integer subfield {fs[1]}");

                if (parsedSubfield < 1)
                    throw new ArgumentException($"Invalid subfield value {parsedSubfield}. It should be >= 1.");

                subfieldPart = parsedSubfield;
            }

            // Parse field
            if (!int.TryParse(fieldPart, out int parsedField))
                throw new ArgumentException($"Invalid non-integer field {fieldPart}");

            if (parsedField < 1)
                throw new ArgumentException($"Invalid field value {parsedField}. It should be >= 1.");

            // Special case for MSH
            if (SegmentName == "MSH")
                parsedField -= 1;

            Field = parsedField;
            SubField = subfieldPart;
        }

        public override string ToString() => Path;

    }
}
