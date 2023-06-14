# Changelog

## [0.8.2] - 2023-06-13

- States can be properly re-used. A pooling change from last year undid that.

## [0.8.2] - 2023-06-13

- Fixed null context in CyclopsLambda.

## [0.8.1] - 2023-04-19

- Added the missing 0.8.0 change log.
- Fixed an issue with AnimationState in CyclopsAnimation where it was possible for AnimationState.speed to remain altered past its relationship with CyclopsAnimation.
- Fixed a theorectical domain reloading issue with CyclopsRoutine's use of a static CyclopsPool. Even if domain reloading is disabled, the pool will still be properly reset.

## [0.8.0] - 2023-04-16

- Removed default tags despite a few benefits they provided for testing. Tags can be added as needed for testing. Debugging will likely be updated to provide a view of both tags and class names.
- Simplified timers. They are now correctly removed when tag based removals are requested. This was past behavior that didn't make it into the new framework until just now.
- TODO: Timers should be affected by pausing as well. They behaved as such before being special cased. They even took speed into account. Hmmm.

## [0.7.0] - 2023-04-10

- Simplified: Removed constructors from all CyclopsRoutines in favor of using the public static Instantiate + InstantiateFromPool methods only.
- Added experimental and untested CyclopsAnimation.
- Future note: async/await appears to be the future of Unity C# development but is missing a decent way to automatically handle cancelation tokens (they have to be passed around and manually polled) and that complicates state management a bit. If there's a reasonably decent way to bring the benefits of the Cyclops Framework to the async/await paradigm, I'd like to do it.

## [0.6.0] - 2022-07-17

- Heavily refactored everything relying on CyclopsCommon. CyclopsCommon may be on the way out.
- Provided a foo.Next counterpart to match foo.Immediately.

## [0.5.0] - 2022-01-27

- Added optional pooling functionality to CyclopsRoutine to reduce GC pressure. It's there when you need it, easy to use, and will stay out of your way. All built-in CyclopsRoutines use it. In the case of multiple CyclopsEngine instances running on different threads, everything should remain thread-safe as well. This isn't fully tested, but all existing test cases pass. TODO: Although not required for most scenarios, providing adjustable maximum capacities for pooling and the ability to clear the pools will likely be a good thing for someone somewhere, so it's on the list.
- Added several tweening helper structs to simplify the creation of tweening oriented CyclopsRoutines.
- Added a material tweening smoke test that uses a Standard Material because Standard Materials are compatible (for testing purposes) with other pipelines.
- Added a smoke test to sanity check pooling in the sense that no exceptions are thrown when a stressful version of typical activity (that would trigger pooling) is encountered across multiple frames.
- Changed all built-in CyclopsRoutines. They all use pooling.
- Changed tweening oriented CyclopsRoutines. They all use the new helper structs.
- Removed CyclopsIterate. It was unlikely to ever be used.
- Removed various built-in CyclopsRoutine overloads including anything using a propertyName string instead of a propertyId integer.

## [0.4.0] - 2022-01-19

- Added two pause and resume smoke tests with several variants each.
- Fixed pause and resume functionality was unintentionally disabled during the refactoring process. Caught this with the new smoke tests. It's working properly now.
- Changed pooling data structures used for collections within CyclopsRoutine. Queue was replaced with ConcurrentBag for improved thread safety. The main thread is still the primary use case and that's what ConcurrentBag is optimized for... no locking overhead. Note: It's likely that built-in pooling functionality will be added to CyclopsRoutine in order to minimize allocations and garbage collection.
- Changed several routines that still weren't using const string Tag. They are now.
- Changed several CyclopsCommon sugar methods were returning a CyclopsRoutine. They now return their specific type. This provides transparency and will automatically expose any unique functionality those classes may have now or in the future.

## [0.3.0] - 2022-01-09

###Changed

- Removed CyclopsEngine sentinel code. It was used to maintain proper order in status reports but isn't really needed.
- Removed exception wrapping code in CyclopsEngine. Couldn't justify general purpose usage. Wrap from the outside if needed.

## [0.2.1] - 2022-01-07

###Fixed

- Fixed several code compatibility issues affecting earlier versions of Unity.

## [0.2.0] - 2022-01-07

###Added

- Added additional smoke tests to better validate CyclopsEngine.Immediate.Add() functionality.
- Added CyclopsEngine.MaxNestingDepth and supporting code to limit Immediate.Add() enqueuing per frame... not exactly recursive, but similar idea.
  ###Changed
- Refactored CyclopsEngine error handling events.
  ###Fixed
- Fixed queuing issues with CyclopsEngine.Immediate.Add() which itself is new to the Framework. New smoke tests caught this issue.

## [0.1.1] - 2022-01-06

###Fixed

- Fixed documentation issues.

## [0.1.0] - 2022-01-05

###Added

- This is the beginning of an effort to upgrade and refactor Cyclops Framework as a Unity package.
