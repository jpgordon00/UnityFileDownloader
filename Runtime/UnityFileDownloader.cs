using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UFD
{
    /// <summary>
    /// Represents the top-level object responsible for handling file downloading in Untiy.
    /// </summary>
    public class UnityFileDownloader : IDownloader
    {
     public UnityFileDownloader()
     {

     }

        /// <summary>
        /// Constructor to allow the setting of URIs
        /// </summary>
        /// <param name="strings"></param>
        public UnityFileDownloader(IEnumerable<string> strings)
        {
            List<string> uris = new List<string>();
            foreach (var str in strings) uris.Add(str);
            Uris = uris.ToArray();
        }

        internal readonly static SemaphoreLocker _Locker = new SemaphoreLocker();

        internal int _InitialCount;
        internal int _Timeout = 6;
        internal int _MaxConcurrency = 4;
        internal bool _AbandonOnFailure = true;
        internal bool _ContinueAfterFailure = false;
        internal bool _Downloading = false;
        internal bool _Paused = false;
        internal bool _DidError = false;
        internal int _NumFilesRemaining = 0;
        internal int _StartTime = 0, _EndTime = 0;
        internal string _DownloadPath = UnityEngine.Application.persistentDataPath;
        internal string[] _PendingUris = null;
        internal string[] _DownloadedUris = null;

        #region Events/Actions

        /// <summary>
        /// Invoked when all files are downloaded
        /// </summary>
        public event Action OnDownloadsSuccess;

        /// <summary>
        /// Invoked implicitly upon download and cancel.
        /// </summary>
        public event Action OnDownloadInvoked, OnCancelInvoked;

        /// <summary>
        /// Invoked implicitly when a pause or unpause initiates.
        /// </summary>
        public event Action<string> OnCancelIndividual, OnDownloadIndividualInvoked;

        public event Action OnCancel;
        
        #endregion

        public override string IDownloadFulfillerClassName => "UWRFulfiller"; // for reundancy / an example

        public override int StartTime => _StartTime;
        public override int EndTime => _EndTime;

        /// <summary>
        /// Calculates the amount of files-per-second this downloder processed. 
        /// </summary>
        /// <value></value>
        public override float NumFilesPerSecond
        {
            get
            {
                if (ElapsedTime == 0) return 0f;
                return (ElapsedTime / 1000) / (DownloadedUris.Length);
            }
        }

        public float MegabytesDownloadedPerSecond => BytesDownloadedPerSecond == 0 ? 0 : BytesDownloadedPerSecond / 1000;

        public float BytesDownloadedPerSecond
        {
            get
            {
                float tb = 0;
                float elapsed = 0;
                foreach (var idf in _FulfillersOld) {
                    tb += idf.BytesDownloaded;
                    elapsed += idf.ElapsedTime;
                }
                foreach (var idf in _Fulfillers) {
                    tb += idf.BytesDownloaded;
                    elapsed += idf.ElapsedTime;
                }
                return (tb / 1000) / (elapsed / 1000f);
            }
        }
        
        public override int Timeout
        {
            get => _Timeout;
            set {
                _Timeout = value;
            }
        }

        public override int MaxConcurrency
        {
            get => _MaxConcurrency; set {
                _MaxConcurrency = value;
            }
        }

        public override string DownloadPath
        {
            get => _DownloadPath; set {
                _DownloadPath = value;
            }
        }

        /// <summary>
        /// If true, then this downloader will delete all partially downloaded files.
        /// </summary>
        /// <value></value>
        public override bool AbandonOnFailure
        {
            get => _AbandonOnFailure; set {
                _AbandonOnFailure = value;
            }
        }

        /// <summary>
        /// If true, then this downloader will continue attempting to download files post-error.
        /// </summary>
        /// <value></value>
        public override bool ContinueAfterFailure
        {
           get => _ContinueAfterFailure; set {
            _ContinueAfterFailure = value;
            }
        }


        public override bool Downloading => _Downloading;

        public override bool DidError {
            get => _DidError;
            protected set {
                _DidError = value;
            }
        }

        public override bool Paused => _Paused;

        public override int NumFilesRemaining => _NumFilesRemaining;

        public override string[] PendingURIS => _PendingUris;

        public override string[] IncompletedURIS => IncompletedURIS;

        /// <summary>
        /// Current number of concurrent WebRequest's being processed in the moment.
        /// </summary>
        public int NumThreads => _N;

        internal int _N = 0;

        /// <summary>
        /// TODO: Return task after complete download
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> Download()
        {
            if (Downloading || Uris == null ? true : Uris.Length == 0) {
                throw new Exception($"{GetType().FullName}.Download() cannot be invoked with property Uris set null or empty.");
                return false;
            };
            OnDownloadInvoked?.Invoke();
            _PendingUris = _Uris;
            _NumFilesRemaining = Uris.Length;
            _StartTime = DateTime.Now.Millisecond;
            _Downloading = true;
            // dispatch the appropriate amount of files depending on concurrency and amnt of files
            _InitialCount = Uris.Length;
            int n = _InitialCount >= MaxConcurrency ? MaxConcurrency : _NumFilesRemaining;
            if (n <= 0) {
                throw new Exception($"{GetType().FullName}.Download expects MaxConcurrency to be a non-negative integer.");
                return false;
            }
            List<Task<bool>> rvs = new List<Task<bool>>();
            for (int i = 0; i < n; i++) rvs.Add(_Dispatch());
            var resp = await Task.WhenAll(rvs);
            // wait if download is in process or if an error occured and you do not continue post-failure
            while (Downloading && !(DidError && !ContinueAfterFailure)) await Task.Delay(75);
            return true; // TODO: create return response from fulfiller response
        }

        /// <summary>
        /// if URI is a fulfiller, then do nothing
        /// if URI is an old fulfiller, move to Fulfiller/add pending UR
        /// if URI has never been fulfilled, do normal fulfillment process
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public override async Task<bool> Download(string uri)
        {
            if (!Downloading) {
                _Downloading = true;
                _StartTime = DateTime.Now.Millisecond;
                _InitialCount = 1;
            };
            if (_Fulfillers.ToList().Select(idf => idf.Uri == uri).ToList().Count == 1) {
                // case: queued (to be fulfilled)
            } else if (_FulfillersOld.ToList().Select(idf => idf.Uri == uri).ToList().Count == 0) {
                // case: has yet to be fulfilled
                IDownloadFulfiller idf = DownloadFulfillerFactory.CreateFromClassName(IDownloadFulfillerClassName);
                idf.Uri = uri;
                idf.DownloadPath = DownloadPath;
                idf.AbandonOnFailure = AbandonOnFailure;
                idf.Timeout = Timeout;
                _PendingUris.Add(new string[]{uri});
                _Fulfillers.Add(new IDownloadFulfiller[]{idf});
                _NumFilesRemaining += 1;
            } else {
                // case: has been fulfilled already
                // move existing fulfiller
                IDownloadFulfiller idf = _FulfillersOld.ToList().Where(idf => idf.Uri == uri).ToArray()[0];
                var v = _FulfillersOld.ToList();
                v.Remove(idf);
                _FulfillersOld = v.ToArray();
                _PendingUris.Add(new string[]{uri});
                _Fulfillers.Add(new IDownloadFulfiller[]{idf});
            }
            OnDownloadIndividualInvoked?.Invoke(uri);
            return true;
        }

        internal async Task<bool> _ReturnFalseAsync() {
            return false;
        }

        /// <summary>
        /// Syncronously dispatches a file downloader for any remaining Uris.
        /// </summary>
        internal Task<bool> _Dispatch()
        {
            if (_PendingUris == null ? true : _PendingUris.Length == 0) return _ReturnFalseAsync();
            string uri = _PendingUris[0];
            IDownloadFulfiller idf = _Fulfillers[0];
            _PendingUris = _PendingUris.Dequeue();
            _Fulfillers = _Fulfillers.Dequeue();
            _FulfillersOld = _FulfillersOld.Add(new IDownloadFulfiller[]{idf}); // add to old
            if (idf._CompletedMultipartDownload) return _ReturnFalseAsync();
            if (!idf._DidHeadReq && idf.TryMultipartDownload) {
                // case: didn't do a head request yet and we should, submit one
                UnityWebRequestAsyncOperation treq = ((UWRFulfiller)idf)._HeadRequest();
                _N++;
                treq.completed += (obj) => {
                    UnityWebRequestAsyncOperation rv = idf.Download();
                    rv.completed += resp => {
                        _N--;
                        _DispatchCompletion(idf);
                    };
                };
                return _ReturnFalseAsync();
            }
            UnityWebRequestAsyncOperation rv = idf.Download();
            if (rv == null) { // case: either multi-part completed or file is fully downloaded already
                _DispatchCompletion();
                return _ReturnFalseAsync();
            }
            _N++;
            rv.completed += resp => {
                _N--;
                _DispatchCompletion(idf);
            };
            return _ReturnFalseAsync();
        }
        
        /// <summary>
        /// Dispatches a given IDF, for multi-part downloads.
        /// </summary>
        /// <param name="idf"></param>
        /// <returns></returns>
        internal Task<bool> _Dispatch(IDownloadFulfiller idf)
        {
            string uri = idf.Uri;
            UnityWebRequestAsyncOperation rv = idf.Download();
            if (rv == null) {
                _DispatchCompletion();
                return _ReturnFalseAsync();
            }
            _N++;
            rv.completed += resp => {
                _N--;
                _DispatchCompletion(idf);
            };
            return _ReturnFalseAsync();
        }

        internal async Task _DispatchCompletion()
        {
            await _Locker.LockAsync(async () => {
                if (!Downloading) {
                    return;
                }
                if (_PendingUris.Length > 0)
                {
                    _Dispatch();   
                } else if (NumThreads == 0) {
                    // case: download completion
                     OnDownloadsSuccess?.Invoke();
                    _EndTime = DateTime.Now.Millisecond;
                    _Downloading = false;
                }
            });
        }

        /// <summary>
        /// Handles dispatch completion in a syncronous fashion (with allowable awaiting)
        /// </summary>
        /// <param name="idf"></param>
        /// <returns></returns>
        internal async Task _DispatchCompletion(IDownloadFulfiller idf)
        {
            await _Locker.LockAsync(async () => {
                if (!Downloading) {
                    // case: download pause occured after this IDF has been fulfilled
                    idf.Cancel();
                    return;
                }
                // handle idf error and 'ContiueAfterError'
                if (idf.DidError)
                {
                    if (_ContinueAfterFailure && _PendingUris.Length > 0) {
                        // case: can continue
                        _Dispatch();
                    } else if (!ContinueAfterFailure) {
                        // case: can't continue, cancel
                        await Cancel();
                    } else {
                        await Cancel();
                    }
                } else 
                {
                    if (!idf._CompletedMultipartDownload && idf.MultipartDownload) {
                        // case: continue multi-part download instead of dispatchal
                        _Dispatch(idf);
                        return;
                    } else {
                        //_FulfillersOld = _FulfillersOld.Add(new IDownloadFulfiller[]{idf}); // add to old
                    }
                    if (_PendingUris.Length > 0)
                    {
                        _Dispatch();   
                    } else if (NumThreads == 0) {
                        // case: download completion
                        OnDownloadsSuccess?.Invoke();
                        _EndTime = DateTime.Now.Millisecond;
                        _Downloading = false;
                    }
                }
            });
        }

        
        public IDownloadFulfiller GetFulfiller(string uri)
        {
            return _Fulfillers.Where(idf => idf.Uri == uri).ToArray().Length == 0 ? _FulfillersOld.Where(idf => idf.Uri == uri).ToArray()[0] : _Fulfillers.Where(idf => idf.Uri == uri).ToArray()[0];
        }

        /// <summary>
        /// Cancel all takss for this downloader asyncronously.
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> Cancel()
        {
            _Downloading = false;
            // invoke event
            OnCancel?.Invoke();

            _EndTime = DateTime.Now.Millisecond;
            _HandleAbandonOnFailure();
            return true;
        }

        /// <summary>
        /// Moves fulfiller for given URI to old if applicable
        /// Invokes cancel on fulfiller 
        /// /// </summary>
        /// <returns></returns>
        public override async Task<bool> Cancel(string uri)
        {
            OnCancelIndividual?.Invoke(uri);

            if (_Fulfillers.ToList().Select(idf => idf.Uri == uri).ToList().Count == 1) {
                // case: queued (to be fulfilled)
                IDownloadFulfiller idf = _Fulfillers.ToList().Where(idf => idf.Uri == uri).ToArray()[0];
                idf.Cancel();
                var v = _Fulfillers.ToList();
                v.Remove(idf);
                _Fulfillers = v.ToArray();
                _FulfillersOld = _FulfillersOld.Add(new IDownloadFulfiller[]{idf});
            } else if (_FulfillersOld.ToList().Select(idf => idf.Uri == uri).ToList().Count == 0) {
                // case: has never been fulfilled
                throw new Exception("Cancelation invoked for a URI that was never invoked " + uri);
                return false;
            } else {
                // case: has been fulfilled
                Debug.LogError("Cancelation invoked for a URI that has completed.");
            }
            return true;
        }

        /// <summary>
        /// Deletes all files if AbandonOnFailure is true.
        /// </summary>
        internal void _HandleAbandonOnFailure()
        {
            if (AbandonOnFailure) foreach (var idf in _Fulfillers)
            {
                idf.Cancel();
            }
            if (AbandonOnFailure) foreach (var idf in _FulfillersOld)
            {
                idf.Cancel();
            }
        }

        /// <summary>
        /// Resets all properties relating to this downloader.
        /// </summary>
        public override void Reset()
        {
            if (Downloading) {
                throw new Exception("Reset for UnityFileDownloader cannot be invoked while still Downloading. Invoke Cancel first.");
                return;
            }
            _Downloading = false;
            _Timeout = 6;
            _MaxConcurrency = 4;
            _AbandonOnFailure = true;
            _ContinueAfterFailure = false;
            _Downloading = false;
            _Paused = false;
            _DidError = false;
            _NumFilesRemaining = 0;
            _StartTime = 0;
            _EndTime = 0;
            _PendingUris = null;
            _DownloadedUris = null;
            _IncompletedUris = null;
            _Fulfillers = new IDownloadFulfiller[0];
            _FulfillersOld = new IDownloadFulfiller[0];
            _Uris = new string[0];
        }


    }
}