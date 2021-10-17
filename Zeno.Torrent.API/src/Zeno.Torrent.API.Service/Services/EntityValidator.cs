using System.Collections.Generic;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Service.Services {
    public abstract class EntityValidator<T> : IEntityValidator<T> where T : class, IEntity {
        public abstract IEnumerable<string> Validate(T entity);

        protected bool ValidateRequired(string property, string name, List<string> errors) {
            if (string.IsNullOrEmpty(property)) {
                errors.Add($"{name} is required on {typeof(T).Name}");
                return false;
            }
            return true;
        }

        protected bool ValidateOneOf(string property, string name, List<string> options, List<string> errors) {
            if (!options.Contains(property)) {
                errors.Add($"{name} ({property}) is an invalid option on {typeof(T).Name}");
                return false;
            }
            return true;
        }

    }
}
