using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;


namespace UFD
{
    /// <summary>
    /// Defines a valid or invalid HTTP/HTTPS response.
    /// </summary>
    public class HTTPResponse {
        public string ResponseText;
        public int ResponseCode;
        public int DownloadedBytes = 0;

        public Dictionary<string, string> Headers;

        /// <summary>
        /// This property is determined by UWR and is true incase of an error.
        /// </summary>
        public bool DidError;

        public string ToString() {
            return $"ResponseText={ResponseText}, ResponseCode={ResponseCode}, DidError={DidError}";
        }
    }

    /// <summary>
    /// Contains basic utilities for HTTP communication via UnityWebRequest. 
    /// </summary>
    public static class HTTPHelper
    {

        /// <summary>
      /// Submits an async HTTP Head request.
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="timeoutSeconds"></param>
      /// <returns></returns>
      public static async Task < HTTPResponse > Get(string uri, Dictionary < string, string > headers = null, int timeoutSeconds = 3) {
        HTTPResponse resp = new HTTPResponse();

        UnityWebRequest req = UnityWebRequest.Get(uri);
        req.timeout = timeoutSeconds;
        if (headers != null) {
          foreach(var kvp in headers) req.SetRequestHeader(kvp.Key, kvp.Value);
        }
        UnityWebRequest.Result result = await req.SendWebRequest();

        if (result == UnityWebRequest.Result.Success) {
          resp.ResponseText = req.downloadHandler.text;
          resp.DidError = false;
        } else {
          resp.ResponseText = req.error;
          resp.DidError = true;
        }
        resp.ResponseCode = (int) req.responseCode;
        return resp;
      }

        /// <summary>
        /// Given a URI, naively extracts the file and type it is pointing to. 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetFilenameFromUriNaively(string uri)
        {
            string[] arr = uri.Split("/");
            string v = arr[arr.Length - 1];
            if (v.Contains("%")) {
                arr = v.Split("%");
                v = arr[arr.Length - 1];
            }
            return v;
        }



        /// <summary>
        /// Submits a basic HEAD HTTP/HTTPS REST call.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        /*
        public static async Task < HTTPResponse > Head(string uri, Dictionary < string, string > headers = null, int timeoutSeconds = 3) {
            HTTPResponse resp = new HTTPResponse();

            UnityWebRequest req = new UnityWebRequest();
            req.method = UnityWebRequest.kHttpVerbHEAD;
            req.url = uri;
            req.timeout = timeoutSeconds;
            if (headers != null) {
                foreach(var kvp in headers) req.SetRequestHeader(kvp.Key, kvp.Value);
            }

            Debug.Log($"Head URI={uri}");
            if (headers != null) foreach (var str in headers) Debug.Log($"[{str.Key}={str.Value}");
            UnityWebRequest.Result result = await req.SendWebRequest();

            if (result == UnityWebRequest.Result.Success) {
                resp.ResponseText = req.downloadHandler.text;
                resp.DidError = false;
            } else {
                resp.ResponseText = req.error;
                resp.DidError = true;
            }
            resp.ResponseCode = (int) req.responseCode;
            resp.Headers = req.GetResponseHeaders();
            req.Dispose();
            return resp;
        }
        */

        public static UnityWebRequestAsyncOperation Head(ref UnityWebRequest req, string uri, Dictionary < string, string > headers = null, int timeoutSeconds = 3) {

            req = new UnityWebRequest();
            req.method = UnityWebRequest.kHttpVerbHEAD;
            req.url = uri;
            req.timeout = timeoutSeconds;
            if (headers != null) {
                foreach(var kvp in headers) req.SetRequestHeader(kvp.Key, kvp.Value);
            }

            Debug.Log($"Head URI={uri}");
            if (headers != null) foreach (var str in headers) Debug.Log($"[{str.Key}={str.Value}");
            return req.SendWebRequest();
        }


        public static UnityWebRequestAsyncOperation Download(ref UnityWebRequest req, string uri, String path = null, bool abandonOnFailure = false, bool append = false, Dictionary < string, string > headers = null, int timeoutSeconds = 3) {
            if (path == null) path = Application.persistentDataPath; // c# does not support non-const defaults
            if (headers != null) foreach (var str in headers) Debug.Log($"[{str.Key}={str.Value}");
            HTTPResponse resp = null;
            req = new UnityWebRequest(uri);
            req.method = UnityWebRequest.kHttpVerbGET;
            string filename = GetFilenameFromUriNaively(uri);
            string _path = Path.Combine(path, filename);
            _path = _path.Replace("/", Path.DirectorySeparatorChar.ToString()); 
            if (_path.Contains("%")) {
                var arr = _path.Split("%");
                _path = arr[arr.Length - 1];
            }
            req.downloadHandler = new DownloadHandlerFile(_path, append);
            ((DownloadHandlerFile)req.downloadHandler).removeFileOnAbort = abandonOnFailure;
            req.timeout = timeoutSeconds;
            if (headers != null) {
                foreach(var kvp in headers) req.SetRequestHeader(kvp.Key, kvp.Value);
            }
            return req.SendWebRequest();
        }

    }
}