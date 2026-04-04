namespace HL7.Tea.core
{
    public class Helpers
    {
        public static string GetSubfield(string field, int index)
        {
            var parts = field.Split('^');
            return index > 0 && index <= parts.Length ? parts[index - 1] : "";
        }
    }
}
