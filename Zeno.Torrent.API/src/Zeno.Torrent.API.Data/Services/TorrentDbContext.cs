using Microsoft.EntityFrameworkCore;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Data.Services {
    public class TorrentDbContext : DbContext {

        public const string SQLITE = "sqlite";
        public const string POSTGRES = "postgres";

        /// <summary>
        /// Creates a new instance of the <see cref="TorrentDbContext"/> class.
        /// </summary>
        public TorrentDbContext(DbContextOptions options) : base(options) {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            #region Show
            modelBuilder
                .Entity<Show>()
                .HasKey(e => e.Id);
            modelBuilder
                .Entity<Show>()
                .Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            modelBuilder
                .Entity<Show>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder
                .Entity<Show>()
                .Property(e => e.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(255);
            #endregion

            #region Episode
            modelBuilder
                .Entity<Episode>()
                .HasKey(e => e.Id);
            modelBuilder
                .Entity<Episode>()
                .Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            modelBuilder
                .Entity<Episode>()
                .HasOne<Show>()
                .WithMany()
                .HasForeignKey(e => e.ShowId);
            modelBuilder
                .Entity<Episode>()
                .Property(e => e.SeasonNumber)
                .IsRequired();
            modelBuilder
                .Entity<Episode>()
                .Property(e => e.EpisodeNumber)
                .IsRequired();
            modelBuilder
                .Entity<Episode>()
                .Property(e => e.Repack)
                .IsRequired();
            modelBuilder
                .Entity<Episode>()
                .Property(e => e.DownloadId);
            #endregion

            #region Torrent
            modelBuilder
                .Entity<Download>()
                .HasKey(e => e.Id);
            modelBuilder
                .Entity<Download>()
                .Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            modelBuilder
                .Entity<Download>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder
                .Entity<Download>()
                .Property(e => e.Hash)
                .HasMaxLength(255);
            modelBuilder
                .Entity<Download>()
                .Property(e => e.TorrentLocation)
                .HasMaxLength(255);
            modelBuilder
                .Entity<Download>()
                .Property(e => e.OriginalUri)
                .HasMaxLength(4096);
            modelBuilder
                .Entity<Download>()
                .Property(e => e.State)
                .HasMaxLength(4);
            modelBuilder
                .Entity<Download>()
                .Property(e => e.DownloadType)
                .HasMaxLength(3)
                .IsRequired();
            modelBuilder
                .Entity<Download>()
                .Property(e => e.DestinationTypeId)
                .IsRequired();
            modelBuilder
                .Entity<Download>()
                .Property(e => e.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(255);
            #endregion
        }

        public DbSet<Show> Shows { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Download> Downloads { get; set; }

    }
}
