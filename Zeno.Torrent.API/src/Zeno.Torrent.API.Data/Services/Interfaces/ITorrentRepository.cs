using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Core;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Data.Services.Interfaces {

    public interface ITorrentRepository {

        Task<bool> SaveAsync<T>(T entity, CancellationToken cancellationToken) where T : class, IEntity;
        Task<bool> DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T : class, IEntity;
        Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken) where T : class, IEntity;
        Task<T> GetAsync<T>(string id, CancellationToken cancellationToken) where T : class, IEntity;
        Task SaveChangesAsync(CancellationToken cancellationToken);
        ITransaction BeginTransaction();

        IQueryable<Show> Show { get; }
        IQueryable<T> Set<T>() where T : class, IEntity;
    }

}
