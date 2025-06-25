using AIDMS.Shared.Application.Interfaces.Serialization;
using MimeKit;
using System;
using System.Text.Json;

namespace AIDMS.Shared.Application.Serialization.Settings
{
    public class SystemTextJsonOptions : IJsonSerializerOptions
    {
        public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions { NewLine = Environment.NewLine };
    }
}
