using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UFD
{
    public abstract class IDownloader
    {
        /// <summary>
        /// Constructs a IDownloader-child object with the given URIs.
        /// </summary>
        /// <param name="uris"></param>
        public IDownloader(params string[] uris)
        {
            Uris = uris;
        }

        /// <summary>
        /// Stores the Uri's that this downloader will attempt to download.
        /// </summary>
        protected string[] _Uris;

        /// <summary>
        /// Stores the URI's that this downloader succesfully downloaded.
        /// </summary>
        protected string[] _DownloadedUris;

        /// <summary>
        /// Stores the Uri's that this downloader has failed to download.
        /// </summary>
        protected string[] _IncompletedUris;

        /// <summary>
        /// Headers to be used for any dispatched requests.
        /// </summary>
        public Dictionary<string, string> RequestHeaders = null;

        public bool TryMultipartDownload = true;

        /// <summary>
        /// Stores the actual IDownloadFulfiller's associated with each file that is to be downloaded.
        /// </summary>
        protected IDownloadFulfiller[] _Fulfillers = new IDownloadFulfiller[0];

        protected IDownloadFulfiller[] _FulfillersOld = new IDownloadFulfiller[0];

        /// <summary>
        /// Using the 'IDownloadFulfillerFactory', this string will be passed into the 'CreateFromClassName' function to construct 'Fulfillers' upon 'Uris' to assignment.
        /// </summary>
        public virtual string IDownloadFulfillerClassName => "UWRFulfiller";

        /// <summary>
        /// Invoked upon error, where the first parameter is the uri, the second is the error code, and the third is the error message.
        /// </summary>
        public event Action<string, int, string> OnDownloadError;

        /// <summary>
        /// Invoked when a file has been downloaded, with param name being the uri
        /// </summary>
        public event Action<string> OnDownloadSuccess, OnDownloadChunkedSucces;

        /// <summary>
        /// Initiates a download from 'Uris'.
        /// </summary>
        public abstract Task<bool> Download();

        /// <summary>
        /// Initiates a download for a specific URI
        /// This is used to either target additional URI's or to uncancel a specific URI that was downloading.
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> Download(string uri);

        /// <summary>
        /// Pauses the downloader for a specific URI, if applicable.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public abstract Task<bool> Cancel(string uri);

        /// <summary>
        /// Cancels this downloader, if applicable.
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> Cancel();



        public abstract void Reset();

        /// <summary>
        /// Calculates progress based on 'Fulfillers' from 0f to 1f.
        /// </summary>
        /// <value></value>
        public float Progress
        {
            get {
                // progress = (allProg) / numFiles
                float prog = 0;
                float num = NumFilesTotal;
                foreach (var idf in _Fulfillers) prog += idf.Progress;
                return prog;
            }
        }

        /// <summary>
        /// Calculates the total amount of files to download based on 'Fulfillers'.
        /// </summary>
        /// <value></value>
        public int NumFilesTotal => _Fulfillers.Length;

        /// <summary>
        /// Returns true when this downloader has completed.
        /// </summary>
        public bool Completed => Progress == 1.0f;

        public int MultipartChunkSize = 200000;

        public abstract string DownloadPath
        {
            get; set;
        }

        /// <summary>
        /// Gets progress associated with a specific URI, if it exists.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public float GetProgress(string uri)
        {
            if (!_Uris.ToList().Contains(uri)) return 0f;
            return _Fulfillers.ToList().Where(idf => idf.Uri == uri).ToArray().Length == 1 ? _Fulfillers.ToList().Find(idf => idf.Uri == uri).Progress : _FulfillersOld.ToList().Find(idf => idf.Uri == uri).Progress;
        }

        /// <summary>
        /// Set upon construction or manually, but a 'Download' invokation must follow.
        /// Checks URI for basic parsing before allowing.
        /// </summary>
        public string[] Uris {
            get => _Uris;
            set {
                List<string> list = new List<string>();
                //foreach (var idf in _Fulfillers) idf.Dispose();
                _Fulfillers = null;
                List<IDownloadFulfiller> fulfillers = new List<IDownloadFulfiller>();
                foreach (var str in value)
                {
                    try {
                        Uri uri = new Uri(str); // dummy way of checking for valid URIs
                    } catch (Exception ex)
                    {
                        throw new Exception($"URI {str} cannot be fed into {GetType().Name}.Uris");
                        return;
                    }
                    IDownloadFulfiller idf = DownloadFulfillerFactory.CreateFromClassName(IDownloadFulfillerClassName);
                    idf.Uri = str;
                    idf.DownloadPath = DownloadPath;
                    idf.AbandonOnFailure = AbandonOnFailure;
                    idf.Timeout = Timeout;
                    idf.RequestHeaders = RequestHeaders;
                    idf.TryMultipartDownload = TryMultipartDownload;
                    idf.IntitialChunkSize = MultipartChunkSize; 
                    fulfillers.Add(idf);
                    PendingURIS.Add(str);

                    /// <summary>
                    /// Invok action on parent
                    /// </summary>
                    /// <returns></returns>
                     ((UWRFulfiller) idf).OnDownloadChunkedSucces += () => {
                        OnDownloadChunkedSucces?.Invoke(idf.Uri);
                    };
                    
                    /// <summary>
                    /// Handle AbandonOnFailure, updating 'DidError' and dispatching 'OnError', and IncompleteUris
                    /// </summary>
                    /// <param name="errorCode"></param>
                    /// <param name="errorMsg"></param>
                    /// <returns></returns>
                    ((UWRFulfiller) idf).OnDownloadError += async (int errorCode, string errorMsg) => {
                        DidError = true;
                        OnDownloadError?.Invoke(idf.Uri, errorCode, errorMsg);
                        _IncompletedUris.Add(new string[]{idf.Uri});
                    };

                    /// <summary>
                    /// Updates 'DownloadedURIS' asyncronously
                    /// TODO: avoid casting by keeping event in parent class
                    /// </summary>
                    /// <returns></returns>
                    ((UWRFulfiller) idf).OnDownloadSuccess += () => {
                      _DownloadedUris.Add(new string[]{idf.Uri});  
                      OnDownloadSuccess?.Invoke(idf.Uri);
                    };
                    list.Add(str);
                }
                _Uris = list.ToArray();
                _Fulfillers = fulfillers.ToArray();
            }
        }

        public abstract int Timeout
        {
            get; set;
        }

        public abstract int MaxConcurrency
        {
            get; set;
        }

        /// <summary>
        /// Returns the amount of files this downloader processes per second, only counting time after a Download initiates and before it begins.
        /// </summary>
        /// <value></value>
        public abstract float NumFilesPerSecond
        {
            get;
        }

        /// <summary>
        /// If true, then this downloader will delete all partially downloaded files.
        /// </summary>
        /// <value></value>
        public abstract bool AbandonOnFailure
        {
            get; set;
        }

        /// <summary>
        /// If true, then this downloader will continue attempting to download files post-error.
        /// </summary>
        /// <value></value>
        public abstract bool ContinueAfterFailure
        {
           get; set;
        }


        public abstract bool Downloading
        {
            get;
        }

        public abstract bool Paused
        {
            get;
        }

        public abstract bool DidError
        {
            get; protected set;
        }

        public abstract int NumFilesRemaining
        {
            get;
        }

        public abstract int StartTime
        {
            get;
        }

        public abstract int EndTime
        {
            get;
        }

        public int ElapsedTime => Math.Abs(StartTime == 0 ? 0 : (EndTime == 0 ? DateTime.Now.Millisecond - StartTime : EndTime - StartTime));

        public abstract string[] PendingURIS
        {
            get;
        }

        public string[] DownloadedUris => _DownloadedUris;

        public abstract string[] IncompletedURIS
        {
            get;
        }
    }

}