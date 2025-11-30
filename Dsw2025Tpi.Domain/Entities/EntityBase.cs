using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Domain.Entities;

public abstract class EntityBase : IEntityBase
{
    protected EntityBase()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; set; }
}
