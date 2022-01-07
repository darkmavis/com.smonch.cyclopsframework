# Changelog
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
