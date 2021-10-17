using System;

namespace Zeno.Torrent.API.Data.Models {
    public interface IEntity {
        public Guid Id { get; set; }
    }
}
