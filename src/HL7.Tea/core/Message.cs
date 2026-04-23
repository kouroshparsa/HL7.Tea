
using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7.Tea.Core
{
    public class HL7Message
    {
        const string START_BLOCK = "\x0b";
        const string END_BLOCK = "\x1c\x0d";

        public List<Segment> Segments { get; private set; }
        // Example: key: OBX value: [first obx, second obx] where first obx is a list of fields [obx-1,obx-2,...]
        public Dictionary<string, string> Promotions { get; private set; }

        private void Parse(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                msg = msg.Replace("\n", "\r")
                         .Replace("\r\r", "\r")
                         .Trim();

                var lines = msg.Split('\r');

                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    var segName = parts[0];
                    // Skip first field (segment name)
                    Segments.Add(new Segment(segName, parts.Skip(1).ToList<string>()));
                }

            }
        }

        public HL7Message(string msg)
        {
            Segments = new List<Segment>();
            Promotions = new Dictionary<string, string>();
            Parse(msg);
        }

        public HL7Message()
        {
            Segments = new List<Segment>();
            Promotions = new Dictionary<string, string>();
        }

        public bool HasSegment(string segmentName)
        {
            foreach(var seg in this.Segments)
            {
                if (seg.Name == segmentName)
                    return true;
            }
            return false;
        }
        
        public List<Segment>GetSegments(string segmentName)
        {
            return this.Segments.Where(seg => seg.Name == segmentName).ToList();
        }

        public void RemoveSegment(string segmentName, int? delInd = null)
        {
            /// <summary>
            /// Removes all segments matching the segment name.
            /// delInd is an optional zero-based index parameter that can be used
            /// to delete a specific repetition of the segment.
            /// </summary>

            int segInd = 0;

            for (int i = this.Segments.Count - 1; i >= 0; i--)
            {
                var seg = this.Segments[i];

                if (seg.Name == segmentName)
                {
                    if (delInd.HasValue)
                    {
                        if (delInd.Value == segInd)
                        {
                            this.Segments.RemoveAt(i);
                            return;
                        }
                        segInd++;
                    }
                    else
                    {
                        this.Segments.RemoveAt(i);
                    }
                }
            }
        }

        public void AddSegment(string line)
        {
            string[] parts = line.Split('|');
            string segName = parts[0];
            this.Segments.Add(new Segment(segName, parts.Skip(1).ToList<string>()));
        }
        public List<string> GetFieldAll(string path)
        {
            var hp = new HL7Path(path);
            var res = new List<string>();
            foreach (var seg in this.Segments) {
                if (seg.Name == hp.SegmentName) {
                    foreach (var val in seg.GetFieldAll(path)) {
                        res.Add(val);
                    }
                }
            }
            return res;
        }
        public string GetFieldOne(string path)
        {
            var hp = new HL7Path(path);
            foreach (var seg in this.Segments) {
                if (seg.Name == hp.SegmentName)
                    return seg.GetFieldOne(path);
            }
            return null;
        }

        public void RemoveField(
            string path,
            int? segmentRepetitionIndex = null,
            int? fieldRepetitionIndex = null)
        {
            var hp = new HL7Path(path);
            int segInd = 0;

            foreach (var seg in this.Segments)
            {
                if (seg.Name == hp.SegmentName)
                {
                    if (segmentRepetitionIndex.HasValue)
                    {
                        if (segmentRepetitionIndex.Value == segInd)
                        {
                            seg.RemoveField(path, fieldRepetitionIndex);
                        }
                    }
                    else
                    {
                        seg.RemoveField(path, fieldRepetitionIndex);
                    }

                    segInd++;
                }
            }
        }

        public void SetField(string path, object newValue, int? repetitionIndex = null)
        {
            /// <summary>
            /// Sets a field value given its path.
            /// For repeating fields, you can specify repetitionIndex (1-based).
            /// </summary>

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
                throw new ArgumentException($"Invalid type for newValue. It must be a string or a List<string>, but it is {newValue.GetType().Name}");
            }

            var hp = new HL7Path(path);

            if (hp.IsSubField && valueToSet.Contains("^"))
            {
                throw new ArgumentException("Since your path is a subfield path, your value must not contain '^'.");
            }

            if (!HasSegment(hp.SegmentName))
            {
                this.Segments.Add(new Segment(hp.SegmentName, new List<string> { "" }));
            }

            foreach (var seg in this.Segments)
            {
                if (seg.Name != hp.SegmentName)
                    continue;

                seg.SetField(path, valueToSet, repetitionIndex);
            }
        }

        public string Content
        {
            get
            {
                var lines = new List<string>();

                foreach (var seg in this.Segments)
                {
                    lines.Add($"{seg.Name}|{string.Join("|", seg.Fields)}");
                }

                return string.Join("\r", lines);
            }
        }

        public override string ToString()
        {
            return this.Content.Replace("\r", "\r\n");
        }

        public void Promote(Dictionary<string, string> map)
        {
            foreach(var item in map)
            {
                this.Promotions[item.Key] = item.Value;
            }
        }

        public string GetPromotion(string key)
        {
            return GetFieldOne(this.Promotions[key]);
        }
        
        public int? GetPatientAge(DateTime? specifiedToday = null)
        {
            DateTime today = specifiedToday ?? DateTime.Today;

            // Retrieves patient's age from PID segment
            if (!HasSegment("PID"))
                return null;

            string pid7 = GetFieldOne("PID-7");
            if (!pid7.All(char.IsDigit))
                return null;

            if (!DateTime.TryParseExact(pid7, "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime dob))
            {
                return null;
            }

            int age = today.Year - dob.Year;

            // Adjust if birthday hasn't occurred yet this year
            if (today.Month < dob.Month ||
               (today.Month == dob.Month && today.Day < dob.Day))
            {
                age--;
            }

            return age;
        }
        
        public void Transform(Dictionary<string, string> specs)
        {
            Transformer.Transform(this, specs);
        }

        public string GetAck(string ackType = "AA") {
            string sending_app = GetFieldOne("MSH-3");
            string sending_fac = GetFieldOne("MSH-4");
            string receiving_app = GetFieldOne("MSH-5");
            string receiving_fac = GetFieldOne("MSH-6");
            string msg_time = GetFieldOne("MSH-7");
            string control_id = GetFieldOne("MSH-10");
            string processing_id = GetFieldOne("MSH-11");
            string version_id = GetFieldOne("MSH-12");
            string ack = $"{START_BLOCK}MSH|^~\\&|{sending_app}|{sending_fac}|{receiving_app}|{receiving_fac}|{msg_time}||ACK|{control_id}|{processing_id}|{version_id}\rMSA|{ackType}|{control_id}{END_BLOCK}";
            return ack;
        }
    }
}
