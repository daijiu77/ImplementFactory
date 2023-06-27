using System.Collections.Generic;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DOList<T> : List<T>
    {
        public object ParentModel { get; set; } = null;

        public new void Add(T item)
        {
            base.Add(item);
        }

        public new void Remove(T item)
        {
            base.Remove(item);
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        public new void Clear()
        {
            base.Clear();
        }
    }
}
