using System;
using System.Collections.Generic;
using System.Linq;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Core.Utility {

    public static class MovieFilenameOperations {

        internal const int MinYear = 1900;

        internal static string ExtractQuality(string name) {
            foreach (var q in Constants.Quality.All) {
                if (name.Contains(q, System.StringComparison.OrdinalIgnoreCase)) {
                    return q;
                }
            }
            return string.Empty;
        }

        internal static List<string> ExtractTokens(string name, string delimiters) {
            return name.Split(delimiters.ToCharArray()).ToList();
        }

        internal static string ExtractYear(string name, string quality) {
            var periodTokens = ExtractTokens(name, ". ");
            var bracketTokens = ExtractTokens(name, ".[](){} ");

            var periodQualityIndex = string.IsNullOrEmpty(quality)
                ? periodTokens.Count - 1
                : periodTokens.IndexOf(quality);
            var bracketQualityIndex = string.IsNullOrEmpty(quality)
                ? bracketTokens.Count - 1
                : bracketTokens.IndexOf(quality);

            if (periodQualityIndex != -1) {
                foreach (var token in periodTokens.Take(periodQualityIndex + 1)) {
                    if (int.TryParse(token, out var year)) {
                        if (MinYear <= year && year <= DateTime.Now.Year + 1) {
                            return token;
                        }
                    }
                }
            }
            if (bracketQualityIndex != -1) {
                foreach (var token in bracketTokens.Take(bracketQualityIndex + 1)) {
                    if (int.TryParse(token, out var year)) {
                        if (MinYear <= year && year <= DateTime.Now.Year + 1) {
                            return token;
                        }
                    }
                }
            }

            return string.Empty;
        }

        internal static string ExtractNameFromTokens(List<string> tokens, int qualityTokenIndex, int yearTokenIndex) {
            if (qualityTokenIndex != -1 &&
                yearTokenIndex != -1) {
                string displayName = string.Empty;
                foreach (var token in tokens
                    .Take(Math.Min(qualityTokenIndex, yearTokenIndex))
                    .Where(t => !string.IsNullOrEmpty(t))) {
                    if (!string.IsNullOrEmpty(displayName)) {
                        displayName += " ";
                    }
                    else if (int.TryParse(token, out var tok) && tok < MinYear) {
                        continue;
                    }
                    displayName += token;
                }
                return displayName;
            }

            return string.Empty;
        }

        internal static string ExtractName(string name, string quality, string year) {
            var periodTokens = ExtractTokens(name, ".- ");
            var bracketTokens = ExtractTokens(name, "[](){}-. ");

            var periodQualityIndex = string.IsNullOrEmpty(quality)
                ? periodTokens.Count - 1
                : periodTokens.IndexOf(quality);
            var periodYearIndex = string.IsNullOrEmpty(year)
                ? periodTokens.Count - 1
                : periodTokens.IndexOf(year);
            var bracketQualityIndex = string.IsNullOrEmpty(quality)
                ? bracketTokens.Count - 1
                : bracketTokens.IndexOf(quality);
            var bracketYearIndex = string.IsNullOrEmpty(year)
                ? bracketTokens.Count - 1
                : bracketTokens.IndexOf(year);

            var periodName = ExtractNameFromTokens(periodTokens, periodQualityIndex, periodYearIndex);
            if (!string.IsNullOrEmpty(periodName)) {
                if (periodName.Contains('/')) {
                    return periodName.Split('/').Last().Trim();
                }
                return periodName.Trim();
            }

            var bracketName = ExtractNameFromTokens(bracketTokens, bracketQualityIndex, bracketYearIndex);
            if (!string.IsNullOrEmpty(bracketName)) {
                if (bracketName.Contains('/')) {
                    return bracketName.Split('/').Last().Trim();
                }
                return bracketName.Trim();
            }

            return string.Empty;
        }

        public static MovieFilenameInfo ExtractInfo(string name) {
            var quality = ExtractQuality(name);
            var year = ExtractYear(name, quality);
            
            return new MovieFilenameInfo {
                Quality = quality,
                Year = string.IsNullOrEmpty(year) ? -1 : int.Parse(year),
                Name = string.IsNullOrEmpty(year) ? string.Empty : ExtractName(name, quality, year)
            };
        }

    }

}
