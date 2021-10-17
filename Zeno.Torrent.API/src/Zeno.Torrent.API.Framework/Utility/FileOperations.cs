using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Framework.Instrumentation;

namespace Zeno.Torrent.API.Framework.Utility {

    public interface IFileOperations {
        bool FileExists(string file);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        void CopyFile(string source, string destination);
        void CopyDirectory(string source, string destination);
        void DeleteDirectory(string path);
        void DeleteFile(string file);
        string[] GetDirectoryContents(string path);
    }

    public class FileOperations : IFileOperations {

        private readonly ILogger logger;

        public FileOperations(ILogger<FileOperations> logger) {
            this.logger = logger;
        }

        public bool FileExists(string file) {
            return File.Exists(file);
        }
        public bool DirectoryExists(string path) {
            return Directory.Exists(path);
        }
        public void CreateDirectory(string path) {
            Directory.CreateDirectory(path);
        }
        public void CopyFile(string source, string destination) {
            using (logger.ProfileOperation()) {
                File.Copy(source, destination, true);
            }
        }
        public void CopyDirectory(string source, string destination) {
            using (logger.ProfileOperation()) {
                foreach (var file in Directory.GetFiles(source)) {
                    var filename = Path.GetFileName(file);
                    CopyFile(file, Path.Combine(destination, filename));
                }
            }
        }
        public void DeleteDirectory(string path) {
            Directory.Delete(path, true);
        }
        public void DeleteFile(string file) {
            File.Delete(file);
        }
        public string[] GetDirectoryContents(string path) {
            return Directory.EnumerateFiles(path).ToArray();
        }
    }
}
