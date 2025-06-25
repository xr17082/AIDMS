using AIDMS.Shared.Application.Interfaces.Serialization;
using AIDMS.Shared.Application.Serialization.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AIDMS.Shared.Application.Serialization.Serializers
{
    public class NewtonSoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public NewtonSoftJsonSerializer(IOptions<NewtonsoftJsonSettings> settings)
        {
            _settings = settings.Value.JsonSerializerSettings;
        }
        public T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json, _settings);

        public string Serialize<T>(T obj)
            => JsonConvert.SerializeObject(obj, _settings);
    }
}
