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
        Debug.Log("00000000");
        UnityFileDownloader ufd = new UnityFileDownloader(new string[] {
            "https://wallpaperaccess.com/full/87731.jpg",
            "https://wallpaperaccess.com/full/1126085.jpg",
            "https://wallpaperaccess.com/full/1308917.jpg",
        });
        ufd.AbandonOnFailure = true;
        ufd.ContinueAfterFailure = false;
        ufd.MaxConcurrency = 2; // Unity does impose an asyncronous limit so I'd keep this under 10 
        ufd.OnDownloadSuccess += (string uri) => {
            Debug.Log("Downloaded " + uri + "!");

            if (true)
            {   // dummy
                //ufd.Cancel();
            }
        };
        ufd.OnDownloadChunkedSucces += (uri) => {
            IDownloadFulfiller idf = ufd.GetFulfiller(uri);
            Debug.Log("Progress for " + uri + " is " + ufd.GetProgress(uri) + ", and " + idf.BytesDownloaded + " bytes downloaded, " + idf.MegabytesDownloadedPerSecond + " mb/s.");
        };
        ufd.OnDownloadsSuccess += () => {
            Debug.Log("Downloaded all files. (inline func)");
        };
        ufd.OnDownloadError += (string uri, int errorCode, string errorMsg) =>
        {
            Debug.Log($"ErrorCode={errorCode}, EM={errorMsg}, URU={uri}");
        };
        Debug.Log(ufd.MultipartChunkSize); // amount of bytes used by each chunked https request 
        await ufd.Download();
        Debug.Log("Downloaded all files. (post-awaitable Download invokation)");
        Debug.Log("MB/S = " + ufd.MegabytesDownloadedPerSecond);
        Debug.Log(ufd.DownloadPath);
    }

void Start()
    {
        Download();
    }
}
