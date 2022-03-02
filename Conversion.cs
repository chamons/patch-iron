using System;
using System.Linq;
using System.Collections.Generic;

namespace patchiron
{	
	public class Conversion
	{
		PatchChunk? ChunkWithUsing = null;
		int? UsingLine = null;
		int DiffsRemaining = 0;

		// Add patching logic here. 
		public void ProcessChunk (PatchChunk chunk, string fileName)
		{
			//string [] removedLines = chunk.Lines.Where (x => x.StartsWith ("-", StringComparison.Ordinal)).ToArray ();
			//string [] addedLines = chunk.Lines.Where (x => x.StartsWith ("-", StringComparison.Ordinal)).ToArray ();
			Dictionary<int, Delta> deltaList = new Dictionary<int, Delta> ();

			List<Range> diffs = chunk.CalculateDiffs ();

			// For each range in diff
				// If one line, and text is "using System.Runtime.Versioning;" then note
				// Else if multiple lines, and the line after it is "partial class" then remove it
				// Else keep
				// If we removed all bits in diff but the using is gone, also remove it
			DiffsRemaining += diffs.Count;
			foreach (var range in diffs)
			{
				// Find using System.Runtime.Versioning;
				if (range.High - range.Low == 1) {
					switch (chunk.Lines[range.Low].Trim ()) {
						case "-using System.Runtime.Versioning;":
							if (UsingLine != null) {
								throw new InvalidOperationException ("More than one using?");
							}
							UsingLine = range.Low;
							ChunkWithUsing = chunk;
							break;
						case "-":
							// Remove whitespace diff while we're here
							DiffsRemaining -= 1;
							deltaList.Add (range.Low, Delta.Removal);
							break;
						default:
							break;
					}
				}
				else {
					// if multiple lines, and the line after it is "partial class" then remove it
					// Else keep
					string lineAfterChunk = chunk.Lines [range.High];
					if (lineAfterChunk.Contains ("partial") && lineAfterChunk.Contains ("class")) {
						DiffsRemaining -= 1;
						for (int i = range.Low; i < range.High; ++i) {
							deltaList.Add (i, Delta.Removal);
						}
					}
				}
			}

			Delta.ApplyListToChunk (chunk, deltaList);
		}

		public void ProcessPart (PatchPart part)
		{
			if (DiffsRemaining == 1 && UsingLine != null) {
				Dictionary<int, Delta> deltaList = new Dictionary<int, Delta> ();
				deltaList.Add (UsingLine.Value, Delta.Removal);
				Delta.ApplyListToChunk (ChunkWithUsing!, deltaList);
			}
		}
	}
}
