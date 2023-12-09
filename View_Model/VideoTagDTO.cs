using System.ComponentModel.DataAnnotations;
using VideoGuide.Models;

namespace VideoGuide.View_Model
{
    public class VideoTagDTO
    {
        public List<listVideoID> listVideoID { get; set; } = new List<listVideoID>();
        public List<listTagID> listTagID { get; set; } = new List<listTagID>();
    }
    public class listVideoID
    {
        [Required]
        [DataExists(typeof(Video), "VideoID", ErrorMessage = "This Video is Not Found")]
        public int VideoID { get; set; } = 0;
    }
    public class listTagID
    {
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        [DataExists(typeof(Tag), "TagID", ErrorMessage = "This Tag is Not Found")]
        public int TagID { get; set; } = 0;
    }
    public class GroupTagDTO
    {
        public List<listGroupID> listGroupID { get; set; } = new List<listGroupID>();
        public List<listTagID> listTagID { get; set; } = new List<listTagID>();
    }
}
