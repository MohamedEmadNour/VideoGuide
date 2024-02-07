using VideoGuide.View_Model;

namespace VideoGuide.Repository
{
    public class Compare
    {
        public static CompareListsResult<T> CompareLists<T>(IList<T> list1, IList<T> list2)
        {
            // Check if the lists are equal
            bool areEqual = list1.SequenceEqual(list2);

            // Find distinct elements of list1 not in list2
            var list1NotInList2 = list1.Except(list2);

            // Find distinct elements of list2 not in list1
            var list2NotInList1 = list2.Except(list1);
            CompareListsResult<T> compareListsResult = new CompareListsResult<T>
            {
                areEqual = areEqual,
                list1NotInList2 = list1NotInList2,
                list2NotInList1 = list2NotInList1
            };
            // Return true if all conditions are met
            return compareListsResult;
        }
    }
}
