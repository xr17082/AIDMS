using AIDMS.Shared.Infrastructure.Services.Storage;
using System;
using System.Threading.Tasks;


namespace AIDMS.Shared.Application.Interfaces.Services.Storage
{
    public interface IServerStorageService
    {
        ValueTask ClearAsync();
        ValueTask<T> GetItemAsync<T>(string key);
        ValueTask<string> GetItemAsStringAsync(string key);
        ValueTask<string> KeyAsync(int index);
        ValueTask<bool> ContainsKeyAsync(string key);
        ValueTask<int> LengthAsync();
        ValueTask RemoveItemAsync(string key);
        ValueTask SetItemAsync<T>(string key, T data);
        ValueTask SetItemAsStringAsync(string key, string data);

        event EventHandler<ChangingEventArgs> Changing;
        event EventHandler<ChangedEventArgs> Changed;
    }
}
