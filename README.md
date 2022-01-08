# About Cyclops Framework

Cyclops Framework (for Unity) provides a tag-based approach to simplifying state management, asynchronous routines, messaging, and tweening.  It plays well with others, drops nicely into existing projects, and you can use as much or little of it as you like.

But why?

Imagine that you have several coroutine sequences running in parallel and you don't know how long they'll take to complete.

- What if you could tag these sequences and then stop or pause them by tag?
- What if you wanted to insert cascading tags at various points in a sequence?
- What if a sequence is shaped like a tree?
- What if you could use as many tags as you like and decide which tags should cascade and which tags shouldn't?
- What if your coroutines provided state management events such as OnEnter, OnExit, and OnUpdate?
- What if these coroutines could repeat for a number of cycles and provide cycle specific events such as OnFirstFrame and OnLastFrame?
- What if you could send messages by tag to anything you'd like, no receiver required?

Cyclops Framework was built for these types of situations.
Usage should lead to greater flexibility with less code written.

This project dates back over a decade and was heavily inspired by [Cyclops Framework (for AS3)](https://github.com/darkmavis/CyclopsFramework)
from the 2010-2012 Flash era.

# Installing Cyclops Framework

Cyclops Framework can be added to a Unity project via Unity's [Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).
There are no install scripts and no unusual steps are required.

# Using Cyclops Framework

Please stay tuned!  This is an early release and example code will follow.  For the time being, there are a handful of smoke tests that could be used as a starting point, but it would be better to wait for purpose built examples.  While the code base currently provides some documention, it's incomplete and will be improved.

That said, the AS3 version provides a decent overview even if it's not coming from a C# / Unity perspective.  If you're not familiar with AS3, it was an OOP form of JS used by Flash and could easily be confused with TypeScript.

[Cyclops Framework (for AS3)](https://github.com/darkmavis/CyclopsFramework)

# Technical details
## Requirements

This version of Cyclops Framework is compatible with the following versions of Unity:

* 2021
* 2020 LTS
* 2019 LTS

## Package contents

The following table indicates the folder structure of the Cyclops Framework package:

|Location|Description|
|---|---|
|`<Runtime>`|Root folder containing the source for Cyclops Framework.|
|`<Tests>`|Root folder containing the source for testing Cyclops Framework.|

## Document revision history

|Date|Reason|
|---|---|
|January 7, 2022|Updated Unity compatibility details after testing each version of Unity. Matches package version 0.2.1|
|January 6, 2022|Added mention of potentially helpful AS3 documentation. Matches package version 0.1.1|
|January 5, 2022|Document created. Matches package version 0.1.0|

## License

[Apache License 2.0](LICENSE.md)
