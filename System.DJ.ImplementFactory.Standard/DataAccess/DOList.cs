using System.Collections.Generic;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DOList<T> : List<T>
    {
        private LazyDataOpt lazyDataOpt = new LazyDataOpt();

        public object ParentModel { get; set; } = null;

        public string Name { get; set; }

        public new int Add(T item)
        {
            List<T> list = new List<T>();
            list.Add(item);
            int num = lazyDataOpt.AddData(ParentModel, Name, list);
            base.Add(item);
            return num;
        }

        public new int Remove(T item)
        {
            int num = 0;
            bool mbool = base.Remove(item);
            if (!mbool) return num;
            List<T> list = new List<T>();
            list.Add(item);
            num = lazyDataOpt.DeleteData(ParentModel, list);
            return num;
        }

        public new int RemoveAt(int index)
        {
            int num = 0;
            if (0 > index) return num;
            if (index >= this.Count) return num;
            object item = this[index];
            List<object> list = new List<object>();
            list.Add(item);
            num = lazyDataOpt.DeleteData(ParentModel, list);
            base.RemoveAt(index);
            return num;
        }

        public new int Clear()
        {
            int num = lazyDataOpt.DeleteData(ParentModel, this);
            base.Clear();
            return num;
        }
    }
}
