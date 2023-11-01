using System;
using System.Collections.Generic;

namespace VideoGuide.Models;

public partial class UserGroup
{
    public int UserGroupID { get; set; }

    public string? Id { get; set; }

    public int? GroupID { get; set; }

    public virtual Group? Group { get; set; }

    public virtual AspNetUser? IdNavigation { get; set; }
}
