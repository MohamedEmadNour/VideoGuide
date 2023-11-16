using Microsoft.AspNetCore.Mvc;

namespace VideoGuide.View_Model
{
    public class Get_GroupsDTO
    {
        public string Lantin_GroupName { get; set; } = string.Empty;
        public string Local_GroupName { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string? Group_Photo_Location { get; set; }
        public int GroupID { get; set; }
        public virtual ICollection<GetGroupUser> GetGroupUser { get; set; } = new List<GetGroupUser>();
    }
    public class GetGroupUser
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? GroupID { get; set; } = 0;
        public string Local_GroupName { get; set; } = string.Empty;
        public string Lantin_GroupName { get; set; } = string.Empty;
    }
}
