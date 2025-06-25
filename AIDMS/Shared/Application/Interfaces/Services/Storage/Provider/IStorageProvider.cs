using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDMS.Shared.Application.Interfaces.Services.Storage.Provider
{
    public interface IStorageProvider
    {
        void Clear();
        ValueTask ClearAsync();
        bool ContainsKey(string key);
        ValueTask<bool> ContainsKeyAsync(string key);
        string GetItem(string key);
        ValueTask<string> GetItemAsync(string key);
        string Key(int index);
        ValueTask<string> KeyAsync(int index);
        int Length();
        ValueTask<int> LengthAsync();
        void RemoveItem(string key);
        ValueTask RemoveItemAsync(string key);
        void SetItem(string key, string value);
        ValueTask SetItemAsync(string key, string value);
    }
}
