namespace VideoGuide.View_Model
{
    public class Get_GroupsDTO
    {
        public string Lantin_GroupName { get; set; } = string.Empty;
        public string Local_GroupName { get; set; } = string.Empty;
        public byte[] Image { get; set; }
        public string? Group_Photo_Location { get; set; }

    }
}
