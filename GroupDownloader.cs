using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;

namespace UFD
{
 
    /// <summary>
    /// Used to handle async locking while being able to await
    /// </summary>
    internal class SemaphoreLocker
    {
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
 
    public async Task LockAsync(Func<Task> worker, object args = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            await worker();
        }
        finally
        {
            _semaphore.Release();
        }
    }
 
    // overloading variant for non-void methods with return type (generic T)
    public async Task<T> LockAsync<T>(Func<Task<T>> worker, object args = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await worker();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
 
 
 
    // downloads multiple files to a path ( one-after-another execution )
    // events for download failure and success
    // all pending URI's should be pre-checked for syntax
    // can assign filenames via filenameToURI or 
    public class GroupDownloader {
        internal readonly SemaphoreLocker _Locker = new SemaphoreLocker();
 
        // path to download each file appended by the file name ofc
        // defaults to persistent data path
        private string _downloadPath = Application.persistentDataPath;
 
        public string DownloadPath {
            get {
                return _downloadPath;
            }
            set {
              _downloadPath = value;
            }
        }
 
        // true if any files are being downloaded currently
        private bool _downloading = false;
        public bool Downloading {
            get {
                return _downloading;
            }
        }
 
        // true if the download handler should continue on failure
        private bool _abandonOnFailure = true;
 
        public bool AbandonOnFailure {
            get {
                return _abandonOnFailure;
            }
            set {
              _abandonOnFailure = value;
            }
        }
 
        // time before timing out a specific download
        public int Timeout = 7;
 
        public delegate void GroupDownloadFailureEvent(bool completed, string uri, string fileResultPath);
        public delegate void GroupDownloadSuccessEvent(bool completed, string uri, string fileResultPath);
 
        // triggered when a download fails
        // may be invoked multiple times
        public event GroupDownloadFailureEvent OnDownloadFailure;
 
        // triggered when the entire group succeeds
        public event GroupDownloadSuccessEvent OnDownloadSuccess;
 
        // option for delivering filenames from URI
        public delegate string URIToFilename(string uri);
 
        // true to get filenames from this map
        // otherwise use URIToFilename delegate
        private bool _useUriFilenameMap = true;
 
        public bool UseURIFilenameMap {
            get {
                return _useUriFilenameMap;
            }
            set {
                _useUriFilenameMap = value;
            }
        }
        
        private URIToFilename _onUriToFilename;
 
        public URIToFilename OnURIToFilename {
            get {
                return _onUriToFilename;
            }
            set {
                _onUriToFilename = value;
            }
        }
 
        public Dictionary<string, string> _uriFilenameMap = new Dictionary<string, string>();
 
        public Dictionary<string, string> URIFilenameMap {
            get {
                return _uriFilenameMap;
            }
            set {
                _uriFilenameMap = value;
            }
        }
 
        // urls to download in increasing index order
        // urls are removed after completion or failure
        private List < string > _pendingUrls = new List < string > ();
 
        // urls that downloaded succesfully
        // do not manually load or remove items
        private List < string > _completedUrls = new List < string > ();
 
        // urls that were abandoned and not downloaded
        // do not manually load or remove items
        private List < string > _uncompletedUrls = new List < string > ();
 
        // populated with succesfully downloaded 
        private List < string > _completedURLPaths = new List < string > ();
 
        // cxurrent files downloading
        public int Count {
            get {
                return PendingURLS == null ? 0 : PendingURLS.Count;
            }
        }
 
 
        public List < string > PendingURLS {
            get {
                return _pendingUrls;
            }
        }
 
        public List < string > CompletedURLS {
            get {
                return _completedUrls;
            }
        }
 
        public List < string > UncompletedURLS {
            get {
                return _uncompletedUrls;
            }
        }
 
        public List < string > CompletedURLPaths {
            get {
                return _completedURLPaths;
            }
        }
 
        /// <summary>
        /// Number of files to download at once.
        /// Set to 1 to download one by one .
        /// </summary>
        public int MaxConcurrency = 16;
 
        // download progress of all succesful and pending downloads
        /// from 0f to 1f 
        private float _progress = 0f;
 
        // number of qued files calculated when 'Download' is invoked
        private int _initialCount;
 
        public float Progress {
            get {
                return _progress;
            }
        }
 
        // true if the download expierenced an error or got cancelled
        public bool DidError {
            get {
                return UncompletedURLS.Count > 0;
            }
        }
 
        // true if the download is finished and not in progress
        public bool DidFinish {
            get {
                return PendingURLS.Count == 0;
            }
        }
 
        public int _startTime = -1, _endTime = -1, _elapsedTime = -1;
 
        // time the last download was initiated at
        public int StartTime {
          get {
            return _startTime;
          }
        }
 
        // time the last download ended, cancelled or errorerd
        public int EndTime {
          get {
            return _endTime;
          }
        }
 
        // time since start if stil running
        // or total time taken to download or fail
        public int ElapsedTime {
          get {
            if (EndTime == -1) return DateTime.Now.Millisecond - StartTime;
            return EndTime - StartTime;
          }
        }
 
        /// <summary>
        /// Wait this many milliseconds to fulfill new downloads in a concurrency thrread
        /// </summary>
        public float _WaitForConcurrency = 1200;
 
        // constructor to feed in pending urls
        public GroupDownloader(List < string > urls = null) {
            if (urls != null) {
              foreach(var str in urls) PendingURLS.Add(str);
            }
        }
 
        // starts a group download with PendingURLS
        public bool Download() {
            if (PendingURLS.Count == 0 || Downloading) return false;
            _initialCount = PendingURLS.Count;
            _downloading = true;
            _startTime = DateTime.Now.Millisecond;
            
            for (int i = 0; i < MaxConcurrency; i++) _Dispatch();
            return true;
        }

        internal int _N = 0;
 
        // dispatch each request, assume one by one
        private async void _Dispatch() {
            if (PendingURLS.Count == 0) { // handle no URL case
                HandleCancel();
                return;
                }
                string uri = PendingURLS[0];
                if ((OnURIToFilename == null && !UseURIFilenameMap) || (UseURIFilenameMap && !URIFilenameMap.ContainsKey(uri))) {
                HandleCancel();
                return;
                }
                string fileName = UseURIFilenameMap ? URIFilenameMap[uri]: OnURIToFilename(uri);
                if (fileName == null && AbandonOnFailure) {
                HandleCancel();
                return;
                }
                var fileResultPath = Path.Combine(DownloadPath, fileName);
                PendingURLS.RemoveAt(0);
                if (!Downloading) { // case cancel invoked
                HandleCancel(uri, fileResultPath);
                if (AbandonOnFailure) return;
                }
            _N ++;
            var uwr = new UnityWebRequest(uri);
            uwr.timeout = Timeout;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            var dh = new DownloadHandlerFile(fileResultPath);
            dh.removeFileOnAbort = true;
            uwr.downloadHandler = dh;
            var operation = uwr.SendWebRequest();
            while (!operation.isDone) await Task.Delay(100);
            _HandleDispatchComplete(uwr, uri, fileName, fileResultPath);
        }

        private async Task _HandleDispatchComplete(UnityWebRequest uwr, string uri, string fileName, string fileResultPath) {
            await _Locker.LockAsync(async () => {
                if (!Downloading) {
                    return;
                }
            if (uwr.isNetworkError || uwr.isHttpError) { // case network error 
                Debug.LogError(uwr.error);
                HandleCancel(uri, fileResultPath);
                //if (PendingURLS.Count > 0) _HandleDispatch();
            } else { // case succcesful download
                _progress += (float)(1f / (float) _initialCount);
                CompletedURLS.Add(uri);
                CompletedURLPaths.Add(fileResultPath);
                if (PendingURLS.Count > 0) { // case more files to download
                    OnDownloadSuccess?.Invoke(DidFinish, uri, fileResultPath);
                    // case: too many files processing at once, wait for _WaitForConcurrency
                    if (_N > MaxConcurrency) {
                        var time = DateTime.UtcNow.Millisecond;
                        while (DateTime.UtcNow.Millisecond - time < _WaitForConcurrency) await Task.Delay(50);
                        if (_N > MaxConcurrency) {
                        // case: too many files processing at once, wait for _WaitForConcurrency
                        // cancel bro
                        return;
                    }
                    // new worker thread
                    _Dispatch();
                   _N--;
                }
                // new worker thread
                _Dispatch();
                _N--;
                } else { 
                    // case no more files to download
                    _downloading = false;
                    _endTime = DateTime.Now.Millisecond;
                    _elapsedTime = EndTime - StartTime;
                    OnDownloadSuccess?.Invoke(DidFinish, uri, fileResultPath);
                }
            }
            });
        }
 
        // handles a failure of cancel post-download or pre-download
        private void HandleCancel(string uri = null, string fileResultPath = null) {
            _endTime = DateTime.Now.Millisecond;
            _elapsedTime = EndTime - StartTime;
            _downloading = false;
 
            if (uri != null) UncompletedURLS.Add(uri);
            if (AbandonOnFailure) {
              if (fileResultPath != null) File.Delete(fileResultPath);
              for (int i = CompletedURLPaths.Count - 1; i >= 0; i--) {
                File.Delete(CompletedURLPaths[i]);
              }
              for (int i = PendingURLS.Count - 1; i >= 0; i--) {
                UncompletedURLS.Add(PendingURLS[i]);
              }
              PendingURLS.Clear();
              _progress = 0f;
              OnDownloadFailure?.Invoke(DidFinish, uri, fileResultPath);  
              return;
            }
            _progress += (1 / _initialCount); // because this value would not be incremented otherwise
            OnDownloadFailure?.Invoke(DidFinish, uri, fileResultPath);            
        }
 
        // param true to stop a current download
        // NOTE: case enum gets called AFTER this func invokates
        public void Cancel() {
            if (!Downloading) return;
            _downloading = false;
        }
 
        // resets internal values and allows reuse of this group downloader
        public void Reset() {
            if (Downloading) return;
            _completedUrls = new List < string > ();
            _uncompletedUrls = new List < string > ();
            _progress = 0f;
            _initialCount = 0;
            OnURIToFilename = null;
            _uriFilenameMap = new Dictionary<string, string>();
        }
 
    }
 
}
