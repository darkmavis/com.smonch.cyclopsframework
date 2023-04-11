# About Cyclops Framework

Cyclops Framework (for Unity) provides a tag-based approach to simplifying state management, asynchronous routines, messaging, and tweening. It plays well with others, drops nicely into existing projects, and you can use as much or little of it as you like.

Cyclops Framework predates Unity style co-routines, Javascript promises, and the various async/await frameworks now found in numerous languages. All of these frameworks were created to solve similar problems and each has its own strengths and weaknesses.

The name Cyclops is descriptive. This framework was built to provide asynchronous cyclic operations... Cyclops.

- What if you could stop, pause, and message asynchronous sequences by tag?
- What if you could insert cascading tags at arbitrary positions in a sequence?
- Why pass or poll cancelation tokens manually when a framework could handle it for you? (via tags in this case)
- What if you had unlimited tags and could decide which tags should cascade and which tags shouldn't?
- What if you need a sequence with the shape of a tree?
- What if your async routines could provide state management support?
- For asynchronous cyclic routines, OnEnter, OnFirstFrame, OnUpdate, OnLastFrame, and OnExit might be handy.
- What if you could send messages by tag to anything you'd like, no receiver required?

Cyclops Framework was built for these types of situations.
Usage should lead to greater flexibility with less code written.

This project dates back over a decade and was heavily inspired by [Cyclops Framework (for AS3)](https://github.com/darkmavis/CyclopsFramework)
from the 2010-2012 Flash/AS3 era. The AS3 framework was inspired by previous work dating back to 2007.

# Installing Cyclops Framework

Cyclops Framework can be added to a Unity project via Unity's [Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).
There are no install scripts and no unusual steps are required.

# Using Cyclops Framework

Please stay tuned! The framework is undergoing a large upgrade and refactoring. Despite it's age, the Unity version of this framework was only previously released with Voxelform in 2011. For the time being, there are a handful of smoke tests that could be used as a starting point, but it would be better to wait for purpose built examples. While the code base currently provides some documention, it's incomplete and will be improved.

That said, the AS3 version provides a very nice overview even if your coming from a Unity/C# perspective.

[Cyclops Framework (for AS3)](https://github.com/darkmavis/CyclopsFramework)

# Technical details

## Requirements

This version of Cyclops Framework should be compatible with the following versions of Unity:

- 2023 (recently tested)
- 2022
- 2021 LTS

## Package contents

The following table indicates the folder structure of the Cyclops Framework package:

| Location    | Description                                                      |
| ----------- | ---------------------------------------------------------------- |
| `<Runtime>` | Root folder containing the source for Cyclops Framework.         |
| `<Tests>`   | Root folder containing the source for testing Cyclops Framework. |

## Document revision history

| Date            | Reason                                                                                                 |
| --------------- | ------------------------------------------------------------------------------------------------------ |
| July 17, 2022   | Updated description and compatibility details.                                                         |
| January 7, 2022 | Updated Unity compatibility details after testing each version of Unity. Matches package version 0.2.1 |
| January 6, 2022 | Added mention of potentially helpful AS3 documentation. Matches package version 0.1.1                  |
| January 5, 2022 | Document created. Matches package version 0.1.0                                                        |

## License

[Apache License 2.0](LICENSE.md)
