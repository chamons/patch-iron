using System;
using System.Linq;
using System.Collections.Generic;

namespace patchiron
{
	public static class Conversion
	{
		// Add patching logic here. 
		public static void ProcessChunk (PatchChunk chunk, string fileName)
		{
			string [] removedLines = chunk.Lines.Where (x => x.StartsWith ("-", StringComparison.Ordinal)).ToArray ();
			string [] addedLines = chunk.Lines.Where (x => x.StartsWith ("-", StringComparison.Ordinal)).ToArray ();
			List<Range> diffs = chunk.CalculateDiffs ();

			int rangeOffSet = 0;

			foreach (var range in diffs)
			{
				List<int> removeLines = new List<int> ();
				int lastRemovalIndex = -1;
				List<string> linesToAdd = new List<string> ();

				for (int i = range.Low; i < range.High; ++i)
				{
					int index = i + rangeOffSet;

					string line = chunk.Lines [index];
					if (line.StartsWith ("-", StringComparison.InvariantCulture))
					{
						lastRemovalIndex = index;
						linesToAdd.Add ("+" + ProcessLine (line.Substring (1)));
					}
					else if (line.StartsWith ("+", StringComparison.InvariantCulture))
					{
						removeLines.Add (index);
					}
					else
					{
						throw new NotImplementedException ();
					}
				}

				foreach (int index in removeLines.Reverse<int> ())
				{
					chunk.RemoveAt (index);
					rangeOffSet -= 1;
				}

				foreach (string lineToAdd in linesToAdd.Reverse<string> ())
				{
					chunk.InsertAt (lastRemovalIndex + 1, lineToAdd);
					rangeOffSet += 1;
				}

				// We have to fix up the chunk header
				for (int i = range.Low; i >= 0; --i)
				{
					string line = chunk.Lines [i];
					if (line.StartsWith ("@@ -", StringComparison.Ordinal))
					{
						var bits = line.Split (new char [] { ',' }, 3);

						var correctPart = bits [1].Substring (0, bits [1].IndexOf (' '));
						var fixedLine = correctPart + bits [2].Substring (bits [2].IndexOf (' '));
						chunk.Replace (line, bits [0] + "," + bits [1] + "," + fixedLine);
						break;
					}
				}
			}
		}

		// TODO - Process Platform.iOS_10_0 => PlatformName, 10, 0 and the like
		// Doesn't have to be perfect
		static string ProcessLine (string line)
		{
			string name = null;
			if (line.Contains ("[Availability (Deprecated = "))
				name = "Deprecated";
			else if (line.Contains ("[Availability (Introduced = "))
				name = "Introduced";
			else if (line.Contains ("[Availability (Unavailable = "))
				name = "Unavailable";

			if (name != null)
			{
				foreach (var replacement in FindPlatformBits (line))
					line = line.Replace (replacement.Key, replacement.Value);

				return line.Replace ($"[Availability ({name} = ", $"[{name} (").Replace ("Message =", "message :");
			}

			return line;
		}

		static IEnumerable<KeyValuePair<string, string>> FindPlatformBits (string line)
		{
			int offset = 0;
			while (true)
			{
				int start = line.IndexOf ("Platform.", offset, StringComparison.Ordinal);
				if (start == -1)
					yield break;

				int end = start + 9 /* Platform. */;
				while (char.IsLetterOrDigit (line [end]) || line [end] == '_')
					end++;

				string token = line.Substring (start, end - start);
				string convertedToken = token.Replace ("Platform", "PlatformName").Replace ("_", ", ").Replace ("Mac", "MacOSX").Replace ("Watch", "WatchOS").Replace ("TV", "TvOS");

				yield return new KeyValuePair<string, string> (token, convertedToken);
				offset = end + 1;
			}
		}
	}
}
