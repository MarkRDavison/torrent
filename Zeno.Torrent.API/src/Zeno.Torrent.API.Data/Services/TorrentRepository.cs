using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Core;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Data.Services.Interfaces;

namespace Zeno.Torrent.API.Data.Services {
    /// <summary>
    /// Implementation of the torrent  repository
    /// </summary>
    public class TorrentRepository : ITorrentRepository {

        private readonly TorrentDbContext context;

        /// <summary>
        /// Creates a new instance of the <see cref="TorrentRepository"/> class.
        /// </summary>
        public TorrentRepository(TorrentDbContext context) {
            this.context = context;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveAsync<T>(T entity, CancellationToken cancellationToken) where T : class, IEntity {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }
            if (entity.Id == Guid.Empty) {
                var result = await context.Set<T>().AddAsync(entity, cancellationToken);
                return result.State == Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else {
                var result = context.Set<T>().Attach(entity);
                result.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                return true;// result.State == Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T : class, IEntity {
            var result = await Task.Run(() => context.Set<T>().Remove(entity), cancellationToken);
            return result.State == Microsoft.EntityFrameworkCore.EntityState.Deleted;
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken) where T : class, IEntity {
            return await context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string id, CancellationToken cancellationToken) where T : class, IEntity {
            Guid guid;
            if (!Guid.TryParse(id, out guid)) {
                return null;
            }
            return await GetAsync<T>(guid, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync(CancellationToken cancellationToken) {
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public ITransaction BeginTransaction() {
            return new Transaction(context.Database.BeginTransaction(), context);
        }

        /// <inheritdoc/>
        public IQueryable<Show> Show => context.Shows;

        /// <inheritdoc/>
        public IQueryable<T> Set<T>() where T : class, IEntity => context.Set<T>();

    }
}
