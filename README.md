<h1 align="center">UnityFileDownloader☁️</h1>
<p align="center">
  <a href="https://www.npmjs.com/package/readme-md-generator">
    <img alt="downloads" src="https://img.shields.io/npm/dm/readme-md-generator.svg?color=blue" target="_blank" />
  </a>
  <a href="https://github.com/kefranabg/readme-md-generator/blob/master/LICENSE">
    <img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-yellow.svg" target="_blank" />
  </a>
</p>

A file downloader that handles asyncronous and concurrent file downloading, implicit multi-part file downloading, pausing/restarting, properties with per-file granularity and more.

## Appendix

This file downloader was built because UnityWebRequest and recent solutions built by Unity and third-parties are subpar and bare of features. While this project is written 100% in C#, this downloader should support all platforms that Unity does. If you have any feedback, please reach out to me at jpgordon00@gmail.com, or submit an issue or pull request. Collaboration is encouraged! 

## Features

- Concurrently download any amount of files while adhering to a fixed number of asyncronous tasks
- Implicitly handles multi-part file downloading, including pausing/restarting of chunked downloads
- Provides both task-based asyncronous methods and standard callbacks for errors, individual file completion, all file completion, and more
- Granularity of properties can be on a per-file basis, providing the ability to pause/restart, and to get/set progress, timeouts, headers, callbacks and more
- Optionally handle "atomic" file downloading by deleting all downloaded (partially or impartially) files and ability to continue downloading through HTTP/S errors
- Code structure that allows for modularity of both download fulfillment (such as UnityWebRequest) and dispatching of downloads.

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

## Issues
- Unity Editor termination and application lifecycle methods do not stop this downloader from functioning.

## TODO
The following features are some of which I deem to be deseriable and not currently implemented:

- Dynamic chunk sizing based on download speeds
- Cleanup of class structure to provide complete modularity of download fulfillment and downloader.
- Provide alternative fulfillers to UnityWebRequest, perhaps unlocking better platform specific performance.
