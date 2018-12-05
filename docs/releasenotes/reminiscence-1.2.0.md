Reminiscence 1.2.0 Release Notes
--------------------------------

This update contains a few bugfixes and enhancements to update Indexes/Dictionaries.

Changelog:

- Added 'EnsureMinimumSize' extension methods for array resizing.
- Fixed custom block size and block size cannot be zero anymore.
- Indexes can become writable again.
- Content of Indexes can be enumerated.
- Fixed a bug only occuring when updating a dictionary value when there are exactly n² collisions. This put the dictionary in an invalid state.
- Added netstandard2.0 as a target.