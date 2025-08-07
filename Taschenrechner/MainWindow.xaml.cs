using System;
using System.Linq.Expressions;
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

        private double memory = 0;
        private double lastResult = 0;



        public MainWindow() //load wpf layout
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) // method that is executed when button is clicked
        {
            var btn = sender as Button;
            var content = btn.Content.ToString();


            //button clicks are checked
            if (double.TryParse(content, out _) || content == ".")
            {
                if (!operatorPressed)
                    number1 += content;
                else
                    number2 += content;
            }
            else if (content == "AC")
            {
                number1 = "";
                number2 = "";
                op = "";
                operatorPressed = false;
                memory = 0;
                lastResult = 0;
                UpdateMemoryLabel();
            }
            else if (content == "+" || content == "-" || content == "*" || content == "/")
            {
                op = content;
                operatorPressed = true;
            }
            else if (content == "=")
            {
                Calculate();
            }
            else if (content == "←")
            {
                if (!operatorPressed && number1.Length > 0) // if operator is not activ take 1 from number2
                {
                    number1 = number1.Substring(0, number1.Length - 1);
                }
                else if (operatorPressed && number2.Length > 0)// if operator gedrükt ist take 1 from number1
                {
                    number2 = number2.Substring(0, number2.Length - 1);
                }
                else if (operatorPressed && number2.Length == 0 && op.Length > 0) //if the 3 cases are true op gets reset
                {
                    op = "";
                    operatorPressed = false;
                }
            }
            else if (content == "M+")
            {
                if (number1 != "" && !operatorPressed)
                    memory += double.Parse(number1);
                else
                    memory += lastResult;
                UpdateMemoryLabel();
            }
            else if (content == "M-")
            {
                if (number1 != "" && !operatorPressed)
                    memory -= double.Parse(number1);
                else
                    memory -= lastResult;
                UpdateMemoryLabel();
            }
            else if (content == "MS")
            {
                if (number1 != "" && !operatorPressed)
                    memory = double.Parse(number1);
                else
                    memory = lastResult;
                UpdateMemoryLabel();
            }

            UpdateDisplay(); //updates number displayed on display
        }

        //calculations with the operator identified above
        private void Calculate()
        {
            try
            {
                double num1 = double.Parse(number1);
                double num2 = double.Parse(number2);
                double result = 0;

                if (op == "+") result = num1 + num2;
                else if (op == "-") result = num1 - num2;
                else if (op == "*") result = num1 * num2;
                else if (op == "/")
                {
                    if (num2 == 0)
                    {
                        MessageBox.Show("Division by 0 is not defined.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        number2 = "";
                        UpdateDisplay();
                        return;
                    }


                    result = num1 / num2;
                }

                lastResult = result;


                //Histroy
                string entry = $"{num1} {op} {num2} = {result}";
                HistoryBox.Text = entry + Environment.NewLine + HistoryBox.Text; //the one before goes one line down




                Display.Text = result.ToString();
                number1 = result.ToString();
                number2 = "";
                op = "";
                operatorPressed = false;
            }
            //error message
            catch (Exception ex)
            {
                Display.Text = "Error";
                MessageBox.Show($"Calculation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); //display system error text in message box
            }
        }


        //display the numbers on the display
        private void UpdateDisplay()
        {
            if (!operatorPressed)
                Display.Text = number1;
            else if (number2 == "")
                Display.Text = number1 + " " + op;
            else
                Display.Text = number1 + " " + op + " " + number2; 
        }

        //memory label display
        private void UpdateMemoryLabel()
        {
            if (memory != 0)
            {
                MemoryLabel.Content = $"M: {memory}";
                MemoryLabel.Visibility = Visibility.Visible;
            }
            else
            {
                MemoryLabel.Visibility = Visibility.Collapsed;
            }
        }

        // set focus to window after loading
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }


        //keyboard input translation to button clicks (Special Keys)
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
                SimulateButtonInput("←");
            else if (e.Key == Key.Escape)
                SimulateButtonInput("AC");
            else if (e.Key == Key.Return || e.Key == Key.Enter)
                SimulateButtonInput("=");
        }


        //keyboard input translation to button clicks (normal Keys)

        private void Window_TextInput(object sender, TextCompositionEventArgs e)
        {
            string input = e.Text;

            if ("0123456789.+-*/".Contains(input))
                SimulateButtonInput(input);
        }


        // Simulates a button click using keyboard input

        private void SimulateButtonInput(string input)
        {
            Button_Click(new Button { Content = input }, null);
        }
    }
}

