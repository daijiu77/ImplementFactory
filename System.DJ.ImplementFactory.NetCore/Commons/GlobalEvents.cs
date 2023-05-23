using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public delegate void FindType(Type type);
    public delegate void FindTypeOnlyNormalClass(Type type);
    public delegate void FindTypeFinish();
    public class GlobalEvents
    {
        private static List<ActionType> actionTypes = new List<ActionType>();

        private static List<FindType> findTypes = new List<FindType>();
        private static List<FindTypeOnlyNormalClass> typeOnlyNormalClasses = new List<FindTypeOnlyNormalClass>();
        private static List<FindTypeFinish> findTypeFinishes = new List<FindTypeFinish>();

        private static event FindType _OnFindType;
        private static event FindTypeOnlyNormalClass _OnFindTypeOnlyNormalClass;
        private static event FindTypeFinish _OnFindTypeFinish;

        private static bool _IsLoaded = false;
        private static object _LoadTypesLock = new object();

        public static event FindType OnFindType
        {
            add
            {
                _OnFindType += value;
                findTypes.Add(value);
            }
            remove
            {
                _OnFindType -= value;
            }
        }

        public static event FindTypeOnlyNormalClass OnFindTypeOnlyNormalClass
        {
            add
            {
                _OnFindTypeOnlyNormalClass += value;
                typeOnlyNormalClasses.Add(value);
            }
            remove
            {
                _OnFindTypeOnlyNormalClass -= value;
            }
        }

        public static event FindTypeFinish OnFindTypeFinish
        {
            add
            {
                _OnFindTypeFinish += value;
                findTypeFinishes.Add(value);
            }
            remove
            {
                _OnFindTypeFinish -= value;
            }
        }

        public static void ForeachType(Action<Type> actionType, Action actionFinish, Type parentType, bool isNormalClassType)
        {
            lock (_LoadTypesLock)
            {
                if (null != actionType)
                {
                    actionTypes.Add(new ActionType()
                    {
                        actionType = actionType,
                        actionFinish = actionFinish,
                        parentType = parentType,
                        isNormalClassType = isNormalClassType
                    });
                }

                if (_IsLoaded)
                {
                    LoadTypes();
                }
            }            
        }

        public static void ForeachType(Action<Type> actionType)
        {
            lock (_LoadTypesLock)
            {
                ForeachType(actionType, null, null, false);
            }            
        }

        public static void ForeachType(Action<Type> actionType, Action actionFinish)
        {
            lock (_LoadTypesLock)
            {
                ForeachType(actionType, actionFinish, null, false);
            }           
        }

        public static void ForeachType(Action<Type> actionType, Action actionFinish, Type parentType)
        {
            lock (_LoadTypesLock)
            {
                ForeachType(actionType, actionFinish, parentType, false);
            }            
        }

        public static void ForeachType(Action<Type> actionType, Action actionFinish, bool isNormalClassType)
        {
            lock (_LoadTypesLock)
            {
                ForeachType(actionType, actionFinish, null, isNormalClassType);
            }            
        }

        public static void ForeachType(Action<Type> actionType, bool isNormalClassType)
        {
            lock (_LoadTypesLock)
            { 
                ForeachType(actionType, null, null, isNormalClassType); 
            }
        }

        public static void ForeachType(Action<Type> actionType, Type parentType)
        {
            lock(_LoadTypesLock) ForeachType(actionType, null, parentType, false);
        }

        public static void ForeachType(Action<Type> actionType, Type parentType, bool isNormalClassType)
        {
            lock (_LoadTypesLock) ForeachType(actionType, null, parentType, isNormalClassType);
        }

        private static void ExecFindType(Type type)
        {
            if (null != _OnFindType)
            {
                try
                {
                    _OnFindType(type);
                }
                catch (Exception ex)
                {

                    //throw;
                }
            }

            bool mbool = true;
            if (type.IsAbstract || type.IsInterface) mbool = false;

            if (null != _OnFindTypeOnlyNormalClass)
            {
                if (mbool)
                {
                    _OnFindTypeOnlyNormalClass(type);
                }
            }

            foreach (ActionType item in actionTypes)
            {
                if (item.isNormalClassType)
                {
                    if (!mbool) continue;
                }

                if (null != item.parentType)
                {
                    if (!item.parentType.IsAssignableFrom(type)) continue;
                }

                try
                {
                    item.actionType(type);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
        }

        private static void ExecFindTypeFinish()
        {
            if (null != _OnFindTypeFinish)
            {
                try
                {
                    _OnFindTypeFinish();
                }
                catch (Exception ex)
                {

                    //throw;
                }
            }

            foreach (ActionType item in actionTypes)
            {
                if (null == item.actionFinish) continue;
                try
                {
                    item.actionFinish();
                }
                catch (Exception)
                {

                    //throw;
                }
            }
        }
                        
        public static void LoadTypes()
        {
            lock (_LoadTypesLock)
            {
                List<Assembly> asseList = DJTools.GetAssemblyCollection(DJTools.RootPath);
                Type[] types = null;
                foreach (Assembly item in asseList)
                {
                    types = item.GetTypes();
                    foreach (var type in types)
                    {
                        GlobalEvents.ExecFindType(type);
                    }
                }
                GlobalEvents.ExecFindTypeFinish();

                foreach (FindType item in findTypes)
                {
                    OnFindType -= item;
                }
                findTypes.Clear();

                foreach (var item in typeOnlyNormalClasses)
                {
                    OnFindTypeOnlyNormalClass -= item;
                }
                typeOnlyNormalClasses.Clear();

                foreach (var item in findTypeFinishes)
                {
                    OnFindTypeFinish -= item;
                }
                findTypeFinishes.Clear();

                actionTypes.Clear();
                _IsLoaded = true;
            }
        }

        class ActionType
        {
            public Action<Type> actionType { get; set; }
            public Action actionFinish { get; set; }
            public Type parentType { get; set; }
            public bool isNormalClassType { get; set; }
        }

    }
}
