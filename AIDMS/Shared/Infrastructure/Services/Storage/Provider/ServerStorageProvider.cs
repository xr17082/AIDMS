using AIDMS.Shared.Application.Interfaces.Services.Storage.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDMS.Shared.Infrastructure.Services.Storage.Provider
{
    public class ServerStorageProvider : IStorageProvider
    {
        private Dictionary<string, string> _storage = new();

        public ServerStorageProvider(){}

        public void Clear() => _storage.Clear();

        public ValueTask ClearAsync()
        {
            Clear();
            return ValueTask.CompletedTask;
        }

        public bool ContainsKey(string key)
        {
            return _storage.ContainsKey(key);
        }

        public ValueTask<bool> ContainsKeyAsync(string key)
        {
            return ValueTask.FromResult(ContainsKey(key));
        }

        public string GetItem(string key)
        {
            if (_storage.ContainsKey(key)) return _storage[key];
            return null;
        }

        public ValueTask<string> GetItemAsync(string key)
        {
            return ValueTask.FromResult(GetItem(key));
        }

        public string Key(int index)
        {
            if(_storage.Count <= index) return _storage.ElementAt(index).Key;
            return null;
        }

        public ValueTask<string> KeyAsync(int index)
        {
            return ValueTask.FromResult(Key(index));
        }

        public int Length()
        {
            return _storage.Count();
        }

        public ValueTask<int> LengthAsync()
        {
            return ValueTask.FromResult(_storage.Count());
        }

        public void RemoveItem(string key)
        {
            if(key is not null && _storage.ContainsKey(key)) _storage.Remove(key);
        }

        public ValueTask RemoveItemAsync(string key)
        {
            RemoveItem(key);
            return ValueTask.CompletedTask;
        }

        public void SetItem(string key, string value)
        {
            if(_storage.ContainsKey(key)) _storage[key] = value;
            else _storage.Add(key, value);
        }

        public ValueTask SetItemAsync(string key, string value)
        {
            SetItem(key, value);
            return ValueTask.CompletedTask;
        }
    }
}
