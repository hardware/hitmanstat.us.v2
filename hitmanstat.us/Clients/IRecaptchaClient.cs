using System.Net;
using System.Threading.Tasks;

namespace hitmanstat.us.Clients
{
    public interface IRecaptchaClient
    {
        Task<bool> Validate(string token, IPAddress address);
    }
}
