using System.Collections.Generic;

namespace HL7.Tea.Core
{
    internal class OrGroup: FilterNode
    {
        public List<FilterNode> Or { get; set; }
    }
}
