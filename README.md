# UnityGroupDownloader
One file for downloading of multiple files in Unity

## Wha does it do?
- One C# (.cs) file dependent on only Unity and C#. 
- Download any amount of files one at a time, using single-threaded execution.
- Simple success and error callbacks via C# [events](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/).
- Atomic file downloading.
> By specifying the 'AbandonOnFailure' property of GroupDownloader to true, a failed download will cause the downloader to pause and delete all previous files. UnityWebRequests allows the downloader to ensure a file is never partially downloaded via a [DownloadHandler](https://docs.unity3d.com/ScriptReference/Networking.DownloadHandler.html) property.
- Properties for timeouts, internal timers and keeps track of pending, completed and uncomplete URIs.
- Additional [component](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) for controling the downloader through the editor.

## How do I use it?
- Place GroupDownloader.cs in your Scripts folder, or any folder that Unity reads scripts from.
- Add GroupDownloaderComponent to any GameObject.
> Set the following properties in the inspector: DownloadPath, PendingURIS, URIToFilenameMap
A picture of this in the editor will be included.

- An example of using GroupDowloader in code:
```javascript
         // in some .cs file, maybe or maybe not using MonoBehavior
         public void DownloadFiles1() {
                  GroupDownloader downloader = new GroupDownloader();
                  
                  /* Make sure to set MonoPuppet to some MonoBehavior in order for
                     the IEnumerator to work.
                     This can be any script that uses MonoBehavior.
                  */
                  downloader.MonoPuppet = gameObject.GetComponents(typeof MonoBehavior)[0]; // using any MonoBehavior on an example GameObject
                  
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
         
         public void DownloadFiles2() {
                  GroupDownloader downloader = new GroupDownloader();
                  
                  /* Make sure to set MonoPuppet to some MonoBehavior in order for
                     the IEnumerator to work.
                     This can be any script.
                  */
                  downloader.MonoPuppet = gameObject.GetComponents(typeof MonoBehavior)[0]; // using any MonoBehavior on an example GameObject
                  
                  /* Add some URLS to download here. */
                  downloader.PendingURLS.Add("www.google.com/image/someimage.jpg");
                  
                  /* This time lets use a function to decide how each URI is named.
                     We will specify the property 'UseURIToFilenameMap' to False and then
                     
                  */
                  downloader.OnURIToFilename["www.google.com/image/someimage.jpg"] = "myimage.jpg";
                  
                  /* All files are downloaded to the DownloadPath property
                     By defaut it is set to the persistant data path of Unity
                     and then specify the delegate URIToFilename, which has a string param and returns a string
                  */
                  downloader.URIToFilename = delegate(string uri) {
                           /* 
                           URI will be called for each URI pending to be downloaded
                           And the filename will be the return value of this statement
                           */
                           return uri; // psudeo code
                  };
                  
                  
                  /* If true this stops a download in the event of a failure.
                     Lets stop this download in case of failure ;)
                  */
                  downloader.AbandonOnFailure = true;
                  
                  /* Add our events as listeners. Can register multiple. */
                  downloader.OnDownloadFailure += OnUpdateFailure;
                  downloader.OnDownloadSuccess += OnUpdateSuccess;
                  
                  
                  
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
                  
                  /* Want to cancel the downloader? Easy. */
                  downloader.Cancel();
                  
         }


         private void OnUpdateFailure(bool completed, string uri, string fileResultPath) {
                  /* Invoked when a file fails to download
                  Invoked multiple times when AbandonOnFailure = false
                  */
                  Debug.Log("Done downloading: " + completed");
                  Debug.Log("Failure. URI=" + uri + ", fileResultIfDownloaded=" + fileResultPath");
        }
         
        private void OnUpdateSuccess(bool completed, string uri, string fileResultPath) {
                  /* Invoked when a file downloads succesfully
                    Invoked multiple times when AbandonOnFailure = false
                   */
                  Debug.Log("Success! " + (completed ? "COMPLETED : "INCOMPLETE") + ");
                  Debug.Log( "URI=" + uri + ", filePath=" + fileResultPath");
        }
```




## What technologies/frameworks are involved?
- C# for the scripting language for Unity
- Unity as the cross-platform game engine
- [UnityWebRequests](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html) for fullfiling downloads

# Future improvement
- More complete and visible testing for this product is a must. While it was tested extensively, clear testing should be provided.
- Easier configuration via chained constructor-like functions. A special object used for constructing GroupDownloader's will have public methods that all returned a shared object. This allows the programmer to create objects through specifying only the functions they need in place of properties in constructors. Below is a basic example of using this programming structure:
```MyObjectCreator.Create(); // creates an MyObject without any params --> new MyObject()
   MyObjectCreator.Create().SetInt(1); // creates a MyObject with 1 set as some property
   MyObjectCreate.Create().SetInt(1).SomeProperty().YouGetIt();
```
- Use of multi-threaded file downloading.
- Remove dependence on MonoBehavior by replacing IEnumerator's with [async and await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).
- Allow use of different fullfillers of downloaders, for example [WebClient](https://www.c-sharpcorner.com/blogs/consume-webapi-using-webclient-in-c-sharp).


## What I learned.
