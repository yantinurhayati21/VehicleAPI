using System.Threading.Tasks;
using VehicleAPI.Models;

namespace VehicleAPI.Repository
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(int id);
        Task<User> AddUserAsync(User user);
    }
}
