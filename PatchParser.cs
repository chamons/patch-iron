using System;
using System.Collections.Generic;
using System.Linq;

namespace patchiron
{
	public class PatchChunk
	{
		public List<string> Lines { get; } = new List<string> ();

		public PatchChunk (IEnumerable<string> lines)
		{
			Lines = lines.ToList ();
		}

		public void Replace (string oldLine, string newLine)
		{
			Lines [Lines.IndexOf (oldLine)] = newLine;
		}

		public void Print ()
		{
			foreach (string line in Lines)
				Console.WriteLine (line);
		}
	}

	public class PatchPart
	{
		public List<string> Header { get; } = new List<string> ();
		public List<PatchChunk> Chunks { get; } = new List<PatchChunk> ();
		public string FileName { get; set; }

		public PatchPart (IEnumerable<string> header)
		{
			Header = header.ToList ();
		}

		public void Add (PatchChunk chunk) => Chunks.Add (chunk);

		public override string ToString () => $"[PatchInstance: {FileName} ({Chunks.Count ()})";

		public void Print ()
		{
			foreach (string line in Header)
				Console.WriteLine (line);
			foreach (var chunk in Chunks)
				chunk.Print ();
		}
	}

	public class Patch
	{
		public List<PatchPart> Parts { get; } = new List<PatchPart> ();
		public void Add (PatchPart parth) => Parts.Add (parth);

		public override string ToString () => $"[Patch ({Parts.Count ()})";

		public void Print ()
		{
			foreach (var part in Parts)
				part.Print ();
		}
	}

	public class PatchParser
	{
		int Index;
		string [] Lines;

		public PatchParser (string [] lines)
		{
			Index = 0;
			Lines = lines;
		}

		public Patch Parse ()
		{
			Patch patch = new Patch ();

			while (!EndOfFile)
				patch.Add (ParsePart ());

			return patch;
		}

		bool EndOfFile => Index >= Lines.Length;
		string Current => Lines [Index];

		PatchPart ParsePart ()
		{
			PatchPart part = new PatchPart (Lines.Skip (Index).Take (4).ToList ());

			Expect ("diff --git");
			Advance (3);
			Expect ("+++");
			part.FileName = Current.Split (new char [] { '/' }, 2) [1];
			Advance ();

			foreach (var chunk in ParsePatch ())
				part.Add (chunk);

			return part;
		}

		List <PatchChunk> ParsePatch ()
		{
			List<PatchChunk> chunks = new List<PatchChunk> ();
			do
			{
				List<string> lines = new List<string> ();
				lines.Add (Current);
				Expect ("@@");
				Advance ();

				lines.AddRange (AdvanceUntilEither ("@@", "diff --git"));
				chunks.Add (new PatchChunk (lines));
			}
			while (!EndOfFile && !CurrentHasPrefix ("diff --git"));

			return chunks;
		}

		List<string> AdvanceUntil (string prefix)
		{
			List<string> lines = new List<string> ();
			do
			{
				lines.Add (Current); ;
				Advance ();
			}
			while (!EndOfFile && !CurrentHasPrefix (prefix));
			return lines;
		}

		List<string> AdvanceUntilEither (string firstPrefix, string secondPrefix)
		{
			List<string> lines = new List<string> ();
			do
			{
				lines.Add (Current);
				Advance ();
			}
			while (!EndOfFile && !CurrentHasPrefix (firstPrefix) && !CurrentHasPrefix (secondPrefix));
			return lines;
		}

		void Advance (int amount = 1) => Index += amount;
		bool CurrentHasPrefix (string prefix) => Current.StartsWith (prefix, StringComparison.Ordinal);

		void Expect (string prefix)
		{
			if (!CurrentHasPrefix (prefix))
				Die ($"Expected {prefix} on line ({Index}) {Current}");
		}

		static void Die (string message = null) => MainClass.Die (message);
	}
}
