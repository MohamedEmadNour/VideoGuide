using System;
using System.Collections.Generic;

namespace VideoGuide.Models;

public partial class Video
{
    public int VideoID { get; set; }

    public string? Video_Lantin_Title { get; set; }

    public string? Video_Local_Tiltle { get; set; }

    public string? Video_Description { get; set; }

    public string? Video_Location { get; set; }

    public int? Video_CountOfViews { get; set; }

    public virtual ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();

    public virtual ICollection<Video_Fav> Video_Favs { get; set; } = new List<Video_Fav>();
}
