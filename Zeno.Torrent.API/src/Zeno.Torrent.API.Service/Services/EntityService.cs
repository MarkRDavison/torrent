using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Data.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Zeno.Torrent.API.Service.Services {

    public class EntityService<T> : IEntityService<T> where T : class, IEntity {

        private readonly ILogger logger;
        private readonly ITorrentRepository repository;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public EntityService(
            ILogger<EntityService<T>> logger,
            ITorrentRepository repository,
            IServiceScopeFactory serviceScopeFactory
        ) {
            this.logger = logger;
            this.repository = repository;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IEnumerable<T>> GetEntitiesAsync(CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                return await repository.Set<T>().ToListAsync();
            }
        }


        public async Task<T> GetEntityAsync(string id, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                return await repository.GetAsync<T>(id, cancellationToken);
            }
        }

        public async Task<T> GetEntityAsync(Guid id, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                return await repository.GetAsync<T>(id, cancellationToken);
            }
        }

        public async Task<T> SaveEntityAsync(T entity, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var repository = scope.ServiceProvider.GetRequiredService<ITorrentRepository>();
                    var validator = scope.ServiceProvider.GetRequiredService<IEntityValidator<T>>();
                    var errors = validator.Validate(entity);
                    if (errors.Any()) {
                        throw new AggregateException($"Validate {typeof(T).Name} failed", errors.Select(e => new Exception(e)));
                    }

                    using (var transaction = repository.BeginTransaction()) {
                        if (!await repository.SaveAsync(entity, cancellationToken)) {
                            await transaction.RollbackAsync(cancellationToken);
                            return null;
                        }
                        await transaction.CommitAsync(cancellationToken);
                        return await repository.GetAsync<T>(entity.Id, cancellationToken);
                    }
                }
            }
        }

        public async Task<bool> DeleteEntityAsync(T entity, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                using (var transaction = repository.BeginTransaction()) {
                    if (!await repository.DeleteAsync(entity, cancellationToken)) {
                        await transaction.RollbackAsync(cancellationToken);
                        return false;
                    }
                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }
            }
        }

        public async Task DeleteAllEntitiesAsync(CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                using (var transaction = repository.BeginTransaction()) {
                    var entities = await repository.Set<T>().ToListAsync();
                    foreach (var e in entities) {
                        await repository.DeleteAsync(e, cancellationToken);
                    }
                    await transaction.CommitAsync(cancellationToken);                    
                }
            }
        }

        public IQueryable<T> Entities => repository.Set<T>().AsNoTracking().AsQueryable();
    }
}
