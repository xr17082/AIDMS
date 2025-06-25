using Newtonsoft.Json;

namespace AIDMS.Shared.Application.Interfaces.Serialization
{
    public interface IJsonSerializerSettings
    {
        /// <summary>
        /// Settings for <see cref="Newtonsoft.Json"/>.
        /// </summary
        public JsonSerializerSettings JsonSerializerSettings { get;}
    }
}
