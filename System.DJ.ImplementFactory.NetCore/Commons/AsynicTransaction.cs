using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public static class AsynicTransaction
    {
        private static Thread thread = null;
        private static bool isRunning = true;
        private static Dictionary<string, AsynicElement> dic = new Dictionary<string, AsynicElement>();
        private static int sleepNum = 100;

        private static object objThis = null;

        static AsynicTransaction()
        {
            //thread = new Thread(run);
            //thread.Start();
            new Task(() =>
            {
                isRunning = true;
                while (isRunning)
                {
                    Thread.Sleep(sleepNum);
                    ExecAsynic(null, false, 0, 0, 0, 0, null);
                }
            }).Start();
        }

        private static void run()
        {
            isRunning = true;
            while (isRunning)
            {
                Thread.Sleep(sleepNum);
                ExecAsynic(null, false, 0, 0, 0, 0, null);
            }
        }

        private static void getSourceObj()
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = trace.GetFrame(2);
            MethodBase methodBase = stackFrame.GetMethod();
            string methodName = methodBase.Name;
            object impl = methodBase.DeclaringType;
            if (impl.GetType() == typeof(AsynicTransaction)) return;
            objThis = impl;
        }

        /// <summary>
        /// 向计时器添加一个以100毫秒为间隔的执行任务
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="milliseconds_100">执行任务时间间隔,单位:100毫秒</param>
        /// <param name="whileCount">执行次数, 为零时无限执行,默认值为零</param>
        /// <param name="action">执行任务</param>
        public static AsynicData Add(string taskName, int milliseconds_100, int whileCount, Action<AsynicData> action)
        {
            getSourceObj();
            AsynicElement asynicElement = null;
            dic.TryGetValue(taskName, out asynicElement);
            if (null != asynicElement) return asynicElement.asynicData;
            sleepNum = 10;
            milliseconds_100 *= 10;
            asynicElement = ExecAsynic(taskName, true, 0, milliseconds_100, sleepNum, whileCount, action);
            return asynicElement.asynicData;
        }

        /// <summary>
        /// 向计时器添加一个以100毫秒为间隔的执行任务
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="milliseconds_100">执行任务时间间隔,单位:100毫秒</param>
        /// <param name="action">执行任务</param>
        public static AsynicData Add(string taskName, int milliseconds_100, Action<AsynicData> action)
        {
            getSourceObj();
            int whileCount = 0;
            AsynicData asynicData = Add(taskName, milliseconds_100, whileCount, action);
            return asynicData;
        }

        /// <summary>
        /// 向计时器添加一个以10毫秒为间隔的执行任务
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="milliseconds_10">执行任务时间间隔,单位:10毫秒</param>
        /// <param name="whileCount">执行次数, 为零时无限执行,默认值为零</param>
        /// <param name="action">执行任务</param>
        /// <returns></returns>
        public static AsynicData AddOf10ms(string taskName, int milliseconds_10, int whileCount, Action<AsynicData> action)
        {
            getSourceObj();
            AsynicElement asynicElement = null;
            dic.TryGetValue(taskName, out asynicElement);
            if (null != asynicElement) return asynicElement.asynicData;
            sleepNum = 10;
            asynicElement = ExecAsynic(taskName, true, 0, milliseconds_10, sleepNum, whileCount, action);
            return asynicElement.asynicData;
        }

        /// <summary>
        /// 向计时器添加一个以10毫秒为间隔的执行任务
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="milliseconds_10">执行任务时间间隔,单位:10毫秒</param>
        /// <param name="action">执行任务</param>
        /// <returns></returns>
        public static AsynicData AddOf10ms(string taskName, int milliseconds_10, Action<AsynicData> action)
        {
            getSourceObj();
            int whileCount = 0;
            return AddOf10ms(taskName, milliseconds_10, whileCount, action);
        }

        public static void Remove(string taskName)
        {
            ExecAsynic(taskName, false, 0, 0, 0, 0, null);
        }

        public static List<string> GetKeys()
        {
            Dictionary<string, AsynicElement>.KeyCollection keys = dic.Keys;
            List<string> list = new List<string>();

            foreach (var item in keys)
            {
                list.Add(item);
            }
            return list;
        }

        public static void Distory()
        {
            isRunning = false;
        }

        static object _asynicObj = new object();
        static AsynicElement ExecAsynic(string taskName, bool isAdd, int milliseconds_100, int milliseconds_10, int UnitOfMS, int whileCount, Action<AsynicData> action)
        {
            lock (_asynicObj)
            {
                AsynicElement ae = null;
                if (string.IsNullOrEmpty(taskName))
                {
                    foreach (KeyValuePair<string, AsynicElement> item in dic)
                    {
                        if (item.Value.asynicData.isStop) continue;
                        if (0 != item.Value.WhileCount && item.Value.WhileCount <= item.Value.asynicData.currentCount) continue;
                        item.Value.currentMS++;
                        if (item.Value.currentMS == item.Value.Milliseconds)
                        {
                            item.Value.currentMS = 0;
                            item.Value.asynicData.currentCount++;
                            if (999999 == item.Value.asynicData.currentCount) item.Value.asynicData.currentCount = 0;
                            item.Value.exec();
                        }
                    }
                }
                else if (isAdd)
                {
                    ae = new AsynicElement();
                    ae.Milliseconds = 0 < milliseconds_100 ? milliseconds_100 : milliseconds_10;
                    ae.Milliseconds1 = ae.Milliseconds;
                    ae.UnitOfMS = UnitOfMS;
                    ae.UnitOfMS1 = UnitOfMS;
                    ae.WhileCount = whileCount;
                    ae.action = action;
                    ae.TaskName = taskName;
                    ae.objThis = objThis;
                    dic.Add(taskName, ae);

                    foreach (var item in dic)
                    {
                        if (sleepNum != item.Value.UnitOfMS)
                        {
                            item.Value.currentMS = 0;
                            item.Value.Milliseconds = (item.Value.Milliseconds * item.Value.UnitOfMS) / sleepNum;
                            if (0 == item.Value.Milliseconds) item.Value.Milliseconds = 1;
                            if (sleepNum == item.Value.UnitOfMS1)
                            {
                                item.Value.Milliseconds = item.Value.Milliseconds1;
                            }
                            item.Value.UnitOfMS = sleepNum;
                        }
                    }
                }
                else
                {
                    dic.TryGetValue(taskName, out ae);
                    if (null != ae)
                    {
                        dic.Remove(taskName);
                    }
                }
                return ae;
            }
        }
    }

    public class AsynicElement
    {
        private SynchronizationContext m_SyncContext = SynchronizationContext.Current;

        private delegate void CallControl();

        public string TaskName { get; set; }

        public Action<AsynicData> action { get; set; }

        public int Milliseconds { get; set; }

        public int Milliseconds1 { get; set; }

        public int UnitOfMS { get; set; }

        public int UnitOfMS1 { get; set; }

        /// <summary>
        /// 循环次数, 0 = 无限
        /// </summary>
        public int WhileCount { get; set; }

        public int currentMS { get; set; } = 0;

        public AsynicData asynicData { get; } = new AsynicData();

        public object objThis { get; set; }

        public void exec()
        {
            Task task = new Task(() =>
            {
                m_SyncContext = SynchronizationContext.Current;
                if (null != m_SyncContext)
                {
                    m_SyncContext.Post(PostFunction, null);
                }
                else if (null != objThis)
                {
                    MethodInfo method = objThis.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance,
                        Type.DefaultBinder, new Type[] { typeof(CallControl) }, new ParameterModifier[] { new ParameterModifier(1) });
                    if (null != method)
                    {
                        CallControl cc = delegate ()
                        {
                            PostFunction(null);
                        };

                        try
                        {
                            method.Invoke(objThis, new object[] { cc });
                        }
                        catch (Exception)
                        {
                            PostFunction(null);
                            //throw;
                        }
                    }
                    else
                    {
                        PostFunction(null);
                    }
                }
                else
                {
                    PostFunction(null);
                }
            });
            task.Start();
        }

        void PostFunction(object para)
        {
            try
            {
                action(asynicData);
            }
            catch { }
        }
    }

    public class AsynicData
    {
        public int currentCount { get; set; } = 0;
        public bool isStop { get; set; }

        public object temp { get; set; }
    }
}
