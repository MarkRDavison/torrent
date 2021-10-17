using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Auth;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Service.Services {

    public class ShowDefaulter : IEntityDefaulter<Show> {
        public async Task DefaultAsync(Show entity, User user) {
            await Task.CompletedTask;

            if (string.IsNullOrEmpty(entity.CreatedByUserId) || entity.CreatedByUserId == Guid.Empty.ToString()) {
                entity.CreatedByUserId = user.Sub;
            }

        }
    }

}
