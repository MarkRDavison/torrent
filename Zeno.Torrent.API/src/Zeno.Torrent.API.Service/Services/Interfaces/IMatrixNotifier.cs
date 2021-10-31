using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services.Interfaces {

    public class MatrixUser {
        public string user_id { get; set; }
        public string access_token { get; set; }
        public string home_server { get; set; }
        public string device_id { get; set; }
    }
    public interface IMatrixNotifier : IMediaNotifier {

        Task<MatrixUser> LoginUser();

        Task<bool> SendText(MatrixUser user, string message);

        Task<string> GenerateMessage(CompletedMedia completedMedia);

    }

}
