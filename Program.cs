using System;
using System.Linq;
using System.IO;
using Mono.Options;


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
			int current = 0;
			while (true)
			{
				Console.WriteLine (lines[current]);
				current++;

				if (current >= lines.Count ())
					break;
			}
		}

		static bool ProcessDiff ()
		{
			return false;
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("Usage: patch-iron [options] start-pach end-patch");
			Console.WriteLine ("ProcessDiff will need to be modified to do something useful");
			os.WriteOptionDescriptions (Console.Out);
			Die (); 
		}

		static void Die (string message = null)
		{
			if (message != null)
				Console.Error.WriteLine (message);
			System.Environment.Exit (1);
		}

	}
}
