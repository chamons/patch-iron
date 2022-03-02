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
			List<Range> diffs = chunk.CalculateDiffs ();

			// For each range in diff
				// If one line, and text is "using System.Runtime.Versioning;" then note
				// Else if multiple lines, and the line after it is "partial class" then remove it
				// Else keep
				// If we removed all bits in diff but the using is gone, also remove it
			DiffsRemaining += diffs.Count;
			foreach (var range in diffs)
			{
				bool processed = false;
				if (range.High - range.Low == 1) {
					switch (chunk.Lines[range.Low].Trim ()) {
						case "-using System.Runtime.Versioning;":
							if (UsingLine != null) {
								throw new InvalidOperationException ("More than one using?");
							}
							UsingLine = range.Low;
							ChunkWithUsing = chunk;
							// Flip it now, we'll flip back later if needed
							// processed = true;
							break;
						case "-":
							// Remove whitespace diff while we're here
							DiffsRemaining -= 1;
							processed = true;
							break;
					}
				}
				else {
					// if multiple lines, and the line after it is "partial class" then remove it
					// Else keep
					string lineAfterChunk = chunk.Lines [range.High];
					if (lineAfterChunk.Contains ("partial") && lineAfterChunk.Contains ("class")) {
						DiffsRemaining -= 1;
						processed = true;
					}
				}

				if (!processed) {
					for (int i = range.Low; i < range.High; ++i) {
						chunk.FlipFirstCharacter (i, ' ');
					}
				}
			}
		}

		public void ProcessPart (PatchPart part)
		{
			if (DiffsRemaining == 1 && UsingLine != null) {
				ChunkWithUsing!.FlipFirstCharacter (UsingLine.Value, '-');
				DiffsRemaining -= 1;
			}
		}

		public bool ShouldDrop => DiffsRemaining != 0;
	}
}
