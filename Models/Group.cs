using System;
using System.Collections.Generic;

namespace VideoGuide.Models;

public partial class Group
{
    public int GroupID { get; set; }

    public string? Lantin_GroupName { get; set; }

    public string? Local_GroupName { get; set; }

    public string? Group_Photo_Location { get; set; }

    public bool? visable { get; set; }

    public virtual ICollection<GroupTag> GroupTags { get; set; } = new List<GroupTag>();

    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
