using AIDMS.Shared.Application.Interfaces.Serialization;
using Newtonsoft.Json;

namespace AIDMS.Shared.Application.Serialization.Settings
{
    public class NewtonsoftJsonSettings : IJsonSerializerSettings
    {
        public JsonSerializerSettings JsonSerializerSettings { get; } = new() { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
    }
}
