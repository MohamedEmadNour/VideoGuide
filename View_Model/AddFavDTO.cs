using System.ComponentModel.DataAnnotations;
using VideoGuide.Models;

namespace VideoGuide.View_Model
{
    public class AddFavDTO
    {

        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        [DataExists(typeof(Video), "VideoID", ErrorMessage = "This Video is Not Found")]
        public int VideoID { get; set; }
        [DataExists(typeof(AspNetUser), "Id", ErrorMessage = "This User is Not Found")]

        public string Id { get; set; } = string.Empty;
    }
}
