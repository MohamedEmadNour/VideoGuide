using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace VideoGuide.View_Model
{
    public class Insert_GroupsDTO
    {
        [Required]
        public string Lantin_GroupName { get; set; } = string.Empty;
        [Required]
        public string Local_GroupName { get; set; } = string.Empty;
        [Required]
        public IFormFile? Image { get; set; } 
    }
    public class Update_GroupsDTO : Insert_GroupsDTO 
    {
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        [DataExists(typeof(Group), "GroupID", ErrorMessage = "This Group is Not Found")]
        public int GroupID { get; set; }
        public bool visable { get; set; } = true;
        public string Group_Photo_Location { get; set; } = string.Empty;

    }
}
