using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Matrix
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            // Инициализация DataGridView для Шеннона-Фано
            dataGridViewFano.Columns.Add("Symbol", "Символ");
            dataGridViewFano.Columns.Add("Probability", "Вероятность");
            dataGridViewFano.Columns.Add("Code", "Код");

            // Инициализация DataGridView для Хаффмана
            dataGridViewHaffmen.Columns.Add("Symbol", "Символ");
            dataGridViewHaffmen.Columns.Add("Probability", "Вероятность");
            dataGridViewHaffmen.Columns.Add("Code", "Код");

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            string input = textBoxInput.Text;

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Введите строку для кодирования.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var frequencies = GetFrequencies(input);

            if (frequencies.Count == 0)
            {
                MessageBox.Show("Входная строка не содержит символов для кодирования.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Вычисление энтропии
            double entropy = CalculateEntropy(frequencies);

            // Генерация кодов
            var shannonFanoCodes = ShannonFano.Encode(frequencies);
            var huffmanCodes = Huffman.Encode(frequencies);

            // Заполнение DataGridView
            FillDataGridView(dataGridViewFano, shannonFanoCodes, frequencies, true);
            FillDataGridView(dataGridViewHaffmen, huffmanCodes, frequencies, true);

            // Кодирование сообщения
            string encodedMessageSF = EncodeMessage(input, shannonFanoCodes);
            if (encodedMessageSF == null)
            {
                return;
            }

            string encodedMessageHF = EncodeMessage(input, huffmanCodes);
            if (encodedMessageHF == null)
            {
                return;
            }

            textBox1.Text = encodedMessageHF; // Хаффман
            textBox2.Text = encodedMessageSF; // Шеннон-Фано

            // Вычисление длины ввода (количество символов без пробелов управления)
            int inputLength = input.Count(c => !char.IsControl(c));

            if (inputLength == 0)
            {
                MessageBox.Show("Входная строка не содержит символов для кодирования.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Средняя длина кода на символ для Шеннона-Фано и Хаффмана
            double averageLengthSF = CalculateAverageLength(shannonFanoCodes, frequencies);
            double averageLengthHF = CalculateAverageLength(huffmanCodes, frequencies);

            // Обновление текстовых полей для средней длины
            txtAverageSymbolLengthSF.Text = averageLengthSF.ToString("F3");
            txtAverageSymbolLengthH.Text = averageLengthHF.ToString("F3");

            // Обновление поля энтропии
            txtEntropy.Text = entropy.ToString("F3");

            // Избыточность теперь рассчитывается по формуле: (L - H) / L
            double redundancySF = (averageLengthSF - entropy) / averageLengthSF;
            double redundancyHF = (averageLengthHF - entropy) / averageLengthHF;

            txtRedundancySF.Text = redundancySF.ToString("F3");
            txtRedundancyH.Text = redundancyHF.ToString("F3");

        }

        private Dictionary<char, double> GetFrequencies(string input)
        {
            // Удаляем символы перевода строки, табуляции и возврата каретки
            input = input.Replace("\r", "")
                         .Replace("\n", "")
                         .Replace("\t", "");

            var freq = new Dictionary<char, double>();
            int total = 0;

            foreach (char c in input)
            {
                char key = char.ToLower(c);
                if (!freq.ContainsKey(key))
                    freq[key] = 0;
                freq[key]++;
                total++;
            }

            if (total == 0)
                return new Dictionary<char, double>();

            // Преобразуем счетчики в вероятности
            var probabilities = freq.ToDictionary(pair => pair.Key, pair => pair.Value / total);
            return probabilities;
        }


        private double CalculateEntropy(Dictionary<char, double> probabilities)
        {
            double entropy = 0;
            foreach (var pair in probabilities)
            {
                double p = pair.Value;
                if (p > 0)
                {
                    entropy -= p * Math.Log(p, 2);
                }
            }
            return entropy;
        }

        private void FillDataGridView(DataGridView dgv, Dictionary<char, string> codes, Dictionary<char, double> frequencies, bool descending = false)
        {
            dgv.Rows.Clear();
            // Сортируем по убыванию частоты (при равной частоте - по символу)
            var sortedCodes = frequencies
                .OrderByDescending(f => f.Value)   // сортировка по убыванию частоты
                .ThenBy(f => f.Key)               // при равенстве частот – по символу
                .ToList();

            foreach (var pair in sortedCodes)
            {
                char symbol = pair.Key;
                string symbolDisplay = symbol == ' ' ? "(пробел)" : symbol.ToString();

                // Если символ есть в словаре кодов, добавляем строку. 
                // Предварительно убедимся, что код для символа существует.
                if (codes.ContainsKey(symbol))
                {
                    dgv.Rows.Add(symbolDisplay, pair.Value.ToString("F3"), codes[symbol]);
                }
                else
                {
                    // Если вдруг кода нет (не должно происходить), можно вывести предупреждение или пропустить символ.
                    // Но для корректно работающего алгоритма такого не будет.
                }
            }
        }

        private string EncodeMessage(string input, Dictionary<char, string> codes)
        {
            var encodedMessage = new StringBuilder();
            foreach (char c in input)
            {
                char key = char.ToLower(c);
                if (codes.ContainsKey(key))
                {
                    encodedMessage.Append(codes[key]);
                }
                else
                {
                    // Если кода для данного символа нет (не должно случиться, но на всякий случай)
                    // Можно игнорировать или делать что-то еще
                }
            }

            return encodedMessage.ToString();
        }

        private double CalculateAverageLength(Dictionary<char, string> codes, Dictionary<char, double> frequencies)
        {
            double averageLength = 0;
            foreach (var pair in codes)
            {
                if (frequencies.ContainsKey(pair.Key))
                    averageLength += frequencies[pair.Key] * pair.Value.Length;
            }
            return averageLength;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Кнопка для загрузки текста из файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Multiselect = true,
                Title = "Выберите файл(ы) для загрузки"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();

                foreach (string fileName in openFileDialog.FileNames)
                {
                    try
                    {
                        string fileContent = File.ReadAllText(fileName);
                        sb.AppendLine(fileContent);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при чтении файла {fileName}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // Добавление текста в textBoxInput с новой строки
                textBoxInput.AppendText(sb.ToString());
            }
        }

        private void txtAverageSymbolLengthH_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }

    public static class ShannonFano
    {
        public static Dictionary<char, string> Encode(Dictionary<char, double> frequencies)
        {
            // Сортируем по убыванию частот. При равной частоте – по символу.
            var sorted = frequencies
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key)
                .ToList();

            var result = new Dictionary<char, string>();
            ShannonFanoRecursive(sorted, result, "");
            return result;
        }

        private static void ShannonFanoRecursive(List<KeyValuePair<char, double>> freq, Dictionary<char, string> result, string prefix)
        {
            if (freq.Count == 0)
                return;

            if (freq.Count == 1)
            {
                result[freq[0].Key] = prefix.Length > 0 ? prefix : "0";
                return;
            }

            double total = freq.Sum(pair => pair.Value);
            double half = total / 2;
            double runningSum = 0;
            int splitIndex = 0;
            double minDifference = double.MaxValue;

            for (int i = 0; i < freq.Count; i++)
            {
                runningSum += freq[i].Value;
                double difference = Math.Abs(runningSum - half);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    splitIndex = i;
                }
                else
                {
                    break;
                }
            }

            var left = freq.Take(splitIndex + 1).ToList();
            var right = freq.Skip(splitIndex + 1).ToList();

            ShannonFanoRecursive(left, result, prefix + "0");
            ShannonFanoRecursive(right, result, prefix + "1");
        }
    }


    public static class Huffman
    {
        public class Node
        {
            public char? Symbol { get; set; }
            public double Frequency { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
        }

        public static Dictionary<char, string> Encode(Dictionary<char, double> frequencies)
        {
            // Сортируем исходный список, чтобы при одинаковой частоте порядок символов был стабильным
            var nodes = frequencies
                .OrderBy(f => f.Value)
                .ThenBy(f => f.Key)
                .Select(pair => new Node { Symbol = pair.Key, Frequency = pair.Value }).ToList();

            // Построение дерева Хаффмана
            while (nodes.Count > 1)
            {
                // Заново сортируем, учитывая, что при равных частотах узлы будут упорядочены по символу
                nodes = nodes.OrderBy(n => n.Frequency)
                             .ThenBy(n => n.Symbol.HasValue ? n.Symbol.Value : char.MaxValue)
                             .ToList();

                var left = nodes[0];
                var right = nodes[1];

                var parent = new Node
                {
                    Symbol = null,
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };

                nodes.Remove(left);
                nodes.Remove(right);
                nodes.Add(parent);
            }

            if (nodes.Count == 0)
                return new Dictionary<char, string>();

            var root = nodes[0];
            var result = new Dictionary<char, string>();
            BuildCode(root, "", result);

            if (result.Count == 1)
            {
                var key = result.Keys.First();
                result[key] = "0";
            }

            return result;
        }

        private static void BuildCode(Node node, string prefix, Dictionary<char, string> result)
        {
            if (node == null)
                return;

            if (node.Symbol.HasValue)
            {
                result[node.Symbol.Value] = prefix.Length > 0 ? prefix : "0";
            }

            BuildCode(node.Left, prefix + "0", result);
            BuildCode(node.Right, prefix + "1", result);
        }
    }

}
