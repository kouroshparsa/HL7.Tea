using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7.Tea.Core
{
    internal class Condition: FilterNode
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value
        {
            get; set;
        }
    }
}
