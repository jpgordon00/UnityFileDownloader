
<h1 align="center">UnityFileDownloader☁️</h1>
<p align="center">
  <a href="https://www.npmjs.com/package/readme-md-generator">
    <img alt="downloads" src="https://img.shields.io/npm/dm/readme-md-generator.svg?color=blue" target="_blank" />
  </a>
  <a href="https://github.com/kefranabg/readme-md-generator/blob/master/LICENSE">
    <img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-yellow.svg" target="_blank" />
  </a>
</p>


A file downloader that handles asyncronous and concurrent file downloading, implicit multi-part file downloading, pausing/restarting, and more.


## Appendix

This file downloader was built because UnityWebRequest and recent solutions built by Unity and third-parties are subpar. Please submit any pull-requests that you deem to be a necessary contribution.
## Features

- Concurrently download any amount of files while adhering to a fixed number of "threads"
- Implicitly handles multi-part file downloading, including pausing/restarting
- Provides both task-based asyncronous methods and standard callbacks for errors, individual file completion, all file completion, and more
- Granularity of properties can be on a per-file basis, providing the ability to pause/restart, and to get/set progress, timeouts, callbacks and more
- Optional "atomic" file downloading and ability to continue downloading through HTTP/S errors
- Code structure that allows for modularity of download both fulfillment (such as UnityWebRequest) and dispatching of downloads.

## Usage/Examples
```javascript
using UFD;
using System;

// ... in some class
public class Example
{
    async void Download()
    {
        UnityFileDownloader ufd = new UnityFileDownloader(new string[] {
            "https://wallpaperaccess.com/full/87731.jpg",
        });
        await ufd.Download();
    }

}
```
```javascript
using UFD;
using System;

// ... in some class
public class Example
{
    async void Download()
    {
        UnityFileDownloader ufd = new UnityFileDownloader(new string[] {
            "https://wallpaperaccess.com/full/87731.jpg",
            "https://wallpaperaccess.com/full/1126085.jpg",
            "https://wallpaperaccess.com/full/1308917.jpg",
            "https://wallpaperaccess.com/full/281585.jpg",
        });
        ufd.AbandonOnFailure = true;
        ufd.ContinueAfterFailure = false;
        ufd.MaxConcurrency = 3;
        ufd.OnDownloadSuccess += (string uri) => {
            Debug.Log("Downloaded " + uri + "!");

            if (true) { // dummy
                ufd.Cancel();
            } 
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

}
```

## TODO

If you have any feedback, please reach out to me at jpgordon00@gmail.com.
The following features are some of which I deem to be deseriable and not currently implemented:
-  Dynamic chunk sizing based on download speeds
-  Cleanup of class structure to provide complete modularity of download fulfillment and downloader. 

