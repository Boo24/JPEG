using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JPEG.Utilities;

namespace JPEG
{
	class HuffmanNode
	{
		public byte? LeafLabel { get; set; }
		public int Frequency { get; set; }
		public HuffmanNode Left { get; set; }
		public HuffmanNode Right { get; set; }
	}

	public class BitsWithLength
	{
		public int Bits { get; set; }
		public int BitsCount { get; set; }

		public class Comparer : IEqualityComparer<BitsWithLength>
		{
			public bool Equals(BitsWithLength x, BitsWithLength y)
			{
				if(x == y) return true;
				if(x == null || y == null)
					return false;
				return x.BitsCount == y.BitsCount && x.Bits == y.Bits;
			}

			public int GetHashCode(BitsWithLength obj)
			{
				if(obj == null)
					return 0;
				return ((397 *obj.Bits) << 5) ^ (17* obj.BitsCount);
			}
		}
	}

	class BitsBuffer
	{
		private List<byte> buffer = new List<byte>();
		private BitsWithLength unfinishedBits = new BitsWithLength();

		public void Add(BitsWithLength bitsWithLength)
		{
			var bitsCount = bitsWithLength.BitsCount;
			var bits = bitsWithLength.Bits;

			int neededBits = 8 - unfinishedBits.BitsCount;
			while(bitsCount >= neededBits)
			{
				bitsCount -= neededBits;
				buffer.Add((byte) ((unfinishedBits.Bits << neededBits) + (bits >> bitsCount)));

				bits = bits & ((1 << bitsCount) - 1);

				unfinishedBits.Bits = 0;
				unfinishedBits.BitsCount = 0;

				neededBits = 8;
			}
			unfinishedBits.BitsCount +=  bitsCount;
			unfinishedBits.Bits = (unfinishedBits.Bits << bitsCount) + bits;
		}

		public byte[] ToArray(out long bitsCount)
		{
			bitsCount = buffer.Count * 8L + unfinishedBits.BitsCount;
			var result = new byte[bitsCount / 8 + (bitsCount % 8 > 0 ? 1 : 0)];
			buffer.CopyTo(result);
			if(unfinishedBits.BitsCount > 0)
				result[buffer.Count] = (byte) (unfinishedBits.Bits << (8 - unfinishedBits.BitsCount));
			return result;
		}
	}

	class HuffmanCodec
	{
		public static byte[] Encode(IEnumerable<byte> data, out Dictionary<BitsWithLength, byte> decodeTable, out long bitsCount)
		{
            var frequences = CalcFrequences(data, byte.MaxValue+1);
		    
			var root = BuildHuffmanTree(frequences);

			var encodeTable = new BitsWithLength[frequences.Length];
			FillEncodeTable(root, encodeTable);
			var bitsBuffer = new BitsBuffer();
            foreach (var b in data)
		    {
		        bitsBuffer.Add(encodeTable[b]);
		    }

		    decodeTable = CreateDecodeTable(encodeTable);
			return bitsBuffer.ToArray(out bitsCount);
		}

		public static byte[] Decode(byte[] encodedData, Dictionary<BitsWithLength, byte> decodeTable, long bitsCount)
		{
			var result = new List<byte>();
			byte decodedByte;
			var sample = new BitsWithLength { Bits = 0, BitsCount = 0 };
			for(var byteNum = 0; byteNum < encodedData.Length; byteNum++)
			{
				var b = encodedData[byteNum];
				for(var bitNum = 0; bitNum < 8 && byteNum * 8 + bitNum < bitsCount; bitNum++)
				{
					sample.Bits = (sample.Bits << 1) + ((b & (1 << (8 - bitNum - 1))) != 0 ? 1 : 0);
					sample.BitsCount++;

					if(decodeTable.TryGetValue(sample, out decodedByte))
					{
						result.Add(decodedByte);
						sample.BitsCount = 0;
						sample.Bits = 0;
					}
				}
			}
			return result.ToArray();
		}

		private static Dictionary<BitsWithLength, byte> CreateDecodeTable(BitsWithLength[] encodeTable)
		{
			var result = new Dictionary<BitsWithLength, byte>(new BitsWithLength.Comparer());
			for(int b = 0; b < encodeTable.Length; b++)
			{
				var bitsWithLength = encodeTable[b];
				if(bitsWithLength == null)
					continue;

				result[bitsWithLength] = (byte) b;
			}
			return result;
		}

		private static void FillEncodeTable(HuffmanNode node, BitsWithLength[] encodeSubstitutionTable, int bitvector = 0, int depth = 0)
		{
			if(node.LeafLabel != null)
				encodeSubstitutionTable[node.LeafLabel.Value] = new BitsWithLength {Bits = bitvector, BitsCount = depth};
			else
			{
				if(node.Left != null)
				{
					FillEncodeTable(node.Left, encodeSubstitutionTable, (bitvector << 1) + 1, depth + 1);
					FillEncodeTable(node.Right, encodeSubstitutionTable, (bitvector << 1) + 0, depth + 1);
				}
			}
		}

		private static HuffmanNode BuildHuffmanTree(int[] frequences)
		{
			var nodes = GetNodes(frequences);
			
			while(nodes.Count > 1)
			{
				var firstMin = nodes.First();
				nodes.RemoveFirst();
				var secondMin = nodes.First();
				nodes.RemoveFirst();
               // nodes = AddNodeToLL(new HuffmanNode { Frequency = firstMin.Frequency + secondMin.Frequency, Left = secondMin, Right = firstMin }, nodes);
				nodes.AddLast(new HuffmanNode {Frequency = firstMin.Frequency + secondMin.Frequency, Left = secondMin, Right = firstMin });
			    nodes = new LinkedList<HuffmanNode>(nodes.OrderBy(node => node.Frequency));
			}
			return nodes.First();
		}

        private static LinkedList<HuffmanNode> AddNodeToLL(HuffmanNode node, LinkedList<HuffmanNode> list)
        {
            var f = list.First;
            if (f == null || f.Value.Frequency >= node.Frequency)
            {
                list.AddFirst(node);
                return list;
            }
            LinkedListNode<HuffmanNode> before = null;
            for (var n = list.First; n != null; n = n.Next)
            {
                if (node.Frequency <= n.Value.Frequency)
                {
                    before = n;
                    break;
                }
            }
            if (before != null)
                list.AddBefore(before, node);
            else
                list.AddLast(node);
            return list;
        }

        private static LinkedList<HuffmanNode> GetNodes(int[] frequences)
		{
		    return new LinkedList<HuffmanNode>(
		        Enumerable.Range(0, byte.MaxValue+1)
		        .Select(num => new HuffmanNode {Frequency = frequences[num], LeafLabel = (byte) num})
		        .Where(node => node.Frequency > 0)
		        .OrderBy(node => node.Frequency));

		}

		private static int[] CalcFrequences(IEnumerable<byte> data, int length)
		{
			var result = new int[length];
			Parallel.ForEach(data, b => Interlocked.Increment(ref result[b]));
		    return result;
		}
	}
}