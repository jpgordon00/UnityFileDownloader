# UnityFileDownloader

<h1 align="center">UnityFileDownloader☁️</h1>
<p align="center">
  <a href="https://www.npmjs.com/package/readme-md-generator">
    <img alt="downloads" src="https://img.shields.io/npm/dm/readme-md-generator.svg?color=blue" target="_blank" />
  </a>
  <a href="https://github.com/kefranabg/readme-md-generator/blob/master/LICENSE">
    <img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-yellow.svg" target="_blank" />
  </a>
</p>

> One C# class that lets you concurrently download multiple files from any HTTPS endpoint using [UnityWebRequest](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html).

## What does it do?
- Download any amount of files concurrently! Its super fast!
> The call to Download happens asyncronously and it dispatches multiple web requests at a time.
- The 'MaxConcurency' property refers to the amount of web requests that can be made at the same time. The default is 8 but you can set it to any nonnegative integer.
- Simple success and error callbacks via C# [events](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/).
- Atomic file downloading.
> By specifying the 'AbandonOnFailure' property of GroupDownloader to true, a failed download will cause the downloader to cancel and delete all previous files. UnityWebRequests allows the downloader to ensure a file is never partially downloaded via a [DownloadHandler](https://docs.unity3d.com/ScriptReference/Networking.DownloadHandler.html) property.
- Progress calculation.
> Access the current progress of the downloader as a float 0f to 1f.
- Properties to change timeouts, access elapsed and total time, and more.
> Access pending, completed and uncomplete URIs as properties in real-time. 
- Additional [component](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) for controling the downloader through the editor.

## How do I use it?
- Place GroupDownloader.cs in your Scripts folder, or any folder that Unity reads scripts from.
- Below is an example of using GroupDownloaderComponent by placing it on any GameObject
> Set the properties in the inspector: PendingURIS, UriFilename.

> You do NOT need to set Download Path. It defaults to Application.persistantDataPath


![](https://i.gyazo.com/76904f86bacc0d49f6686ee52579e29e.png)

- Below is an example of using GroupDowloader in code:
```csharp
import UFD;
...
         // in some .cs file, maybe or maybe not using MonoBehavior
        public void DownloadFiles1() {
          GroupDownloader downloader = new GroupDownloader();
          
          
          // change the amount of files it can download at once
          downloader.MaxConcurrency = 5;

          /* Add some URLS to download here. */
          downloader.PendingURLS.Add("www.google.com/image/someimage.jpg");

          /* By default we will use a map of filenames to URIS to name each image.
             The key for each pair is the URI and the value would be the filename.
             The filename is appended to the DownloadPath.
          */
          downloader.OnURIToFilename["www.google.com/image/someimage.jpg"] = "myimage.jpg";

          /* All files are downloaded to the DownloadPath property
             By defaut it is set to the persistant data path of Unity
          */

          /* If true this stops a download in the event of a failure.
             The default value of this property is false
          */
          downloader.AbandonOnFailure = false;

          /* Add our events as listeners. Can register multiple. */
          downloader.OnDownloadFailure += OnUpdateFailure;
          downloader.OnDownloadSuccess += OnUpdateSuccess;

          /* Starts the download*/
          downloader.Download();
        }

        private void OnUpdateFailure(bool completed, string uri, string fileResultPath) {
            /* Invoked when a file fails to download
            Invoked multiple times when AbandonOnFailure = false
            */
         }

         private void OnUpdateSuccess(bool completed, string uri, string fileResultPath) {
               /* Invoked when a file downloads succesfully
               Invoked multiple times when AbandonOnFailure = false
               */
           }
```
- Below is another example:
```csharp
import UFD;
...
public void DownloadFiles2() {
    GroupDownloader downloader = new GroupDownloader();

    /* Setup Success callback inline*/
    downloader.OnDownloadSuccess += (bool completed, string uri, string fileResultPath) => {
        /* Invoked when a file downloads succesfully
            Invoked multiple times when AbandonOnFailure = false
            */
            }

            /* Do the same for an error callback inline */
            downloader.OnDownloadFail += (bool completed, string uri, string fileResultPath) => {
              /* Invoked when a file fails to download
              Invoked multiple times when AbandonOnFailure = false
              */
              }

              /* Add some URLS to download here. */
              downloader.PendingURLS.Add("
                www.google.com / image / someimage.jpg ");

                /* This time lets use a function to decide how each URI is named.
                   We will specify the property 'UseURIToFilenameMap' to False
                   
                */
                downloader.UseURIFilenameMap = false;

                /* All files are downloaded to the DownloadPath property
                   By defaut it is set to the persistant data path of Unity
                   Spicify the delegate URIToFilename, which has a string param and returns a string
                */
                int i = 0; downloader.URIToFilename = delegate(string uri) {
                  /* 
                  URI will be called for each URI pending to be downloaded
                  And the filename will be the return value of this statement
                  */
                  return "
                  myfile " + i++; // psudeo code
                };

                /* If true this stops a download in the event of a failure.
                   Lets stop this download in case of failure ;)
                */
                downloader.AbandonOnFailure = true;

                /* Starts the download*/
                downloader.Download();

                /* Did the downloader encounter any errors? /*
                if (downloader.DidError()) {} // some code
                
                /* Did the downloader finish? */
                if (downloader.DidFinish()) {} // some code

                /* Time in miliseconds the downloader started.*/
                int startTime = downloader.StartTime;

                /* Time in miliseconds the downloader finished
                   0 if not finished
                */
                int endTime = downloader.EndTime;

                /* Total time taken to downloader
                   OR current time downloading if currently downloading
                */
                int elapsed = downloader.Elapsed;

                /* Gets the current progress as a percent float 
                   Where 0.0 is 0% and 1.0 is %100
                */
                float prog = downloader.Progress;

                /* Want to cancel the downloader? Easy. */
                downloader.Cancel();

              }
```



## What technologies/frameworks are involved?
- C# for the scripting language for Unity
- Unity as the cross-platform game engine
- [UnityWebRequests](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html) for fullfiling downloads

# Future improvements.
- More complete and visible testing for this product is a must. 
> While it was tested extensively, clear testing should be provided.
- Better component for editor use.
> While the provided GroupDownloaderComponent works as described, it does not allow access to add listeners in the editor. There should also be an option to use the URI to filename function by assigning functions to it in the editor.
- Easier configuration via chained constructor-like functions. 
> An object is used for constructing GroupDownloader's  and will have public methods that all returned its own instance. This allows the programmer to create objects through specifying only the functions they need in place of properties in constructors. Below is a basic example of using this programming structure:
```MyObjectCreator.Create(); // creates an MyObject without any params --> new MyObject()
   MyObjectCreator.Create().SetInt(1); // creates a MyObject with 1 set as some property
   MyObjectCreate.Create().SetInt(1).SomeProperty().YouGetIt();
```
- More accurate progress calculation. 
> Currently, progress is calculated linearily with each succesful file increasing the progress by a porportional amount.
> Each file should increase the progress by a weighted percentage instead. If file progress is calculated by file size instead of number of files, progress would be more near the actual amount.
- Use of multi-threaded file downloading.
- ~~Remove dependence on MonoBehavior by replacing IEnumerator's with [async and await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).~~
- Allow use of different fullfillers of downloaders, for example [WebClient](https://www.c-sharpcorner.com/blogs/consume-webapi-using-webclient-in-c-sharp).


## What I learned.
- Extensive testing of all new products should be required.
> Software needs to be tested with many different conditions that could simulate a real work load. How can a product's developer ever confidently enforce high quality standards without actually testing the product? In other words, how can another developer be certain your product works in the ways it is described in without public testing? In most cases, a clearly defined unit test should suffice as long as it deliberately tests many use cases.
- Modular software is better than nonmodular software. 
> While this repository was created to solve a specific problem, this project evolved over time as I updated and added features. Modular components decrease the amount of time it takes to repair, find or add features. This software is does not require modular components due to the problems inherit incomplexity, though it could benefit from some additional modular components. For example, on the list of future features for this project is a need for different download fullfillers. Instead of directly using UnityWebRequest in the projects code, a modular solution would be to wrap the desired features in an interface or some sort of class. Adding a new download fullfiler would be done in the exact same way the code implements its own UnityWebRequest.
