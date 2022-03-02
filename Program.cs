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
			var toRemove = new List<PatchPart>();
			foreach (var part in patch.Parts)
			{
				var conversion = new Conversion();
				foreach (var chunk in part.Chunks.ToList ())
				{
					conversion.ProcessChunk (chunk, part.FileName!);
				}
				conversion.ProcessPart (part);
				if (conversion.ShouldDrop) { 
					toRemove.Add (part);
				}
			}
			toRemove.ForEach(p => patch.Parts.Remove(p));
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("Usage: patch-iron [options] start-patch");
			Console.WriteLine ("ProcessDiff will need to be modified to do something useful");
			os.WriteOptionDescriptions (Console.Out);
			Die (); 
		}

		public static void Die (string? message = null)
		{
			if (message != null)
				Console.Error.WriteLine (message);
			System.Environment.Exit (1);
		}
	}
}
