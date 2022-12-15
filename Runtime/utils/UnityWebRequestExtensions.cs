using UnityEngine.Networking;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

namespace UFD
{
    /// <summary>
    /// Contains the following utilities relating to UnityWebRequest:
    ///     - Provides awaitability for the 'SendRequest' func
    ///     - Provides a stringification function for debugging purposes
    /// </summary>
    public static class UnityWebRequestExtensions {

        /// <summary>
        /// TODO: complete
        /// </summary>
        /// <param name="uwr">The web request to stringify.</param>
        /// <returns></returns>
        public static string ToString(this UnityWebRequest uwr) {
            string fstr = "";
            return $"TYPE: {uwr.method}\nURL: {uwr.url}\nURI: {uwr.uri} FORM: {fstr}";
        }

        /// <summary>
        /// Provides an awaiter for UnityWebRequest's to be used in an asyncronous fashion.
        /// </summary>
        /// <param name="reqOp"></param>
        /// <returns></returns>
        public static TaskAwaiter < UnityWebRequest.Result > GetAwaiter(this UnityWebRequestAsyncOperation reqOp) {
            TaskCompletionSource < UnityWebRequest.Result > tsc = new();
            reqOp.completed += asyncOp => tsc.TrySetResult(reqOp.webRequest.result);

            if (reqOp.isDone)
                tsc.TrySetResult(reqOp.webRequest.result);

            return tsc.Task.GetAwaiter();
        }
    }
}