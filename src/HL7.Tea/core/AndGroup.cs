using System.Collections.Generic;

namespace HL7.Tea.Core
{
    internal class AndGroup: FilterNode
    {
        public List<FilterNode> And { get; set; }
    }
}
