using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7.Tea.Core
{
    public class Mapper
    {
        /// <summary>
        /// Applies direct mapping from src to dst
        /// </summary>
        public static void DirectMap(HL7Message src, HL7Message dst, List<string> paths)
        {
            var groupedPaths = new Dictionary<string, List<string>>();

            // Group unique paths by segment (first 3 chars)
            foreach (var path in paths.Distinct())
            {
                var s = path.Substring(0, 3);

                if (groupedPaths.ContainsKey(s))
                    groupedPaths[s].Add(path);
                else
                    groupedPaths[s] = new List<string> { path };
            }

            foreach (var seg in src.Segments)
            {
                if (!groupedPaths.ContainsKey(seg.Name))
                    continue;

                var newSeg = new Segment(seg.Name, new List<string> { "" });

                foreach (var path in groupedPaths[seg.Name])
                {
                    if (path.Length == 3)
                    {
                        newSeg = new Segment(seg.Name, new List<string>(seg.Fields));
                    }
                    else
                    {
                        newSeg.SetField(path, seg.GetFieldAll(path));
                    }
                }

                dst.Segments.Add(newSeg);
            }
        }

        /// <summary>
        /// Performs a conditional mapping from src to dst with condition function
        /// </summary>
        public static void ConditionalMap(
            HL7Message src,
            HL7Message dst,
            string path,
            Func<object, bool> condition)
        {
            // Segment mapping
            if (path.Length == 3)
            {
                foreach (var seg in src.Segments)
                {
                    if (seg.Name == path && condition(seg))
                    {
                        dst.Segments.Add(new Segment(seg.Name, new List<string>(seg.Fields)));
                    }
                }
            }
            else // Field mapping
            {
                var toBeMapped = new List<string>();

                foreach (var field in src.GetFieldAll(path))
                {
                    if (condition(field))
                    {
                        toBeMapped.Add(field);
                    }
                }

                if (toBeMapped.Count > 0)
                {
                    dst.SetField(path, string.Join("~", toBeMapped));
                }
            }
        }
    }
}
