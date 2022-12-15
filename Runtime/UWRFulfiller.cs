using System.Threading.Tasks;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace UFD
{

    public class UWRFulfiller : IDownloadFulfiller
    {

        protected int _ExpectedSize = 0;
        protected int _ChunkSize = 0;
        


        internal int _StartTime = 0, _EndTime = 0;
        internal int _BytesDownloaded;
        internal float _Progress = 0f;
        internal int _Timeout = 6;
        internal string _Uri = null;
        internal string _DownloadPath = Application.persistentDataPath;
        internal bool _MultipartDownload = false; // set implicitly post-head request
        internal bool _AbandonOnFailure = true;
        internal bool _Paused = false;

        public event Action OnDownloadSuccess, OnCancel, OnDownloadChunkedSucces;

        public bool Completed => Progress == 1.0f;

        public override float Progress
        {
            get => _Progress;
        }

        public override int BytesDownloaded => _BytesDownloaded;
        
        public override string Uri
        {
            get => _Uri;
            set {
                _Uri = value;
            }
        }

        public override string DownloadPath
        {
            get => _DownloadPath;
            set {
                _DownloadPath = value;
            }
        }

        public override bool MultipartDownload
        {
            get => _MultipartDownload;
            set {
                _MultipartDownload = value;
            }
        }

        public override bool AbandonOnFailure
        {
            get => _AbandonOnFailure;
            set {
                _AbandonOnFailure = value;
            }
        }

        public override int Timeout {
            get => _Timeout;
            set
            {
                _Timeout = value;
            }
        }

        public override bool Paused => _Paused;

        internal bool _DidError = false;
        public override bool DidError => _DidError;

        public override int StartTime => _StartTime;
        public override int EndTime => _EndTime;

        public event Action<int, string> OnDownloadError;

        /// <summary>
        /// TODO: Implement
        /// </summary>
        /// <returns></returns>
        public override bool Cancel() {
            OnCancel?.Invoke();
            if (_AbandonOnFailure && File.Exists(DownloadResultPath)) {
                File.Delete(DownloadResultPath);
            }
            return false;
        }

        /// <summary>
        /// Submits a head request to the URI to determine if a multipart download is possible.
        /// </summary>
        /// <returns></returns>
        public UnityWebRequestAsyncOperation _HeadRequest()
        {
                // case: didn't submit a head request yet, and we should
                UnityWebRequest uwr = null;
                UnityWebRequestAsyncOperation hreq = HTTPHelper.Head(ref uwr, _Uri, RequestHeaders, _Timeout);
                _DidHeadReq = true;
                hreq.completed += (resp) => {
                    if (uwr.isNetworkError || uwr.isHttpError) {
                        UnityEngine.Debug.LogError($"URI {_Uri} does not support HEAD requests and therefore Multipart downloading.");
                        MultipartDownload = false;
                        return;
                    }
                    if (!uwr.GetResponseHeaders().ContainsKey("Content-Length") || (!uwr.GetResponseHeaders().ContainsKey("Accept-Ranges") ? true : uwr.GetResponseHeaders()["Accept-Ranges"] != "bytes"))
                    {
                       UnityEngine.Debug.LogError($"URI {_Uri} does not support Multipart downloading.");
                        return; 
                    }
                    try {
                    _ExpectedSize = Int32.Parse(uwr.GetResponseHeaders()["Content-Length"]);
                    } catch (Exception ex) {
                        UnityEngine.Debug.LogError($"URI {_Uri} does not support Multipart downloading.");
                        return;
                    }
                    _ChunkSize = IntitialChunkSize;
                    // download not multipart if size is below chunksize
                    MultipartDownload = _ExpectedSize > _ChunkSize;
                };
                return hreq;
        }

        /// <summary>
        /// Downloads the given URI in either a single or multi-part download.
        /// </summary>
        /// <returns></returns>
        public override UnityWebRequestAsyncOperation Download() {
            if (_CompletedMultipartDownload) return null;
            if (_Uri == null) return null;
            if (_DownloadPath == null) return null;
            _StartTime = DateTime.Now.Millisecond;
            UnityWebRequestAsyncOperation resp = null;
            UnityWebRequest uwr = null;
            if (!MultipartDownload) {
                resp = HTTPHelper.Download(ref uwr, Uri, _DownloadPath, AbandonOnFailure, false, RequestHeaders, _Timeout);
                resp.completed += (obj) => {
                    if (!File.Exists(DownloadResultPath)) return;
                    _Progress = 1.0f;
                    OnDownloadSuccess?.Invoke();
                    _EndTime = DateTime.Now.Millisecond;
                    _BytesDownloaded = (int) new FileInfo(DownloadResultPath).Length;
                }; // invoke completed event when it actually happens
            } else {
                try {
                int fileSize = 0;
                if (File.Exists(DownloadResultPath)) {
                    try {
                    fileSize = (int) (new FileInfo(DownloadResultPath).Length);
                    } catch (Exception ex) {
                        Debug.LogError(ex.ToString());
                        return null;
                    }
                }
                int remaining = _ExpectedSize - fileSize;
                if (remaining == 0) return null; // case: no need to download
                int reqChunkSize = _ChunkSize >= remaining ? remaining : _ChunkSize;
                if (fileSize + reqChunkSize >= _ExpectedSize) reqChunkSize = remaining; // case: _ChunkSize is smaller than remaining but greater than needed 
                if (RequestHeaders == null) RequestHeaders = new Dictionary<string, string>();
                //"Range: bytes=0-1023"
                if (RequestHeaders.ContainsKey("Range")) RequestHeaders.Remove("Range");
                string str = "";
                RequestHeaders.Add("Range", str = $"bytes={fileSize}-{fileSize + reqChunkSize}");
                resp = HTTPHelper.Download(ref uwr, Uri, _DownloadPath, AbandonOnFailure, true, RequestHeaders, _Timeout);
                resp.completed -= _OnCompleteMulti;
                resp.completed += _OnCompleteMulti;
                } catch (Exception e) {
                    Debug.LogError(e.ToString());
                }
            }
            resp.completed += (obj) => {
                if (uwr.isHttpError || uwr.isNetworkError)
                {
                    _DidError = true;
                    OnDownloadError?.Invoke(0, uwr.error);
                    Cancel();
                }
            };
            return resp;
        }


        /// <summary>
        /// This function is called when a request ( multi-chunk only) has completed
        /// </summary>
        /// <param name="obj"></param>
        internal void _OnCompleteMulti(AsyncOperation obj)
        {
            if (!File.Exists(DownloadResultPath)) return;
                    int fileSize = 0;
                if (File.Exists(DownloadResultPath)) fileSize = (int) (new FileInfo(DownloadResultPath).Length);
                OnDownloadChunkedSucces?.Invoke();
                int remaining = _ExpectedSize - fileSize;
                int reqChunkSize = _ChunkSize > remaining ? remaining : _ChunkSize;
                    _Progress = reqChunkSize / _ExpectedSize;
                    _BytesDownloaded = (int) new FileInfo(DownloadResultPath).Length;

                    if (new FileInfo(DownloadResultPath).Length == _ExpectedSize) {
                        // case: complete!
                        OnDownloadSuccess?.Invoke();
                        _EndTime = DateTime.Now.Millisecond;
                        _CompletedMultipartDownload = true;
                    } else {
                        // case: not complete!
                        // Download is invoke recursively 
                    }
        }
    }

    
}
