using System.Collections.Generic;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services {
    public class ShowValidator : EntityValidator<Show> {

        public override IEnumerable<string> Validate(Show entity) {
            var errors = new List<string>();

            ValidateRequired(entity.Name, nameof(Show.Name), errors);
            ValidateRequired(entity.CreatedByUserId, nameof(Show.CreatedByUserId), errors);
            if (ValidateRequired(entity.Quality, nameof(Show.Quality), errors)) {
                ValidateOneOf(entity.Quality, nameof(Show.Quality), Constants.Quality.All, errors);
            }

            return errors;
        }
    }
}
