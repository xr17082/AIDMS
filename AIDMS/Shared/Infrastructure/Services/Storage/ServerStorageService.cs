using AIDMS.Shared.Application.Interfaces.Serialization;
using AIDMS.Shared.Application.Interfaces.Services.Storage;
using AIDMS.Shared.Application.Interfaces.Services.Storage.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIDMS.Shared.Infrastructure.Services.Storage
{
    internal class ServerStorageService : IServerStorageService, ISyncServerStorageService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IJsonSerializer _serializer;

        public ServerStorageService(IStorageProvider storageProvider, IJsonSerializer serializer)
        {
            _serializer = serializer;
            _storageProvider = storageProvider;
        }

        public event EventHandler<ChangingEventArgs> Changing;
        public event EventHandler<ChangedEventArgs> Changed;

        public async ValueTask SetItemAsync<T>(string key, T value)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            
            var e = await RaiseOnChangingAsync(key, value).ConfigureAwait(false);

            if (e.Cancel)
                return;

            var serializedData = _serializer.Serialize(value);
            await _storageProvider.SetItemAsync(key, serializedData).ConfigureAwait(false);

            RaiseOnChanged(key, e.OldValue, value);
        }
        
        public async ValueTask SetItemAsStringAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if(value is null)
                throw new ArgumentNullException(nameof(value));

            var e = await RaiseOnChangingAsync(key, value).ConfigureAwait(false);

            if (e.Cancel)
                return;

            await _storageProvider.SetItemAsync(key, value).ConfigureAwait(false);
            RaiseOnChanged(key, e.OldValue, value);
        }

        public async ValueTask<T> GetItemAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var serializedData = await _storageProvider.GetItemAsync(key).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(serializedData))
                return default;

            try
            {
                return _serializer.Deserialize<T>(serializedData);
            }
            catch (JsonException e) when (e.Path == "$" && typeof(T) == typeof(string))
            {
                return (T)(object)serializedData;
            }
        }

        public ValueTask<string> GetItemAsStringAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return _storageProvider.GetItemAsync(key);
        }

        public ValueTask RemoveItemAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return _storageProvider.RemoveItemAsync(key);
        }

        public ValueTask ClearAsync() => _storageProvider.ClearAsync();
        public ValueTask<int> LengthAsync() => _storageProvider.LengthAsync();
        public ValueTask<string> KeyAsync(int index) => _storageProvider.KeyAsync(index);
        public ValueTask<bool> ContainsKeyAsync(string key) => _storageProvider.ContainsKeyAsync(key);

        public void SetItem<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var e = RaiseOnChangingSync(key, value);

            if (e.Cancel)
                return;

            var serializedData = _serializer.Serialize(value);

            RaiseOnChanged(key, e.OldValue, value);
        }

        public void SetItemAsString(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if(value is null)
                throw new ArgumentNullException(nameof(value));

            var e = RaiseOnChangingSync(key, value);

            if(e.Cancel)
                return ;

            _storageProvider.SetItem(key, value);

            RaiseOnChanged(key, e.OldValue, value);
        }

        public T GetItem<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var serializedData = _storageProvider.GetItem(key);

            if (string.IsNullOrWhiteSpace(serializedData))
                return default;

            try
            {
                return _serializer.Deserialize<T>(serializedData);
            }
            catch (JsonException e) when (e.Path == "$" && typeof(T) == typeof(string))
            {
                return (T)(object)serializedData;
            }
        }

        public string GetItemAsString(string key)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return _storageProvider.GetItem(key);
        }

        public void RemoveItem(string key)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            _storageProvider.RemoveItem(key);
        }

        public void Clear() => _storageProvider.Clear();

        public int Length() => _storageProvider.Length();

        public string Key(int index) => _storageProvider.Key(index);

        public bool ContainsKey(string key) => _storageProvider.ContainsKey(key);




        private async Task<ChangingEventArgs> RaiseOnChangingAsync(string key, object value)
        {
            var e = new ChangingEventArgs
            {
                Key = key,
                OldValue = await GetItemInternalAsync<object>(key).ConfigureAwait(false),
                NewValue = value
            };

            Changing?.Invoke(this, e);

            return e;
        }

        private ChangingEventArgs RaiseOnChangingSync(string key, object data)
        {
            var e = new ChangingEventArgs
            {
                Key = key,
                OldValue = GetItemInternal(key),
                NewValue = data
            };

            Changing?.Invoke(this, e);
            return e;
        }

        private void RaiseOnChanged(string key, object oldValue, object data)
        {
            var e = new ChangedEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = data
            };

            Changed?.Invoke(this, e);
        }

        private async Task<T> GetItemInternalAsync<T>(string key)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentException(nameof(key));

            var serializedData = await _storageProvider.GetItemAsync(key).ConfigureAwait(false);

            if (string.IsNullOrEmpty(serializedData))
                return default;

            try
            {
                return _serializer.Deserialize<T>(serializedData);
            }
            catch (JsonException)
            {
                return (T)(object)serializedData;
            }
        }

        private object GetItemInternal(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var serializedData = _storageProvider.GetItem(key);

            if (string.IsNullOrWhiteSpace(serializedData))
                return default;

            try
            {
                return _serializer.Deserialize<object>(serializedData);
            }
            catch (JsonException)
            {
                return serializedData;
            }
        }
    }
}
