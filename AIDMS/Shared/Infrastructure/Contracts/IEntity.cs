namespace AIDMS.Shared.Infrastructure.Contracts
{
    public interface IEntity<TId> : IEntity
    {
        public TId Id { get; set; }
    }
    public interface IEntity
    {
    }
}
