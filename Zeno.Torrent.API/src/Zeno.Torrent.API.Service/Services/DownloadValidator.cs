using System.Collections.Generic;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services {

    public class DownloadValidator : EntityValidator<Download> {

        public override IEnumerable<string> Validate(Download entity) {
            var errors = new List<string>();

            ValidateRequired(entity.Name, nameof(Download.Name), errors);
            ValidateRequired(entity.CreatedByUserId, nameof(Download.CreatedByUserId), errors);
            ValidateRequired(entity.OriginalUri, nameof(Download.OriginalUri), errors);
            if (ValidateRequired(entity.DownloadType, nameof(Download.DownloadType), errors)) {
                ValidateOneOf(entity.DownloadType, nameof(Download.DownloadType), Constants.DownloadType.All, errors);
            }
            if (ValidateRequired(entity.Source, nameof(Download.Source), errors)) {
                ValidateOneOf(entity.Source, nameof(Download.Source), Constants.DownloadSource.All, errors);
            }
            if (ValidateRequired(entity.State, nameof(Download.State), errors)) {
                ValidateOneOf(entity.State, nameof(Download.State), Constants.DownloadState.All, errors);
            }


            return errors;
        }

    }

}
