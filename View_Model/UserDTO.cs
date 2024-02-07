using System.ComponentModel.DataAnnotations;

namespace VideoGuide.View_Model
{
    public class LoginUserDTO
    {
        [Required]
        public string UserName { get; set; } = "";
        //[Required]
        //[StringLength(15, ErrorMessage = "Your Password is limited to {2} to {1} characters", MinimumLength = 6)]
        public string Password { get; set; } = "";
    }
    public class UserDTO : LoginUserDTO
    {
        [Required]
        public string FullName { get; set; } = "";
        [Required]
        public ICollection<string> Roles { get; set; } = new List<string>();
        public List<listGroupID> listGroupID { get; set; } = new List<listGroupID>();

    }
    public class ChangePassword : LoginUserDTO
    {
        [Required]
        public string NewPassword { get; set; } = "";
    }
    public class RestPassword
    {
        [Required]
        public string UserName { get; set; } = "";
        [Required]
        public string Password { get; set; } = "";
    }
    public class DeleteUser
    {
        [Required]
        public string UserName { get; set; } = "";
        
    }
    public class UpdateUser 
    {
        [Required]
        public string UserName { get; set; } = "";
        [Required]
        public string FullName { get; set; } = "";
        public bool Active { get; set; } = true;
        public ICollection<string> Roles { get; set; } = new List<string>();
    }
    public class Change_FullNameDTO : DeleteUser
    {
        [Required]
        public string FullName { get; set; } = "";
    }
}
