using System;
using System.Collections.Generic;

namespace VideoGuide.Models;

public partial class Tag
{
    public int TagID { get; set; }

    public string? Lantin_TagName { get; set; }

    public string? Local_TagName { get; set; }

    public string? Tag_Photo_Location { get; set; }

    public bool? visable { get; set; }

    public virtual ICollection<GroupTag> GroupTags { get; set; } = new List<GroupTag>();

    public virtual ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
}
