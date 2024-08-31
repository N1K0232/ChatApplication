using System.Globalization;

namespace System.Reflection;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class BuildDateTimeAttribute(string value) : Attribute
{
    public DateTime DateTime
    {
        get
        {
            return DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }
    }
}