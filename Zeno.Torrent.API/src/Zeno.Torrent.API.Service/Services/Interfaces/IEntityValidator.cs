using System.Collections.Generic;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services.Interfaces {

    public interface IEntityValidator<T> where T : class, IEntity {
        IEnumerable<string> Validate(T entity);
    }

}
