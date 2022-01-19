# Changelog
## [0.4.0] - 2022-01-19
- Added two pause and resume smoke tests with several variants each.
- Fixed pause and resume functionality was unintentionally disabled during the refactoring process.  Caught this with the new smoke tests.  It's working properly now.
- Changed pooling data structures used for collections within CyclopsRoutine.  Queue was replaced with ConcurrentBag for improved thread safety.  The main thread is still the primary use case and that's what ConcurrentBag is optimized for... no locking overhead.  Note: It's likely that built-in pooling functionality will be added to CyclopsRoutine in order to minimize allocations and garbage collection.
- Changed several routines that still weren't using const string Tag.  They are now.
- Changed several CyclopsCommon sugar methods were returning a CyclopsRoutine.  They now return their specific type.  This provides transparency and will automatically expose any unique functionality those classes may have now or in the future.
## [0.3.0] - 2022-01-09
###Changed
- Removed CyclopsEngine sentinel code.  It was used to maintain proper order in status reports but isn't really needed.
- Removed exception wrapping code in CyclopsEngine.  Couldn't justify general purpose usage.  Wrap from the outside if needed.
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
- Fixed queuing issues with CyclopsEngine.Immediate.Add() which itself is new to the Framework.  New smoke tests caught this issue.
## [0.1.1] - 2022-01-06
###Fixed
- Fixed documentation issues.
## [0.1.0] - 2022-01-05
###Added
- This is the beginning of an effort to upgrade and refactor Cyclops Framework as a Unity package.
