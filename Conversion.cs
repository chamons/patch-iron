using System;
using System.Linq;
using System.Collections.Generic;

namespace patchiron
{
	enum DeltaType { Addition, AddAfter, Removal };
	struct Delta
	{
		public DeltaType Type { get; private set; }
		public string Data { get; private set; }

		public static Delta CreateAddition (string data)
		{
			return new Delta () {
				Type = DeltaType.Addition,
				Data = data
			};
		}

		public static Delta CreateAddAfter (string data)
		{
			return new Delta ()
			{
				Type = DeltaType.AddAfter,
				Data = data
			};
		}

		public static Delta Removal = new Delta () { Type = DeltaType.Removal };
	}

	
	public static class Conversion
	{
		static string ProcessLine (string line)
		{
			if (line.Contains ("enum"))
				return line;
			return null;
		}

		// Add patching logic here. 
		public static void ProcessChunk (PatchChunk chunk, string fileName)
		{
			//string [] removedLines = chunk.Lines.Where (x => x.StartsWith ("-", StringComparison.Ordinal)).ToArray ();
			//string [] addedLines = chunk.Lines.Where (x => x.StartsWith ("-", StringComparison.Ordinal)).ToArray ();
			Dictionary<int, Delta> deltaList = new Dictionary<int, Delta> ();

			List<Range> diffs = chunk.CalculateDiffs ();

			foreach (var range in diffs)
			{
				for (int i = range.Low; i < range.High; ++i)
				{
					int index = i;// + rangeOffSet;

					string line = chunk.Lines [index];
					if (ProcessLine (line) == null)
					{
						if (line.StartsWith ("-", StringComparison.InvariantCulture))
							deltaList.Add (index, Delta.CreateAddAfter ("+" + line.Substring (1)));
						else
							deltaList.Add (index, Delta.Removal);
					}
				}
			}

			foreach (var kv in deltaList.OrderBy (x => x.Key).Reverse ())
			{
				var action = kv.Value.Type;
				switch (action)
				{
					case DeltaType.Addition:
						chunk.InsertAt (kv.Key, kv.Value.Data);
						break;
					case DeltaType.AddAfter:
						chunk.InsertAt (kv.Key + 1, kv.Value.Data);
						break;
					case DeltaType.Removal:
						chunk.RemoveAt (kv.Key);
						break;
					default:
						throw new NotImplementedException ();
				}
			}

			// If you have unbound add/removals you might have to tweak the chunk header
			//for (int i = range.Low; i >= 0; --i)
			//{
			//	string line = chunk.Lines [i];
			//	if (line.StartsWith ("@@ -", StringComparison.Ordinal))
			//	{
			//		var bits = line.Split (new char [] { ',' }, 3);

			//		var correctPart = bits [1].Substring (0, bits [1].IndexOf (' '));
			//		var fixedLine = correctPart + bits [2].Substring (bits [2].IndexOf (' '));
			//		chunk.Replace (line, bits [0] + "," + bits [1] + "," + fixedLine);
			//		break;
			//	}
			//}
		}

		public static string ReplaceFirst (string text, string search, string replace)
		{
			int pos = text.IndexOf (search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring (0, pos) + replace + text.Substring (pos + search.Length);
		}

		// Doesn't have to be perfect
		//static string ProcessAvailabilityToSpecifc (string line)
		//{
		//	string name = null;
		//	if (line.Contains ("[Introduced (PlatformName.iOS"))
		//		name = "iOS";
		//	else if (line.Contains ("[Availability (Introduced = "))
		//		name = "Introduced";
		//	else if (line.Contains ("[Availability (Unavailable = "))
		//		name = "Unavailable";

		//	if (name != null)
		//	{
		//		foreach (var replacement in FindPlatformBits (line))
		//			line = line.Replace (replacement.Key, replacement.Value);

		//		return line.Replace ($"[Availability ({name} = ", $"[{name} (").Replace ("Message =", "message :");
		//	}

		//	return line;
		//}

		//static IEnumerable<KeyValuePair<string, string>> FindPlatformBits (string line)
		//{
		//	int offset = 0;
		//	while (true)
		//	{
		//		int start = line.IndexOf ("Platform.", offset, StringComparison.Ordinal);
		//		if (start == -1)
		//			yield break;

		//		int end = start + 9 /* Platform. */;
		//		while (char.IsLetterOrDigit (line [end]) || line [end] == '_')
		//			end++;

		//		string token = line.Substring (start, end - start);
		//		string convertedToken = token.Replace ("Platform", "PlatformName").Replace ("_", ", ").Replace ("Mac", "MacOSX").Replace ("Watch", "WatchOS").Replace ("TV", "TvOS");

		//		yield return new KeyValuePair<string, string> (token, convertedToken);
		//		offset = end + 1;
		//	}
		//}
	}
}
