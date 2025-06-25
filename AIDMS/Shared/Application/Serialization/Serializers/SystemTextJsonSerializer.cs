using AIDMS.Shared.Application.Interfaces.Serialization;
using AIDMS.Shared.Application.Serialization.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AIDMS.Shared.Application.Serialization.Serializers
{
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public SystemTextJsonSerializer(IOptions<SystemTextJsonOptions> options)
        {
            _options = options.Value.JsonSerializerOptions;
        }
        public T Deserialize<T>(string json)
            => JsonSerializer.Deserialize<T>(json, _options);

        public string Serialize<T>(T data)
            => JsonSerializer.Serialize(data, _options);
    }
}
