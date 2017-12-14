using System;
using System.Linq;
using System.IO;
using Mono.Options;
using System.Collections.Generic;

namespace patchiron
{
	class MainClass
	{
		enum Action
		{
			Patch,
			Help,
		}

		public static void Main (string [] args)
		{
			Action action = Action.Patch;
			var os = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => action = Action.Help },
			};

			var bits = os.Parse (args);

			if (bits.Count != 1 || action == Action.Help)
				ShowHelp (os);

			if (!File.Exists (bits[0]))
				Die ($"Unable to find file {bits[0]}");

			var lines = File.ReadAllLines (bits [0]);
			PatchParser parser = new PatchParser (lines);

			Patch patch = parser.Parse ();
			Process (patch);
			patch.Print ();
		}

		static void Process (Patch patch)
		{
			foreach (var part in patch.Parts)
			{
				foreach (var chunk in part.Chunks.ToList ())
				{
					ProcessChunk (chunk, part.FileName);
				}
			}
		}

		static void ProcessChunk (PatchChunk chunk, string fileName)
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
						chunk.Replace (line, bits[0] + "," + bits[1] + "," + fixedLine);
						break;
					}
				}
			}
		}

		static string ProcessLine (string line)
		{
			if (line.Contains ("[Availability (Deprecated = "))
			{
				return line.Replace ("[Availability (Deprecated = ", "[Deprecated (");
			}
			else if (line.Contains ("[Availability (Introduced = "))
			{
				return line.Replace ("[Availability (Introduced = ", "[Introduced (");
			}
			else if (line.Contains ("[Availability (Unavailable = "))
			{
				return line.Replace ("[Availability (Unavailable = ", "[Unavailable (");
			}

			return line;
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("Usage: patch-iron [options] start-pach end-patch");
			Console.WriteLine ("ProcessDiff will need to be modified to do something useful");
			os.WriteOptionDescriptions (Console.Out);
			Die (); 
		}

		public static void Die (string message = null)
		{
			if (message != null)
				Console.Error.WriteLine (message);
			System.Environment.Exit (1);
		}
	}
}
