using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using VideoGuide.IRepository;
using VideoGuide.View_Model;
using static VideoGuide.Repository.Compare;

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

            // Return the comparison result
            return compareListsResult;
        }
        public static CompareListsResult<T> CompareListsObject<T>(IList<T> list1, IList<T> list2 , List<String> excption)
        {
            // Check if the lists are equal
            bool areEqual = true;

            // Find elements in list1 that are not in list2
            var list1NotInList2 = list1.Except(list2, new CustomComparer<T>(excption));

            // Find elements in list2 that are not in list1
            var list2NotInList1 = list2.Except(list1, new CustomComparer<T>(excption));

            // Check if any elements were found in the lists
            if (list1NotInList2.Any() || list2NotInList1.Any())
            {
                areEqual = false;
            }

            CompareListsResult<T> compareListsResult = new CompareListsResult<T>
            {
                areEqual = areEqual,
                list1NotInList2 = list1NotInList2,
                list2NotInList1 = list2NotInList1
            };

            // Return the comparison result
            return compareListsResult;
        }

        public class CustomComparer<T> : IEqualityComparer<T>
        {
            private readonly HashSet<string> _includedProperties;

            public CustomComparer(List<string> includedProperties)
            {
                _includedProperties = new HashSet<string>(includedProperties);
            }

            public bool Equals(T x, T y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x == null || y == null)
                    return false;

                var type = typeof(T);

                foreach (var property in type.GetProperties())
                {
                    if (_includedProperties.Contains(property.Name))
                        continue;

                    var valueX = property.GetValue(x);
                    var valueY = property.GetValue(y);

                    if (!Equals(valueX, valueY))
                        return false;
                }

                return true;
            }

            public int GetHashCode(T obj)
            {
                if (obj == null)
                    return 0;

                var type = typeof(T);
                var hashCode = 17;

                foreach (var property in type.GetProperties())
                {
                    if (_includedProperties.Contains(property.Name))
                        continue;

                    var value = property.GetValue(obj);
                    hashCode = hashCode * 23 + (value?.GetHashCode() ?? 0);
                }

                return hashCode;
            }
        }
    }
}
