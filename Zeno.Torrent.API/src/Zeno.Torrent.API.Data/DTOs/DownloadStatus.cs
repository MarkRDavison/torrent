using System;
using System.Collections.Generic;

namespace Zeno.Torrent.API.Data.DTOs {

    public struct PeerInfo {
        public string ConnectionString { get; set; }
        public string Client { get; set; }
        public long DownloadSpeed { get; set; }
        public long DownloadTotal { get; set; }
        public long UploadSpeed { get; set; }
        public long UploadTotal { get; set; }
    }

    public struct DownloadStatus {
        public Guid Id { get; set; }
        public string State { get; set; }
        public double Percentage { get; set; }
        public double DownloadSpeed { get; set; }
        public double UploadSpeed { get; set; }
        public long Size { get; set; }

        public List<PeerInfo> PeerInfo { get; set; }
    }

}
