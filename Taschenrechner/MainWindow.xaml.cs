using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;


namespace Calculator
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string number1 = "";
        private string number2 = "";
        private string op = "";
        private bool operatorPressed = false;
        private const double SidebarWidth = 220;   // width of sidebar when showed
        private const double ShowAtWidth = 560;   // display from this window width



        // Memory box that stores the "base" value after MS is pressed and the "value" after M+/M-
        private class MemoryItem
        {
            public double Base { get; set; }
            public double Value { get; set; }
            public MemoryItem(double v) { Base = v; Value = v; }
            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        //Memory item gets saved
        private ObservableCollection<MemoryItem>/*this list is only alowed in objects from the type MemoryItem  */ memoryList = new(); //TODO: New Class set
        private double lastResult = 0; // saves last result for MR

        private string _displayText = ""; // backing field fürs Binding
        public event PropertyChangedEventHandler? PropertyChanged; // INotifyPropertyChanged-Event


        //Start the wpf layout
        public MainWindow()
        {
            InitializeComponent();
            MemoryListBox.ItemsSource = memoryList; // Connect with both together
            DataContext = this; // Enable data binding for DisplayText property(current Window)


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
                        MemoryListBox.Items.Refresh(); // <<<< UI-Refresh ergänzen
                        RefreshMemoryUI(idx);
                    }
                }
            }
            else if (content == "M-")
            {
                // if no memory exists, save current value as negative memory
                if (memoryList.Count == 0)
                {
                    double value = GetCurrentValueOrZero();
                    AddToMemory(-value);
                }
                else
                {
                    // get index of selected memory
                    int idx = GetTargetMemoryIndex();
                    if (idx >= 0)
                    {
                        // decrease selected memory value by its base amount
                        memoryList[idx].Value -= memoryList[idx].Base;
                        MemoryListBox.Items.Refresh(); // <<<< UI-Refresh ergänzen
                        // update memory display keeping selection on this entry
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


        //get saved memory value and insert into display
        private void InsertMemoryIntoCurrent(double value)
        {

            string v = value.ToString(CultureInfo.InvariantCulture);//in which number value gets inserted
            if (!operatorPressed)
                number1 = v;
            else
                number2 = v;
            UpdateDisplay();
        }

        // add new memory item to list and update memory display with the new entry

        private void AddToMemory(double value)
        {
            memoryList.Add(new MemoryItem(value));
            RefreshMemoryUI(memoryList.Count - 1);
        }


        // refresh memory list display in the UI
        // optionally keep a specific entry selected (preserveSelectionIndex) 
        // or keep current selection if no index is provided
        private void RefreshMemoryUI(int? preserveSelectionIndex = null)
        {
            int desiredIndex = preserveSelectionIndex ?? MemoryListBox.SelectedIndex;


            if (memoryList.Count > 0)
            {
                MemoryLabel.Content = $"M: {memoryList.Count} Einträge";
                MemoryLabel.Visibility = Visibility.Visible;

                if (desiredIndex >= 0 && desiredIndex < memoryList.Count)
                    MemoryListBox.SelectedIndex = desiredIndex;
            }
            else
            {
                MemoryLabel.Visibility = Visibility.Collapsed;
                MemoryListBox.SelectedIndex = -1;
            }
        }


        // Calculation Method
        private void Calculate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(number1) || string.IsNullOrWhiteSpace(op) || string.IsNullOrWhiteSpace(number2))
                    return;

                double num1 = double.Parse(number1, CultureInfo.InvariantCulture);//from string into double, 
                double num2 = double.Parse(number2, CultureInfo.InvariantCulture);
                double result;

                if (op == "+")
                {
                    result = num1 + num2;
                }
                else if (op == "-")
                {
                    result = num1 - num2;
                }
                else if (op == "*")
                {
                    result = num1 * num2;
                }
                else if (op == "/")
                {
                    if (num2 == 0)
                        throw new DivideByZeroException();
                    else
                        result = num1 / num2;
                }
                else
                {
                    throw new InvalidOperationException("Unknown operator"); // Abort the method and output an error
                }


                lastResult = result;//saving result

                string entry = $"{num1} {op} {num2} = {result}";//Whole calculation in String
                HistoryBox.Text = entry + Environment.NewLine + HistoryBox.Text;//Display in History box

                DisplayText = result.ToString(CultureInfo.InvariantCulture);
                number1 = result.ToString(CultureInfo.InvariantCulture);// result of the last calculation
                number2 = "";//reset
                op = "";//reset
                operatorPressed = false;//reset
            }

            catch (DivideByZeroException)
            {
                DisplayText = "Error";
                MessageBox.Show("Division by 0 is not allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                number2 = "";
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                DisplayText = "Error";
                MessageBox.Show($"Calculation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // TODO: property und field display

        public string DisplayText   //property
        {
            get => _displayText; //access to field
            set//logic of field
            {
                _displayText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
            }
        }







        //display numbers and operators
        private void UpdateDisplay()
        {
            if (!operatorPressed)
                DisplayText = number1;
            else if (string.IsNullOrEmpty(number2))
                DisplayText = number1 + " " + op;
            else
                DisplayText = number1 + " " + op + " " + number2;
        }
        //Main funstion of the of the window, determines visible/ivisible based on window width


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Focus(); // Window gets focus
        }



        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) // SizeChangedEventArgs send information form th e wpf to the c#
        {
            bool show = e.NewSize.Width >= ShowAtWidth; // e.NewSize.Width current width,    ShowAtWidth defined varibale for the size sidebar gets showed, 


            if (show == true)
            {
                MemoryPanel.Visibility = Visibility.Visible; //set visible
                RightPaneColumn.Width = new GridLength(SidebarWidth); //link to the wpf code, set length of the sidebar to the value of the variable sidebarwidth
            }
            else
            {
                MemoryPanel.Visibility = Visibility.Collapsed;//deactivate visibility
                RightPaneColumn.Width = new GridLength(0); //link to the wpf code, set length of the sidebar to the value:
            }


        }







        //special keys 
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
        //e details from the wpf keydown
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                SimulateButtonInput("=");
                e.Handled = true;
            }
        }

        //keyboard click
        private void SimulateButtonInput(string input)
        {
            var fakeButton = new Button { Content = input };
            var e = new RoutedEventArgs();

            if (!string.IsNullOrEmpty(input) && (char.IsDigit(input[0]) /* Check if digit 0-9 */ || input == "."))
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

        // clear history box when "clear history" button is clicked
        private void HistoryClear_Click(object sender, RoutedEventArgs e)
        {
            HistoryBox.Clear(); // remove all text from history
        }


        // delete all memory entries and refresh memory display
        private void MemoryDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            memoryList.Clear(); // clear all stored memory items
            RefreshMemoryUI();  // update UI to show empty memory
        }

        //delete selected
        private void MemoryDelete_Selected(object sender, RoutedEventArgs e)//begins method, informatio gets provided
        {
            if (MemoryListBox.SelectedItem is MemoryItem item)// check if cklicked on item is in memoryitemwehen true createvar item 
            {
                memoryList.Remove(item);  // delete the exact item
                RefreshMemoryUI();        // refresh because list no ui refreshes triggers
            }
        }



        // insert selected memory item into current number
        private void MemoryInsertSelected_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedMemory() is MemoryItem m) // check if a memory item is selected
                InsertMemoryIntoCurrent(m.Value);   // insert its value into current input
        }


        // insert selected memory item into current number on double-click
        private void MemoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GetSelectedMemory() is MemoryItem m) // check if a memory item is selected
                InsertMemoryIntoCurrent(m.Value);   // insert its value into current input
        }

    }
}

//updated with comments ver4
