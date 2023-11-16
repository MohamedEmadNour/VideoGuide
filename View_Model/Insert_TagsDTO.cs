using System.ComponentModel.DataAnnotations;
using VideoGuide.Models;

namespace VideoGuide.View_Model
{
    public class Insert_TagsDTO
    {
            [Required]
            public string Lantin_TagName { get; set; } = string.Empty;
            [Required]
            public string Local_TagName { get; set; } = string.Empty;
            [Required]
            public IFormFile? Image { get; set; }
    }
        public class Update_TagsDTO : Insert_TagsDTO
    {
            [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
            [DataExists(typeof(Tag), "TagID", ErrorMessage = "This Group is Not Found")]
            public int TagID { get; set; }
            public bool visable { get; set; } = true;
            public string Tag_Photo_Location { get; set; } = string.Empty;
            public IFormFile? Image { get; set; }
        }
}
