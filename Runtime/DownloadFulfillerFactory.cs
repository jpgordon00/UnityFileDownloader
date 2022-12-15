using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UFD
{

    /// <summary>
    /// Contains functions that create 'IDownloadFulfiller' children from a class name of one of its children
    /// </summary>
    public static class DownloadFulfillerFactory
    {
        /// <summary>
        /// Stores all the 'IDownloadFulfiller' children types.
        /// </summary>
        /// <typeparam name="Singleton"></typeparam>
        /// <returns></returns>
        internal static List < Type > _Types = new List < Type > ();

        /// <summary>
        /// Searches through assembly to find all children of 'IDownloadFulfiller' and store it in '_Types'
        /// </summary>
        static DownloadFulfillerFactory()
        {
            _Types = typeof (IDownloadFulfiller).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof (IDownloadFulfiller)) && !t.IsAbstract).Select(t => {
                if (t == typeof (IDownloadFulfiller)) return null; // ignore base types
                return t; //(IDownloadFulfiller) Activator.CreateInstance(t);
            }).ToList();
        }

        /// <summary>
        /// Given a string matching the complete classname of some 'IDownloadFulfiller' child, return a new instance of that class.
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public static IDownloadFulfiller CreateFromClassName(string classname) {
            return (IDownloadFulfiller) Activator.CreateInstance(_Types.Find(t => t.Name == classname));
        }
    }
}