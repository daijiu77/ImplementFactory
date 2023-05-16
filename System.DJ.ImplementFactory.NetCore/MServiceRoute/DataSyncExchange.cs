using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    internal class DataSyncExchange
    {
        private static List<IMSDataSyncOutput> mSDataSyncOutputs = new List<IMSDataSyncOutput>();
        private static List<DataSyncMessage> dataSyncMessages = new List<DataSyncMessage>();

        private static Task task = null;
        private static object _dataSyncExchangeLock = new object();

        static DataSyncExchange()
        {
            task = Task.Run(() =>
            {
                List<Assembly> assemblies = DJTools.GetAssemblyCollection(DJTools.RootPath);
                Type[] types = null;
                Type dataSyncOutputType = typeof(IMSDataSyncOutput);
                object dataSyncOutput = null;
                foreach (Assembly assembly in assemblies)
                {
                    types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        Thread.Sleep(100);
                        if (type.IsInterface || type.IsEnum || type.IsAbstract) continue;
                        if (!dataSyncOutputType.IsAssignableFrom(type)) continue;
                        try
                        {
                            dataSyncOutput = Activator.CreateInstance(type);
                            mSDataSyncOutputs.Add((IMSDataSyncOutput)dataSyncOutput);
                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                    }
                    Thread.Sleep(100);
                }
            });            
        }

        public static void Start()
        {
            Task.Run(() =>
            {
                task.Wait();
            });
        }
    }
}
