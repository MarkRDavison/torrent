using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Auth;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Service.Services {

    public class DownloadDefaulter : IEntityDefaulter<Download> {

        public async Task DefaultAsync(Download entity, User user) {
            await Task.CompletedTask;

            if (string.IsNullOrEmpty(entity.CreatedByUserId) || entity.CreatedByUserId == Guid.Empty.ToString()) {
                entity.CreatedByUserId = user.Sub;
            }

            if (MonoTorrent.MagnetLink.TryParse(entity.OriginalUri, out var magnet)) {                
                if (string.IsNullOrEmpty(entity.Hash)) {
                    entity.Hash = magnet.InfoHash.ToHex();
                }

                if (string.IsNullOrEmpty(entity.Name)) {
                    entity.Name = magnet.Name;
                }
            }

            if (string.IsNullOrEmpty(entity.Name)) {
                entity.Name = "<<<PENDING>>>";
            }

            if (string.IsNullOrEmpty(entity.Source)) {
                entity.Source = Constants.DownloadSource.Manual;
            }

            if (string.IsNullOrEmpty(entity.State)) {
                entity.State = Constants.DownloadState.Added;
            }
        }

    }

}
