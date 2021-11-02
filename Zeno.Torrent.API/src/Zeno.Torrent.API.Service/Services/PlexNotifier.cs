﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Service.Services {

    public struct PlexSection {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public int Key { get; set; }
    }

    public static class PlexSectionType {
        public const string MOVIE = "movie";
        public const string TVSHOW = "show";
        public const string MUSIC = "artist";
    }

    public class PlexNotifier : IPlexNotifier {

        private IHttpClientFactory httpClientFactory;
        private readonly ILogger logger;
        private readonly IOptions<AppSettings> options;

        public PlexNotifier(ILogger<PlexNotifier> logger, IHttpClientFactory httpClientFactory, IOptions<AppSettings> options) {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.options = options;
        }

        public async Task<IDictionary<string, PlexSection>> FetchSectionIds() {
            using (var client = httpClientFactory.CreateClient("PlexNotifier")) {
                var uri = $"{options.Value.PLEX_URL}/library/sections?X-Plex-Token={options.Value.PLEX_TOKEN}";

                var message = new HttpRequestMessage {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri)
                };

                var response = await client.SendAsync(message);
                var content = await response.Content.ReadAsStringAsync();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);

                var results = new Dictionary<string, PlexSection>();

                foreach (var node in doc.SelectNodes("/MediaContainer/Directory")) {
                    if (node is XmlNode n) {
                        var key = n.Attributes.GetNamedItem("key").Value;
                        var type = n.Attributes.GetNamedItem("type").Value;
                        var title = n.Attributes.GetNamedItem("title").Value;
                        var location = n.SelectSingleNode("Location");
                        var path = location.Attributes.GetNamedItem("path").Value;

                        var section = new PlexSection {
                            Key = int.Parse(key),
                            Path = path,
                            Title = title,
                            Type = type
                        };

                        results[type] = section;
                    }
                }

                return results;
            }
        }

        public async Task NotifyPlexScan(CompletedMedia media, PlexSection section, string path, CancellationToken cancellationToken) {
            using (var client = httpClientFactory.CreateClient("PlexNotifier")) {
                var uri = $"{options.Value.PLEX_URL}/library/sections/{section.Key}/refresh?X-Plex-Token={options.Value.PLEX_TOKEN}&path={HttpUtility.UrlEncode(path)}";

                var message = new HttpRequestMessage {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri)
                };

                await client.SendAsync(message);
            }
        }

        public async Task NotifyCompletedMedia(CompletedMedia media, CancellationToken cancellationToken) {
            var sections = await FetchSectionIds();
            PlexSection section;
            string path;
            switch (media.DownloadType) {
                case Constants.DownloadType.Movie:
                    section = sections[PlexSectionType.MOVIE];
                    path = section.Path;
                    break;
                case Constants.DownloadType.Season:
                case Constants.DownloadType.Episode:
                    section = sections[PlexSectionType.TVSHOW];
                    path = section.Path + "/" + media.Show.Name;
                    break;
                default:
                    return;
            }

            await NotifyPlexScan(media, section, path, cancellationToken);
        }
    }

}