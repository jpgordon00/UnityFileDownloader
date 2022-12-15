using UnityEngine.Networking;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
namespace UFD
{

    public static class TaskExtensions
{
    public static async Task<TV> Then<T,TV>(this Task<T> task, Func<T,TV> then)
    {
        var result = await task;
        return then(result);
    }
}

public static class IDownloadFulfillerExtensions
{

    public static IDownloadFulfiller[] Add(this IDownloadFulfiller[] arr1, IDownloadFulfiller[] arr2)
        {
            if (arr1 == null) arr1 = new IDownloadFulfiller[0];
            if (arr2 == null) arr2 = new IDownloadFulfiller[0];
            var pp = arr1.ToList();
            pp.AddRange(arr2.ToList());
            return pp.ToArray();
        }

    public static IDownloadFulfiller[] Dequeue(this IDownloadFulfiller[] arr)
        {
            if (arr == null) arr = new IDownloadFulfiller[0];
            List<IDownloadFulfiller> strs = new List<IDownloadFulfiller>();
            int i = 0;
            foreach (var str in arr) {
                if (i++ == 0) continue;
                strs.Add(str);
            }
            arr = strs.ToArray();
            return arr;
        }

}


    /// <summary>
    /// Provides the utility to add string arrays together (using Linq) and to Dequeue an array
    /// </summary>
    public static class Extensions {
        public static string[] Add(this string[] arr1, string arr2)
        {
            return Add(arr1, new string[]{arr2});
        }

        public static string[] Add(this string[] arr1, string[] arr2)
        {
            if (arr1 == null) arr1 = new string[0];
            if (arr2 == null) arr2 = new string[0];
            arr1.ToList().AddRange(arr2.ToList());
            return arr1.ToArray();
        }

        public static string[] Dequeue(this string[] arr)
        {
            if (arr == null) arr = new string[0];
            List<string> strs = new List<string>();
            string v = arr[0];
            int i = 0;
            foreach (var str in arr) {
                if (i++ == 0) continue;
                strs.Add(str);
            }
            arr = strs.ToArray();
            return arr;
        }
    }
}