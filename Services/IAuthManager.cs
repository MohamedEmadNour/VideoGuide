using VideoGuide.Data;
using VideoGuide.View_Model;
//using PhoneLogApi.View_Model;

namespace VideoGuide.Services
{
    public interface IAuthManager
    {
        Task<bool> ValidateUser(LoginUserDTO userDTO);
        Task<bool> ActiveUser(LoginUserDTO userDTO);
        Task<string> CreateToken(ApplicationUser user);
    }
}
