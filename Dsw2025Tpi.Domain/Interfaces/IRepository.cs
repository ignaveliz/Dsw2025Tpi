using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Domain.Interfaces;

public interface IRepository
{
    Task<T?> GetById<T>(Guid id, params string[] include) where T : class, IEntityBase;
    Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : class, IEntityBase;
    Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : class, IEntityBase;
    Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : class, IEntityBase;
    Task<T> Add<T>(T entity) where T : class, IEntityBase;
    Task<T> Update<T>(T entity) where T : class, IEntityBase;
    Task<T> Delete<T>(T entity) where T : class, IEntityBase;
}
