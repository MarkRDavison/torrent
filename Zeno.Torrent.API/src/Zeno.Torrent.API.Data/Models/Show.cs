using System;

namespace Zeno.Torrent.API.Data.Models {
    public class Show : IEntity {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Quality { get; set; }
        public string CreatedByUserId { get; set; }
    }
}
