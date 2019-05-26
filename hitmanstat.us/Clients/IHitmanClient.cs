using System.Threading.Tasks;
using hitmanstat.us.Models;

namespace hitmanstat.us.Clients
{
    public interface IHitmanClient
    {
        Task<EndpointStatus> GetStatus();
    }
}
