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

		public static void ApplyListToChunk (PatchChunk chunk, Dictionary<int, Delta> deltaList)
		{
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
		}
	}
}