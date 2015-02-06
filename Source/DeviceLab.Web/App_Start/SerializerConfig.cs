using System.Net.Http.Formatting;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Web
{
    public static class SerializerConfig
    {
        public static void RegisterSerializers(MediaTypeFormatterCollection formatters)
        {
            formatters.Remove(formatters.XmlFormatter);

            formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects
            };
        }
    }
}
