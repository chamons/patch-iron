## patch-iron

A processing hack for ironing patches into shape.

It allows you to process each chunk making changes based upon it's local content.

It is useful when a task is more difficult that can be easily solved with grepdiff and a bash script.

### Usage

- Open patch-iron.sln and bend Conversion.cs ProcessChunk () to your bidding.
- Build
- Run mono patch-iron.exe foo.patch > bar.patch
