using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

namespace UFD
{

    /// <summary>
    /// Represents a class that should be capable of downloading files from a public endpoint.
    /// </summary>
    public abstract class IDownloadFulfiller
    {
        /// <summary>
        /// Amount of bytes to chunk each request by
        /// </summary>
        public int IntitialChunkSize = 200000;
        
        public bool _CompletedMultipartDownload = false;
        public bool _DidHeadReq = false;


        /// <summary>
        /// Returns true when this fulfiller has completed.
        /// </summary>
        public bool Completed => Progress == 1.0f;

        /// <summary>
        /// Returns the progress, if any, on this download, from 0f to 1f.
        /// </summary>
        /// <value></value>
        public abstract float Progress
        {
            get;
        }

        /// <summary>
        /// Returns the URI associated with this fulfiller, if its not set upon 'Download'.
        /// </summary>
        /// <value></value>
        public abstract string Uri
        {
            get; set;
        }

        public abstract int BytesDownloaded
        {
            get;
        }

        /// <summary>
        /// Headers to be used for any dispatched requests.
        /// </summary>
        public Dictionary<string, string> RequestHeaders = null;

        /// <summary>
        /// The download path to download to, excluding the file name.
        /// </summary>
        /// <value></value>
        public abstract string DownloadPath
        {
            get; set;
        }

        public string DownloadResultPath => (Uri == null || DownloadPath == null) ? null : Path.Combine(DownloadPath, HTTPHelper.GetFilenameFromUriNaively(Uri)).Replace("/", Path.DirectorySeparatorChar.ToString());

        /// <summary>
        /// Returns true if this fulfiller can expect to download this file in chunks.
        /// This field is determined implicitly when downloading the file based on the 'ChunkSize'.
        /// </summary>
        /// <value></value>
        public abstract bool MultipartDownload
        {
            get; set;
        }

        /// <summary>
        /// Explicitly checks for Multipart downloads only if this bool is true.
        /// </summary>
        public bool TryMultipartDownload = true;

        /// <summary>
        /// Set to true via the 'Pause' function and set to false by 'Download'. 
        /// </summary>
        /// <value></value>
        public abstract bool Paused
        {
            get;
        }

        /// <summary>
        /// True to remove the entire file upon a network error, even if it was partially downloaded.
        /// </summary>
        /// <value></value>
        public abstract bool AbandonOnFailure
        {
            get; set;
        }

        public abstract bool DidError
        {
            get;
        }

        public abstract int StartTime
        {
            get; 
        }

        public bool Downloading => StartTime != 0;

        public abstract int EndTime
        {
            get;
        }

        public abstract int Timeout
        {
            get; set;
        }

        public int ElapsedTime => Math.Abs(StartTime == 0 ? 0 : (EndTime == 0 ? DateTime.Now.Millisecond - StartTime : EndTime - StartTime));

        public float MegabytesDownloadedPerSecond => (BytesDownloaded / 1000) / ((ElapsedTime / 1000) == 0 ? 1 : (ElapsedTime / 1000));

        /// <summary>
        /// If this fulfiller has `MultipartDownload` set to true, then pause the download.
        /// Returns true if this downloader was succesfully paused.
        /// </summary>
        /// <returns></returns>
        public abstract bool Cancel();


        /// <summary>
        /// Initiates the download (or unpauses it), given the existing 'URI' property
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public abstract UnityWebRequestAsyncOperation Download();
    }
    
}