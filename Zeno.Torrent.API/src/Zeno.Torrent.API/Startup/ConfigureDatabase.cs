using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Data.Services;

namespace Zeno.Torrent.API {
    
    public static class ConfigureDatabase {

        public static void ConfigureDatabaseServices(
            this IServiceCollection services
        ) {

            services.AddTransient(p => {
                var appSettings = p.GetRequiredService<IOptions<AppSettings>>();
                var o = new DbContextOptionsBuilder<TorrentDbContext>();
                switch (appSettings.Value.DATABASE_TYPE) {
                    case TorrentDbContext.SQLITE: 
                        ConfigureSqlite(o, appSettings.Value);
                        break;
                    case TorrentDbContext.POSTGRES:
                        ConfigurePostgres(o, appSettings.Value);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid database type");
                }
                return new TorrentDbContext(o.Options);
            });
        }

        internal static void ConfigureSqlite(
            DbContextOptionsBuilder o,
            AppSettings appSettings
        ) {
            o.UseSqlite(appSettings.CONNECTION_STRING);
        }

        internal static void ConfigurePostgres(
            DbContextOptionsBuilder o,
            AppSettings appSettings
        ) {
            var b = new NpgsqlConnectionStringBuilder {
                Host = appSettings.DATABASE_HOST,
                Port = int.Parse(appSettings.DATABASE_PORT),
                Username = appSettings.DATABASE_USER,
                Password = appSettings.DATABASE_PASSWORD,
                Database = appSettings.DATABASE_NAME
            };
            o.UseNpgsql(b.ToString());
        }

    }

}
