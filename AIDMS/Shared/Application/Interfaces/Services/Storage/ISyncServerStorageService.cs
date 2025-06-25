using AIDMS.Shared.Infrastructure.Services.Storage;
using System;

namespace AIDMS.Shared.Application.Interfaces.Services.Storage
{
    public interface ISyncServerStorageService
    {
        void Clear();
        T GetItem<T>(string key);
        string GetItemAsString(string key);
        string Key(int index);
        bool ContainsKey(string key);
        int Length();
        void RemoveItem(string key);
        void SetItem<T>(string key, T value);
        void SetItemAsString(string key, string value);

        event EventHandler<ChangingEventArgs> Changing;
        event EventHandler<ChangedEventArgs> Changed;
    }
}
