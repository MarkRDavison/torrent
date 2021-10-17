using System.Security.Claims;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services.Interfaces {

    public interface IEntityDefaulter<T> where T : class, IEntity {

        Task DefaultAsync(T entity, User user);

    }

}
