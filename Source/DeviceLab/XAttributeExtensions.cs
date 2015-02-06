using System;
using System.Xml.Linq;

namespace InfoSpace.DeviceLab
{
    public static class XAttributeExtensions
    {
        public static int GetValueAsInt(this XAttribute source)
        {
            return GetValueAsInt(source, 0);
        }

        public static int GetValueAsInt(this XAttribute source, int defaultValue)
        {
            int value;
            if (source != null && int.TryParse(source.Value, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static string GetValueOrDefault(this XAttribute source)
        {
            return source == null ? null : source.Value;
        }
    }
}
