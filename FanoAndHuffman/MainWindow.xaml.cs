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

            Console.Write("Encoded: ");
            foreach (bool bit in encoded)
            {
                huffmanEncodeTextBox.Text += ((bit ? 1 : 0) + "");
            }

			// Decode
            huffmanDecodeTextBox.Text = huffmanTree.Decode(encoded);
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
				nodes.Add(new Node() { Symbol = symbol.Key, Frequency = symbol.Value });
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
				if (bit)// If true then move right
				{
					if (current.Right != null)
					{
						current = current.Right;
					}
				}
				else
				{// If false then move left
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
}
