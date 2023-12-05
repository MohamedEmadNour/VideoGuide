using System.ComponentModel.DataAnnotations;
using VideoGuide.Models;

namespace VideoGuide.View_Model
{
    public class VideoDTO
    {
        [Required]
        [StringLength(255,ErrorMessage = "Video Lantin Title is very long")]
        public string? Video_Lantin_Title { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Video Local Title is very long")]
        public string? Video_Local_Tiltle { get; set; }
        [Required]
        public string? Video_Lantin_Description { get; set; }
        [Required]
        public string? Video_Local_Description { get; set; }
        [Required]
        public IFormFile? Video { get; set; }
        //public string? Video_Location { get; set; }

        //public int? Video_CountOfViews { get; set; }

        //public bool? visable { get; set; }
    }
    public class UpdateVideoDTO : VideoDTO
    {
        [Required]
        public bool? visable { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        [DataExists(typeof(Video), "VideoID", ErrorMessage = "This Video is Not Found")]
        public int VideoID { get; set; }

        public string? Video_Location { get; set; }
        public IFormFile? Video { get; set; }

    }
    public class Get_VideosDTO
    {
        public int VideoID { get; set; }
        public string Video_Lantin_Title { get; set; } = string.Empty;
        public string Video_Local_Tiltle { get; set; } = string.Empty;
        public string Video_Lantin_Description { get; set; } = string.Empty;
        public string Video_Local_Description { get; set; } = string.Empty;
        public string Video_Location { get; set; } = string.Empty;
        public string Video { get; set; } = string.Empty;
        public int? Video_CountOfViews { get; set; } = 0;
        public bool visable { get; set; } = false;
    }
    public class GetVideoTagDTO
    {
        public int VideoTagID { get; set;}
        public int TagID { get; set;}
        public string Lantin_TagName { get; set; } = string.Empty;
        public string Local_TagName { get; set; } = string.Empty;
    }
    public class Get_Videos_with_tagDTO : Get_VideosDTO
    {
        public virtual ICollection<GetVideoTagDTO> GetVideoTagDTO { get; set; } = new List<GetVideoTagDTO>();
    }
}
