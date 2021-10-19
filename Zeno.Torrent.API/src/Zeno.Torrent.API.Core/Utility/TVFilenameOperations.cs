using System;
using System.Linq;
using System.Text.RegularExpressions;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Core.Utility {

    public struct TVFilenameInfo {
        public bool Valid =>
            Season > 0 &&
            Episode > 0;
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Quality { get; set; }
        public bool Repack { get; set; }
    }

    public static class TVFilenameOperations {

        public static string CreateFileName(Show show, TVFilenameInfo info, string extension) {
            var tokens = show.Name.Split(' ').Select(s => {
                return char.ToUpper(s.First()) + s.Substring(1).ToLower();
            }).ToList();
            tokens.Add($"S{string.Format("{0:00}", info.Season)}E{string.Format("{0:00}", info.Episode)}");
            tokens.Add(info.Quality);
            if (info.Repack) {
                tokens.Add("REPACK");
            }
            if (extension[0] == '.') {
                tokens.Add(extension.Substring(1));
            }
            else {
                tokens.Add(extension);
            }
            return string.Join(".", tokens);
        }

        public static bool MatchSxEyFormat(string filename, out int season, out int episode) {
            var regex = new Regex(@"S(?<season>\d{1,2})E(?<episode>\d{1,2})", RegexOptions.IgnoreCase);
            var match = regex.Match(filename);

            season = 0;
            episode = 0;

            if (match.Success &&
                int.TryParse(match.Groups["season"].Value, out season) &&
                int.TryParse(match.Groups["episode"].Value, out episode)) {
                return true;
            }

            return false;
        }

        public static bool MatchSxEFormat(string filename, out int season, out int episode) {
            var regex = new Regex(@"(?<season>\d{1,2})x(?<episode>\d{1,2})", RegexOptions.IgnoreCase);
            var match = regex.Match(filename);

            season = 0;
            episode = 0;

            if (match.Success &&
                int.TryParse(match.Groups["season"].Value, out season) &&
                int.TryParse(match.Groups["episode"].Value, out episode)) {
                return true;
            }

            return false;
        }

        public static string GetQuality(string filename) {

            foreach (var q in Constants.Quality.All) {
                if (filename.Contains(q, StringComparison.InvariantCultureIgnoreCase)) {
                    return q;
                }
            }

            return string.Empty;
        }

        public static bool GetSample(string filename) {
            var segments = filename.Split('/', ',', '.', '-');
            return segments.Any(s => s.Equals("sample", StringComparison.OrdinalIgnoreCase));
        }

        public static TVFilenameInfo ExtractInfo(string name) {
            int season;
            int episode;
            bool repack = name.Contains("REPACK", StringComparison.InvariantCultureIgnoreCase);
            string quality = GetQuality(name);

            if (GetSample(name)) {
                return new TVFilenameInfo { };
            }

            if (MatchSxEyFormat(name, out season, out episode) ||
                MatchSxEFormat(name, out season, out episode)) {
                return new TVFilenameInfo {
                    Season = season,
                    Episode = episode,
                    Repack = repack,
                    Quality = string.IsNullOrEmpty(quality) ? Constants.Quality.HD_720p : quality
                };
            }
            return new TVFilenameInfo { };
        }
    }
}
