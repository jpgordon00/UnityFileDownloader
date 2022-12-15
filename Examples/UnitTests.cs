using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFD;
using System;
using System.IO;

/// <summary>
/// Unit-Tests the behaviours in 'UnityFileDownloader'.
/// </summary>
public class UnitTests : MonoBehaviour
{
    async void Download()
    {
        UnityFileDownloader ufd = new UnityFileDownloader(new string[] {
            "https://wallpaperaccess.com/full/87731.jpg",
            "https://wallpaperaccess.com/full/1126085.jpg",
            "https://wallpaperaccess.com/full/1308917.jpg",
            "https://wallpaperaccess.com/full/281585.jpg",
        });
        ufd.RequestHeaders = new Dictionary<string, string>{
          ["key"] = "value"
        };
        ufd.AbandonOnFailure = true;
        ufd.ContinueAfterFailure = false;
        ufd.MaxConcurrency = 3;
        ufd.DownloadPath = "C://";
        ufd.TryMultipartDownload = true; // false to disable multipart
        ufd.OnDownloadSuccess += (string uri) => {
            Debug.Log("Downloaded " + uri + "! Total progress is " + ufd.Progress + "%");
            IDownloadFulfiller idf = ufd.GetFulfiller(uri);
            Debug.Log("This download was " + (ufd.MultipartDownload ? "" : "NOT ") + "downloaded in multiparts.");

            if (true) { // dummy
                ufd.Cancel();
            }
        };
        // only if multipart is enabled for this uri
        ufd.OnDownloadChunkedSucces += (uri) {
            Debug.Log("Progress for " + uri + " is " + ufd.GetProgress(uri));
        };
        ufd.OnDownloadsSuccess += () => {
            Debug.Log("Downloaded all files. (inline func)");
        };
        ufd.OnDownloadError += (string uri, int errorCode, string errorMsg) =>
        {
            Debug.Log($"ErrorCode={errorCode}, EM={errorMsg}, URU={uri}");
        };
        await ufd.Download();
        Debug.Log("Downloaded all files. (post-awaitable Download invokation)");
        Debug.Log("MB/S = " + ufd.MegabytesDownloadedPerSecond);
    }
   
 void Start() {
   Download();
 }

}

}
