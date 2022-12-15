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
        ufd.MaxConcurrency = 3;
        ufd.OnDownloadSuccess += (string uri) => {
            Debug.Log("Downloaded " + uri + "!");

            if (true)
            {   // dummy
                //ufd.Cancel();
            }
        };
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
        Debug.Log(ufd.IntitialChunkSize); // amount of bytes used by each chunked https request 
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
