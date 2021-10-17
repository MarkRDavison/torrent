using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Zeno.Torrent.API.Data.Core {
    public interface ITransaction : IDisposable {

        Task CommitAsync(CancellationToken cancellationToken);

        Task RollbackAsync(CancellationToken cancellationToken);

    }

    public class Transaction : ITransaction {

        private readonly IDbContextTransaction transaction;
        private readonly DbContext context;

        internal Transaction(IDbContextTransaction transaction, DbContext context) {
            this.transaction = transaction;
            this.context = context;
        }

        public async Task CommitAsync(CancellationToken cancellationToken) {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken) {
            await transaction.RollbackAsync(cancellationToken);
        }

        public void Dispose() {
            transaction.Dispose();
        }
    }
}
