using System;
using System.Collections.Generic;

namespace VideoGuide.Models;

public partial class Video_Fav
{
    public int Video_Fav1 { get; set; }

    public int? VideoID { get; set; }

    public string? Id { get; set; }

    public virtual AspNetUser? IdNavigation { get; set; }

    public virtual Video? Video { get; set; }
}
