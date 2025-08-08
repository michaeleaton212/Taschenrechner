using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        private string number1 = "";
        private string number2 = "";
        private string op = "";
        private bool operatorPressed = false;


        // Memory box that stores the "base" value after MS is pressed and the "value" after M+/M-
        private class MemoryItem
        {
            public double Base { get; set; }
            public double Value { get; set; }
            public MemoryItem(double v) { Base = v; Value = v; }
            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        //Memory item gets saved
        private readonly List<MemoryItem> memoryList = new();
        private double lastResult = 0; // saves last result for MR


        //Start the wpf layout
        public MainWindow()
        {
            InitializeComponent();
        }

        //Methode of the numbers
        private void ButtonClickNumber(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var content = btn.Content.ToString();
            // Decide which variable the digit will be added to
            if (!operatorPressed)
                number1 += content;
            else
                number2 += content;

            UpdateDisplay();
        }

        //Methode of the Operators
        private void ButtonClickOperator(object sender, RoutedEventArgs e)
        {
            //read button, convert into string
            var btn = (Button)sender;
            var content = btn.Content.ToString();

            op = content;
            operatorPressed = true;
            UpdateDisplay();
        }

        // =
        private void ButtonClickEquals(object sender, RoutedEventArgs e) => Calculate();

        // AC
        private void ButtonClickAllClear(object sender, RoutedEventArgs e)
        {
            number1 = "";
            number2 = "";
            op = "";
            operatorPressed = false;
            lastResult = 0;
            UpdateDisplay();
        }

        // ←
        private void ButtonClickBackspace(object sender, RoutedEventArgs e)
        {
            //reduce length depending on condition
            if (!operatorPressed && number1.Length > 0)
                number1 = number1[..^1];
            else if (operatorPressed && number2.Length > 0)
                number2 = number2[..^1];
            else if (operatorPressed && number2.Length == 0 && op.Length > 0)
            {
                op = "";
                operatorPressed = false;
            }
            UpdateDisplay();
        }

        // Memory: MC, MR, MS, M+, M- 
        private void ButtonClickMemory(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var content = btn.Content.ToString();

            if (content == "MS")
            {
                double value = GetCurrentValueOrZero();//getnumber form display
                //saving
                AddToMemory(value);
            }
            else if (content == "M+")
            {
                if (memoryList.Count == 0)//if there is am memory
                {
                    double value = GetCurrentValueOrZero();
                    AddToMemory(value);
                }
                else //add base
                {
                    int idx = GetTargetMemoryIndex();//position of memoryentry
                    if (idx >= 0)
                    {
                        memoryList[idx].Value += memoryList[idx].Base;
                        RefreshMemoryUI(idx);
                    }
                }
            }
            else if (content == "M-")
            {
                if (memoryList.Count == 0)
                {
                    double value = GetCurrentValueOrZero();
                    AddToMemory(-value);
                }
                else
                {
                    int idx = GetTargetMemoryIndex();
                    if (idx >= 0)
                    {
                        memoryList[idx].Value -= memoryList[idx].Base;
                        RefreshMemoryUI(idx);
                    }
                }
            }
            else if (content == "MR")
            {
                if (GetSelectedMemory() is MemoryItem m)//gets current memory and inserts it into current number
                    InsertMemoryIntoCurrent(m.Value);
                else if (memoryList.Count > 0)//if list is empty nothing happens
                    InsertMemoryIntoCurrent(memoryList[^1].Value);
            }
            //delets elemts ouut of list (clear built in C# list method)
            else if (content == "MC")
            {
                memoryList.Clear();
                RefreshMemoryUI();
            }
        }

        // Get target for M+/M-
        private int GetTargetMemoryIndex()
        {
            if (memoryList.Count == 0) return -1;//check if there is even one
            int sel = MemoryListBox.SelectedIndex;
            return (sel >= 0 && sel < memoryList.Count) ? sel : memoryList.Count - 1;
        }

        private MemoryItem? GetSelectedMemory()
        {
            int i = MemoryListBox.SelectedIndex;
            if (i >= 0 && i < memoryList.Count) return memoryList[i];
            return null;
        }

        //gets current number value
        private double GetCurrentValueOrZero()
        {
            if (!operatorPressed && !string.IsNullOrWhiteSpace(number1))//if no operator active take number1
                return double.Parse(number1, CultureInfo.InvariantCulture);//if operator active take number2
            if (operatorPressed && !string.IsNullOrWhiteSpace(number2))
                return double.Parse(number2, CultureInfo.InvariantCulture);
            return lastResult;
        }

        private void InsertMemoryIntoCurrent(double value)
        {
            string v = value.ToString(CultureInfo.InvariantCulture);
            if (!operatorPressed)
                number1 = v;
            else
                number2 = v;
            UpdateDisplay();
        }

        private void AddToMemory(double value)
        {
            memoryList.Add(new MemoryItem(value));
            RefreshMemoryUI(memoryList.Count - 1);
        }

        private void RefreshMemoryUI(int? preserveSelectionIndex = null)
        {
            int desiredIndex = preserveSelectionIndex ?? MemoryListBox.SelectedIndex;

            MemoryListBox.Items.Clear();
            for (int i = 0; i < memoryList.Count; i++)
                MemoryListBox.Items.Add(memoryList[i]);

            if (memoryList.Count > 0)
            {
                MemoryLabel.Content = $"M: {memoryList.Count} Einträge";
                MemoryLabel.Visibility = Visibility.Visible;

                if (desiredIndex >= 0 && desiredIndex < MemoryListBox.Items.Count)
                    MemoryListBox.SelectedIndex = desiredIndex;
            }
            else
            {
                MemoryLabel.Visibility = Visibility.Collapsed;
                MemoryListBox.SelectedIndex = -1;
            }
        }

        // Calculation
        private void Calculate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(number1) || string.IsNullOrWhiteSpace(op) || string.IsNullOrWhiteSpace(number2))
                    return;

                double num1 = double.Parse(number1, CultureInfo.InvariantCulture);
                double num2 = double.Parse(number2, CultureInfo.InvariantCulture);
                double result = op switch
                {
                    "+" => num1 + num2,
                    "-" => num1 - num2,
                    "*" => num1 * num2,
                    "/" => num2 == 0 ? throw new DivideByZeroException() : num1 / num2,
                    _ => throw new InvalidOperationException("Unknown operator")
                };

                lastResult = result;

                string entry = $"{num1} {op} {num2} = {result}";
                HistoryBox.Text = entry + Environment.NewLine + HistoryBox.Text;

                Display.Text = result.ToString(CultureInfo.InvariantCulture);
                number1 = result.ToString(CultureInfo.InvariantCulture);
                number2 = "";
                op = "";
                operatorPressed = false;
            }
            catch (DivideByZeroException)
            {
                Display.Text = "Error";
                MessageBox.Show("Division by 0 is not allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                number2 = "";
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Display.Text = "Error";
                MessageBox.Show($"Calculation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDisplay()
        {
            if (!operatorPressed)
                Display.Text = number1;
            else if (string.IsNullOrEmpty(number2))
                Display.Text = number1 + " " + op;
            else
                Display.Text = number1 + " " + op + " " + number2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => Focus();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
                SimulateButtonInput("←");
            else if (e.Key == Key.Escape)
                SimulateButtonInput("AC");
            else if (e.Key == Key.Return || e.Key == Key.Enter)
                SimulateButtonInput("=");
        }

        private void Window_TextInput(object sender, TextCompositionEventArgs e)
        {
            string input = e.Text;
            if ("0123456789.+-*/".Contains(input))
                SimulateButtonInput(input);
        }

        // Ensure Enter/Return always triggers "=" even if a child control handles the key
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                SimulateButtonInput("=");
                e.Handled = true;
            }
        }

        private void SimulateButtonInput(string input)
        {
            var fakeButton = new Button { Content = input };
            var e = new RoutedEventArgs();

            if (!string.IsNullOrEmpty(input) && (char.IsDigit(input[0]) || input == "."))
                ButtonClickNumber(fakeButton, e);
            else if (input == "+" || input == "-" || input == "*" || input == "/")
                ButtonClickOperator(fakeButton, e);
            else if (input == "=" || input.Equals("ENTER", StringComparison.OrdinalIgnoreCase))
                ButtonClickEquals(fakeButton, e);
            else if (input == "←" || input == "⌫" || input == "\b" || input.Equals("BACK", StringComparison.OrdinalIgnoreCase))
                ButtonClickBackspace(fakeButton, e);
            else if (input == "AC")
                ButtonClickAllClear(fakeButton, e);
        }

        private void HistoryClear_Click(object sender, RoutedEventArgs e)
        {
            HistoryBox.Clear();
        }

        // Löscht ALLE Memory-Einträge (für Button und Context-Menu)
        private void MemoryDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            memoryList.Clear();
            RefreshMemoryUI();
        }

        private void MemoryInsertSelected_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedMemory() is MemoryItem m)
                InsertMemoryIntoCurrent(m.Value);
        }

        private void MemoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GetSelectedMemory() is MemoryItem m)
                InsertMemoryIntoCurrent(m.Value);
        }
    }
}

//updated