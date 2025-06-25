using System.Text.Json;

namespace AIDMS.Shared.Application.Interfaces.Serialization
{
    public interface IJsonSerializerOptions
    {
        /// <summary>
        /// Options for <see cref="System.Text.Json"/>.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get;}
    }
}
