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
- An example of using GroupDowloader in code:
'''
         private void OnUpdateFailure(bool completed, string uri, string fileResultPath) {
        }

        private void OnUpdateSuccess(bool completed, string uri, string fileResultPath) {
        }
'''


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
