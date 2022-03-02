## patch-iron

A processing hack for ironing patches into shape.

It allows you to process each chunk making changes based upon it's local content.

It is useful when a task is more difficult that can be easily solved with grepdiff and a bash script.

### Usage

- Open patch-iron.sln and bend Conversion.cs ProcessChunk () to your bidding.
- Build
- Run mono patch-iron.exe foo.patch > bar.patch

### Hints

- Patches contain offset headers:

```
+++ b/src/AVFoundation/AVAssetDownloadTask.cs
@@ -9,16 +9,9 @@
```

that can be complex to replace, so in almost all cases it is better to replace one line with another, or just change a removal/add to keep by flipping the first characters. 
- If you corrupt the headers, you'll get `patch: **** Only garbage was found in the patch input.` or `patch: **** malformed patch at line 14:`.
- Patch's have many PatchPart, which have many PatchChunk, each one a number of lines.
- Range from PatchChunk::CalculateDiffs will spell out the range of each diff in a chunk