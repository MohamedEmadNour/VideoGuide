using System;
using System.Collections.Generic;

namespace VideoGuide.Models;

public partial class GroupTag
{
    public int GroupTagID { get; set; }

    public int? GroupID { get; set; }

    public int? TagID { get; set; }

    public int? DisplayOrder { get; set; }

    public virtual Group? Group { get; set; }

    public virtual Tag? Tag { get; set; }
}
