namespace VideoGuide.View_Model
{
    public class Get_TagsDTO
    {
        public string Lantin_TagName { get; set; } = string.Empty;
        public string Local_TagName { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string? Tag_Photo_Location { get; set; }
        public int TagID { get; set; }
        public virtual ICollection<GetTagGroup> GetTagGroup { get; set; } = new List<GetTagGroup>();
    }
    public class GetTagGroup
    {
        public int? GroupID { get; set; } = 0;
        public string Local_GroupName { get; set; } = string.Empty;
        public string Lantin_GroupName { get; set; } = string.Empty;
    }
}
