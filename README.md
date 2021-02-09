# UnityGroupDownloader
One class for easily downloading multiple files at a time.

## Wha does it do?
- One C# file.
> The file is dependent only on Unity and C#.
- Download any amount of files one at a time, using single-threaded execution.
- Simple success and error callbacks via C# [events](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/).
- Atomic file downloading.
> By specifying the 'AbandonOnFailure' property of GroupDownloader to true, a failed download will cause the downloader to cancel and delete all previous files. UnityWebRequests allows the downloader to ensure a file is never partially downloaded via a [DownloadHandler](https://docs.unity3d.com/ScriptReference/Networking.DownloadHandler.html) property.
- Progress calculation.
> Access the current progress of the downloader as a float 0f to 1f.
- Properties to change timeouts, access elapsed and total time, and more.
> Access pending, completed and uncomplete URIs. 
- Additional [component](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) for controling the downloader through the editor.

## How do I use it?
- Place GroupDownloader.cs in your Scripts folder, or any folder that Unity reads scripts from.
- Below is an example of using GroupDownloaderComponent by placing it on any GameObject
> Set the following properties in the inspector: DownloadPath, PendingURIS, URIToFilenameMap. The DataPath does not need to be set.


![](https://i.gyazo.com/76904f86bacc0d49f6686ee52579e29e.png)

- Below is an example of using GroupDowloader in code:
```javascript
         // in some .cs file, maybe or maybe not using MonoBehavior
         public void DownloadFiles1() {
                  GroupDownloader downloader = new GroupDownloader();
                  
                  /* Make sure to set MonoPuppet to some MonoBehavior in order for
                     the IEnumerator to work.
                     This can be any script that uses MonoBehavior.
                  */
                  downloader.MonoPuppet = this;
                  
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
- Below is another example:
```javascript
public void DownloadFiles2() {
                  GroupDownloader downloader = new GroupDownloader();
                  
                  /* Setup Success callback inline*/
                  downloader.OnDownloadSuccess += (bool completed, string uri, string fileResultPath) => {
                           /* Invoked when a file downloads succesfully
                           Invoked multiple times when AbandonOnFailure = false
                           */
                           Debug.Log("Success! " + (completed ? "COMPLETED : "INCOMPLETE") + ");
                           Debug.Log( "URI=" + uri + ", filePath=" + fileResultPath");
                  }
                  
                  /* Do the same for an error callback inline */
                  downloader.OnDownloadFail += (bool completed, string uri, string fileResultPath) => {
                           /* Invoked when a file fails to download
                           Invoked multiple times when AbandonOnFailure = false
                           */
                           Debug.Log("Done downloading: " + completed");
                           Debug.Log("Failure. URI=" + uri + ", fileResultIfDownloaded=" + fileResultPath");
                  }
                  
                  /* Make sure to set MonoPuppet to some MonoBehavior in order for
                     the IEnumerator to work.
                     This can be any script.
                  */
                  downloader.MonoPuppet = this; // using any MonoBehavior on an example GameObject
                  
                  /* Add some URLS to download here. */
                  downloader.PendingURLS.Add("www.google.com/image/someimage.jpg");
                  
                  /* This time lets use a function to decide how each URI is named.
                     We will specify the property 'UseURIToFilenameMap' to False
                     
                  */
                  downloader.UseURIFilenameMap = false;
                  
                  /* All files are downloaded to the DownloadPath property
                     By defaut it is set to the persistant data path of Unity
                     Spicify the delegate URIToFilename, which has a string param and returns a string
                  */
                  int i = 0;
                  downloader.URIToFilename = delegate(string uri) {
                           /* 
                           URI will be called for each URI pending to be downloaded
                           And the filename will be the return value of this statement
                           */
                           return "myfile" + i++; // psudeo code
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
- Remove dependence on MonoBehavior by replacing IEnumerator's with [async and await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).
- Allow use of different fullfillers of downloaders, for example [WebClient](https://www.c-sharpcorner.com/blogs/consume-webapi-using-webclient-in-c-sharp).


## What I learned.
