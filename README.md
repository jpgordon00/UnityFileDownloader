# UnityGroupDownloader
Single-threaded atomic downloading of multiple files with UnityWebRequest 

## What does it do?
- Download any amount of files one at a time, using single-threaded execution.
- Simple success and error callbacks via [events](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/).

## How do I use it?

## What technologies/frameworks are involved?
- C# for the scripting language for Unity
- Unity as the cross-platform game engine
- UnityWebRequests for fullfiling downloads

# Future improvement
> 
- Easier configuration via chained constructor-like functions. A special object used for constructing GroupDownloader's will have public methods that all returned a shared object. This allows the programmer to create objects through specifying only the functions they need in place of properties in constructors. Below is a basic example of using this programming structure:
```MyObjectCreator.Create(); // creates an MyObject without any params --> new MyObject()
   MyObjectCreator.Create().SetInt(1); // creates a MyObject with 1 set as some property
   MyObjectCreate.Create().SetInt(1).SomeProperty().YouGetIt();
```

- Remove dependence on MonoBehavior by replacing IEnumerator's with [async and await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).
- Allow use of different fullfillers of downloaders, for example [WebClient](https://www.c-sharpcorner.com/blogs/consume-webapi-using-webclient-in-c-sharp).


## What I learned.
