using System.Diagnostics.CodeAnalysis;

namespace AIDMS.Shared.Infrastructure.Services.Storage
{
    [ExcludeFromCodeCoverage]
    public class ChangingEventArgs : ChangedEventArgs
    {
        public bool Cancel { get; set; }
    }
}
