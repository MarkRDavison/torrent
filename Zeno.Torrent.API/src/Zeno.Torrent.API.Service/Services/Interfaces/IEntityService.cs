using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services.Interfaces {

    public interface IEntityService<T> where T : class, IEntity {

        Task<IEnumerable<T>> GetEntitiesAsync(CancellationToken cancellationToken);
        Task<T> GetEntityAsync(string id, CancellationToken cancellationToken);
        Task<T> GetEntityAsync(Guid id, CancellationToken cancellationToken);
        Task<T> SaveEntityAsync(T entity, CancellationToken cancellationToken);
        Task<bool> DeleteEntityAsync(T entity, CancellationToken cancellationToken);
        Task DeleteAllEntitiesAsync(CancellationToken cancellationToken);
        IQueryable<T> Entities { get; }

    }

}
