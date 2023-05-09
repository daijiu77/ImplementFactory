using System.Collections;
using System.Collections.Generic;

namespace System.DJ.ImplementFactory.Entities
{
    public class GroupsRoute : IEnumerable<RouteAttr>
    {
        private List<RouteAttr> routeAttrList = new List<RouteAttr>();
        private RouteItem routeItem = null;

        public GroupsRoute()
        {
            routeItem = new RouteItem(this);
        }

        public string Name { get; set; }

        public List<RouteAttr> Children { get { return routeAttrList; } }

        IEnumerator<RouteAttr> IEnumerable<RouteAttr>.GetEnumerator()
        {
            return routeItem;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return routeItem;
        }

        class RouteItem : IEnumerator<RouteAttr>, IEnumerator
        {
            private RouteAttr routeAttr = null;
            private GroupsRoute groupsRoute = null;
            private int index = 0;

            public RouteItem(GroupsRoute groupsRoute)
            {
                this.groupsRoute = groupsRoute;
            }

            RouteAttr IEnumerator<RouteAttr>.Current => routeAttr;

            object IEnumerator.Current => routeAttr;

            void IDisposable.Dispose()
            {
                index = 0;
            }

            bool IEnumerator.MoveNext()
            {
                if (groupsRoute.Children.Count <= index) return false;
                routeAttr = groupsRoute.Children[index];
                index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                index = 0;
            }
        }
    }
}
