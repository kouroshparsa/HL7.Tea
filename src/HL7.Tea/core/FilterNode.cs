using System.Text.Json.Serialization;

namespace HL7.Tea.Core
{
    [JsonConverter(typeof(FilterNodeConverter))]
    public abstract class FilterNode // no implementation because this is going to Condition, AndGroup, ...
    {
    }
}
