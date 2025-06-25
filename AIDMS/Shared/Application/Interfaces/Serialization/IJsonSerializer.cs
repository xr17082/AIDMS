﻿namespace AIDMS.Shared.Application.Interfaces.Serialization
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json);
    }
}
