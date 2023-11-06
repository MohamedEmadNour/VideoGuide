using System.ComponentModel.DataAnnotations;
using VideoGuide.Models;

namespace VideoGuide.View_Model
{
    public class GroupUserDTO
    {
        public List<listId> listId { get; set; } = new List<listId>();
        public List<listGroupID> listGroupID { get; set; } = new List<listGroupID>();

    }
    public class listId
    {
        [Required]
        [DataExists(typeof(AspNetUser), "Id", ErrorMessage = "This User is Not Found")]
        public string Id { get; set; } = string.Empty;
    }
    public class listGroupID
    {
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        [DataExists(typeof(Group), "GroupID", ErrorMessage = "This Group is Not Found")]
        public int GroupID { get; set; } = 0;
    }
}
