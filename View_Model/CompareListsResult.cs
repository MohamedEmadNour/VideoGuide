namespace VideoGuide.View_Model
{
    public class CompareListsResult<T>
    {
        public bool areEqual { get; set; }
        public IEnumerable<T> list1NotInList2 { get; set; }
        public IEnumerable<T> list2NotInList1 { get; set; }
    }
}
