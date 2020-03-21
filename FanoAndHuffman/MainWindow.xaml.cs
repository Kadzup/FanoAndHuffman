using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FanoAndHuffman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void mainWindow_Initialized(object sender, EventArgs e)
        {
            fanoInputTextBox.Text = "Here is example of Fano";
            huffmanInputTextBox.Text = "Here is example of Huffman";

            fanoEncodeTextBox.Text = "";
            huffmanEncodeTextBox.Text = "";

            fanoDecodeTextBox.Text = "";
            huffmanDecodeTextBox.Text = "";
        }

        private void huffmanExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            huffmanEncodeTextBox.Text = "";
            huffmanDecodeTextBox.Text = "";

            string input = huffmanInputTextBox.Text;

            HuffmanTree huffmanTree = new HuffmanTree();

            // Build the Huffman tree
            huffmanTree.BuildHuffmanTree(input);

            // Encode
            BitArray encoded = huffmanTree.Encode(input);

            foreach (bool bit in encoded)
            {
                huffmanEncodeTextBox.Text += ((bit ? 1 : 0) + "");
            }

            // Decode
            huffmanDecodeTextBox.Text = huffmanTree.Decode(encoded);
        }

        private void fanoExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            fanoEncodeTextBox.Text = "";
            fanoDecodeTextBox.Text = "";

            string input = fanoInputTextBox.Text;

            SFanno fano = new SFanno();

            byte[] originalData = Encoding.Default.GetBytes(input);
            uint originalDataSize = (uint)input.Length;
            byte[] compressedData = new byte[originalDataSize * (101 / 100) + 384];
           
            int compressedDataSize = fano.Compress(originalData, compressedData, originalDataSize);

            byte[] decompressedData = new byte[originalDataSize];

            fano.Decompress(compressedData, decompressedData, (uint)compressedDataSize, originalDataSize);

            fanoEncodeTextBox.Text = String.Join(" ", compressedData);
            
            fanoDecodeTextBox.Text = Convert.ToBase64String(decompressedData);
        }
    }

    public class Node
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public Node Right { get; set; }
        public Node Left { get; set; }

        // Find the encoded code for a symbol from the current node
        public List<bool> Traverse(char symbol, List<bool> data)
        {
            // Leaf
            if (Right == null && Left == null)
            {
                if (symbol.Equals(this.Symbol))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<bool> left = null;
                List<bool> right = null;

                if (Left != null)
                {
                    List<bool> leftPathData = new List<bool>();
                    leftPathData.AddRange(data);
                    leftPathData.Add(false);

                    left = Left.Traverse(symbol, leftPathData);
                }

                if (Right != null)
                {
                    List<bool> rightPathData = new List<bool>();
                    rightPathData.AddRange(data);
                    rightPathData.Add(true);
                    right = Right.Traverse(symbol, rightPathData);
                }

                if (left != null)
                {
                    return left;
                }
                else
                {
                    return right;
                }
            }
        }
    }

    public class HuffmanTree
    {
        private List<Node> nodes = new List<Node>();
        public Node Root { get; set; }
        public Dictionary<char, int> SymbolFrequencies = new Dictionary<char, int>();

        public void BuildHuffmanTree(string source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (!SymbolFrequencies.ContainsKey(source[i]))
                {
                    SymbolFrequencies.Add(source[i], 0);
                }

                SymbolFrequencies[source[i]]++;
            }

            // Step# 1: Create list of nodes with symbol and frequencies
            foreach (KeyValuePair<char, int> symbol in SymbolFrequencies)
            {
                nodes.Add(new Node() {Symbol = symbol.Key, Frequency = symbol.Value});
            }

            // Generate root nodes for the lowest frequencies and add it to the end of ordered nodes till only 1 node is left as main root of the complete huffman tree
            while (nodes.Count >= 2)
            {
                // Step# 2: Sort the list of nodes based on its frequencies in ascending order
                List<Node> orderedNodes = nodes.OrderBy(node => node.Frequency).ToList<Node>();

                if (orderedNodes.Count >= 2)
                {
                    // Take first two items
                    List<Node> taken = orderedNodes.Take(2).ToList<Node>();

                    // Create a parent node by combining the frequencies
                    Node parent = new Node()
                    {
                        Symbol = '*',
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };
                    //Remove left and right nodes and add their parent to the end of nodes list
                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                this.Root = nodes.FirstOrDefault();
            }
        }

        public BitArray Encode(string source)
        {
            List<bool> encodedSource = new List<bool>();

            for (int i = 0; i < source.Length; i++)
            {
                List<bool> encodedSymbol = this.Root.Traverse(source[i], new List<bool>());

                encodedSource.AddRange(encodedSymbol);

                // Print the bit value for each symbol
                /*Console.Write("Symbol: " + source[i] + " Encoded: ");
                foreach (bool bit in new BitArray(encodedSymbol.ToArray()))
                {
                    Console.Write(bit);
                }
                Console.WriteLine();*/
            }

            BitArray bits = new BitArray(encodedSource.ToArray());
            return bits;
        }

        public string Decode(BitArray bits)
        {
            // Start from root of the huffman tree
            Node current = this.Root;
            string decoded = "";

            foreach (bool bit in bits)
            {
                if (bit) // If true then move right
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                    }
                }
                else
                {
                    // If false then move left
                    if (current.Left != null)
                    {
                        current = current.Left;
                    }
                }

                // Every leaf node is a symbol so once you reach there then add it to decoded and then reset the current to the root of huffman tree
                if (IsLeaf(current))
                {
                    decoded += current.Symbol;
                    current = this.Root;
                }
            }

            return decoded;
        }

        public bool IsLeaf(Node node)
        {
            return (node.Left == null && node.Right == null);
        }

    }


    public class SFanno
    {
        public class BitStream
        {
            public byte[] BytePointer;
            public uint BitPosition;
            public uint Index;
        }

        public struct Symbol
        {
            public uint Sym;
            public uint Count;
            public uint Code;
            public uint Bits;
        }

        public class TreeNode
        {
            public TreeNode ChildA;
            public TreeNode ChildB;
            public int Symbol;
        }

        private static void initBitStream(ref BitStream stream, byte[] buffer)
        {
            stream.BytePointer = buffer;
            stream.BitPosition = 0;
        }

        private static void writeBits(ref BitStream stream, uint x, uint bits)
        {
            byte[] buffer = stream.BytePointer;
            uint bit = stream.BitPosition;
            uint mask = (uint)(1 << (int)(bits - 1));

            for (uint count = 0; count < bits; ++count)
            {
                buffer[stream.Index] = (byte)((buffer[stream.Index] & (0xff ^ (1 << (int)(7 - bit)))) + ((Convert.ToBoolean(x & mask) ? 1 : 0) << (int)(7 - bit)));
                x <<= 1;
                bit = (bit + 1) & 7;

                if (!Convert.ToBoolean(bit))
                {
                    ++stream.Index;
                }
            }

            stream.BytePointer = buffer;
            stream.BitPosition = bit;
        }

        private static void histogram(byte[] input, Symbol[] sym, uint size)
        {
            Symbol temp;
            int i, swaps;
            int index = 0;

            for (i = 0; i < 256; ++i)
            {
                sym[i].Sym = (uint)i;
                sym[i].Count = 0;
                sym[i].Code = 0;
                sym[i].Bits = 0;
            }

            for (i = (int)size; Convert.ToBoolean(i); --i, ++index)
            {
                sym[input[index]].Count++;
            }

            do
            {
                swaps = 0;

                for (i = 0; i < 255; ++i)
                {
                    if (sym[i].Count < sym[i + 1].Count)
                    {
                        temp = sym[i];
                        sym[i] = sym[i + 1];
                        sym[i + 1] = temp;
                        swaps = 1;
                    }
                }
            } while (Convert.ToBoolean(swaps));
        }

        private static void makeTree(Symbol[] sym, ref BitStream stream, uint code, uint bits, uint first, uint last)
        {
            uint i, size, sizeA, sizeB, lastA, firstB;

            if (first == last)
            {
                writeBits(ref stream, 1, 1);
                writeBits(ref stream, sym[first].Sym, 8);
                sym[first].Code = code;
                sym[first].Bits = bits;
                return;
            }
            else
            {
                writeBits(ref stream, 0, 1);
            }

            size = 0;

            for (i = first; i <= last; ++i)
            {
                size += sym[i].Count;
            }

            sizeA = 0;

            for (i = first; sizeA < ((size + 1) >> 1) && i < last; ++i)
            {
                sizeA += sym[i].Count;
            }

            if (sizeA > 0)
            {
                writeBits(ref stream, 1, 1);

                lastA = i - 1;

                makeTree(sym, ref stream, (code << 1) + 0, bits + 1, first, lastA);
            }
            else
            {
                writeBits(ref stream, 0, 1);
            }

            sizeB = size - sizeA;

            if (sizeB > 0)
            {
                writeBits(ref stream, 1, 1);

                firstB = i;

                makeTree(sym, ref stream, (code << 1) + 1, bits + 1, firstB, last);
            }
            else
            {
                writeBits(ref stream, 0, 1);
            }
        }

        public int Compress(byte[] input, byte[] output, uint inputSize)
        {
            Symbol[] sym = new Symbol[256];
            Symbol temp;
            BitStream stream = new BitStream();
            uint i, totalBytes, swaps, symbol, lastSymbol;

            if (inputSize < 1)
                return 0;

            initBitStream(ref stream, output);
            histogram(input, sym, inputSize);

            for (lastSymbol = 255; sym[lastSymbol].Count == 0; --lastSymbol) ;

            if (lastSymbol == 0)
                ++lastSymbol;

            makeTree(sym, ref stream, 0, 0, 0, lastSymbol);

            do
            {
                swaps = 0;

                for (i = 0; i < 255; ++i)
                {
                    if (sym[i].Sym > sym[i + 1].Sym)
                    {
                        temp = sym[i];
                        sym[i] = sym[i + 1];
                        sym[i + 1] = temp;
                        swaps = 1;
                    }
                }
            } while (Convert.ToBoolean(swaps));

            for (i = 0; i < inputSize; ++i)
            {
                symbol = input[i];
                writeBits(ref stream, sym[symbol].Code, sym[symbol].Bits);
            }

            totalBytes = stream.Index;

            if (stream.BitPosition > 0)
            {
                ++totalBytes;
            }

            return (int)totalBytes;
        }

        private static uint readBit(ref BitStream stream)
        {
            byte[] buffer = stream.BytePointer;
            uint bit = stream.BitPosition;
            uint x = (uint)(Convert.ToBoolean(buffer[stream.Index] & (1 << (int)(7 - bit))) ? 1 : 0);
            bit = (bit + 1) & 7;

            if (!Convert.ToBoolean(bit))
            {
                ++stream.Index;
            }

            stream.BitPosition = bit;

            return x;
        }

        private static uint read8Bits(ref BitStream stream)
        {
            byte[] buffer = stream.BytePointer;
            uint bit = stream.BitPosition;
            uint x = (uint)((buffer[stream.Index] << (int)bit) | (buffer[stream.Index + 1] >> (int)(8 - bit)));
            ++stream.Index;

            return x;
        }

        private static TreeNode recoverTree(TreeNode[] nodes, ref BitStream stream, ref uint nodeNumber)
        {
            TreeNode thisNode;

            thisNode = nodes[nodeNumber];
            nodeNumber = nodeNumber + 1;

            thisNode.Symbol = -1;
            thisNode.ChildA = null;
            thisNode.ChildB = null;

            if (Convert.ToBoolean(readBit(ref stream)))
            {
                thisNode.Symbol = (int)read8Bits(ref stream);
                return thisNode;
            }

            if (Convert.ToBoolean(readBit(ref stream)))
            {
                thisNode.ChildA = recoverTree(nodes, ref stream, ref nodeNumber);
            }

            if (Convert.ToBoolean(readBit(ref stream)))
            {
                thisNode.ChildB = recoverTree(nodes, ref stream, ref nodeNumber);
            }

            return thisNode;
        }

        public void Decompress(byte[] input, byte[] output, uint inputSize, uint outputSize)
        {
            TreeNode[] nodes = new TreeNode[256];

            for (int counter = 0; counter < nodes.Length; counter++)
            {
                nodes[counter] = new TreeNode();
            }

            TreeNode root, node;
            BitStream stream = new BitStream();
            uint i, nodeCount;
            byte[] buffer;

            if (inputSize < 1) return;

            initBitStream(ref stream, input);

            nodeCount = 0;
            root = recoverTree(nodes, ref stream, ref nodeCount);
            buffer = output;

            for (i = 0; i < outputSize; ++i)
            {
                node = root;

                while (node.Symbol < 0)
                {
                    if (Convert.ToBoolean(readBit(ref stream)))
                        node = node.ChildB;
                    else
                        node = node.ChildA;
                }

                buffer[i] = (byte)node.Symbol;
            }
        }
    }
}
