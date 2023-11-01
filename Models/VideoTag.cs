using System;
using System.Collections.Generic;

namespace VideoGuide.Models;

public partial class VideoTag
{
    public int VideoTagID { get; set; }

    public int? VideoID { get; set; }

    public int? TagID { get; set; }

    public virtual Tag? Tag { get; set; }

    public virtual Video? Video { get; set; }
}
