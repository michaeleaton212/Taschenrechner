using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Taschenrechner
{
    public partial class MainWindow : Window
    {
        private string zahl1 = "";
        private string zahl2 = "";
        private string op = "";
        private bool operatorGedrueckt = false;

        private double memory = 0;
        private double lastResult = 0;



        public MainWindow() //load wpf layout
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) // method that is executed when button is clicked
        {
            var btn = sender as Button;
            var inhalt = btn.Content.ToString();


            //button clicks are checked
            if (double.TryParse(inhalt, out _) || inhalt == ".") 
            {
                if (!operatorGedrueckt)
                    zahl1 += inhalt;
                else
                    zahl2 += inhalt;
            }
            else if (inhalt == "AC")
            {
                zahl1 = "";
                zahl2 = "";
                op = "";
                operatorGedrueckt = false;
                memory = 0;
                lastResult = 0;
                UpdateMemoryLabel();
            }
            else if (inhalt == "+" || inhalt == "-" || inhalt == "*" || inhalt == "/")
            {
                op = inhalt;
                operatorGedrueckt = true;
            }
            else if (inhalt == "=")
            {
                Berechne();
            }
            else if (inhalt == "←")
            {
                if (!operatorGedrueckt && zahl1.Length > 0)
                    zahl1 = zahl1.Substring(0, zahl1.Length - 1);
                else if (operatorGedrueckt && zahl2.Length > 0)
                    zahl2 = zahl2.Substring(0, zahl2.Length - 1);
            }
            else if (inhalt == "M+")
            {
                memory += lastResult;
                UpdateMemoryLabel();
            }
            else if (inhalt == "M-")
            {
                memory -= lastResult;
                UpdateMemoryLabel();
            }
            else if (inhalt == "MS")
            {
                memory = lastResult;
                UpdateMemoryLabel();
            }

            UpdateDisplay(); //updates number displayed on display
        }

        //calculations with the operator identified above
        private void Berechne()
        {
            try
            {
                double num1 = double.Parse(zahl1);
                double num2 = double.Parse(zahl2);
                double result = 0;

                if (op == "+") result = num1 + num2;
                else if (op == "-") result = num1 - num2;
                else if (op == "*") result = num1 * num2;
                else if (op == "/")
                {
                    if (num2 == 0)
                    {
                        MessageBox.Show("Durch 0 ist nicht definiert.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        zahl2 = "";
                        UpdateDisplay();
                        return;
                    }


                    result = num1 / num2;
                }

                lastResult = result;

                string eintrag = $"{num1} {op} {num2} = {result}";
                VerlaufBox.Text = eintrag + Environment.NewLine + VerlaufBox.Text;

                Display.Text = result.ToString();
                zahl1 = result.ToString();
                zahl2 = "";
                op = "";
                operatorGedrueckt = false;
            }
            catch
            {
                Display.Text = "Fehler";
            }
        }


        //display the numbers on the display
        private void UpdateDisplay()
        {
            if (!operatorGedrueckt)
                Display.Text = zahl1;
            else
                Display.Text = zahl2;
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
                SimuliereButtonEingabe("←");
            else if (e.Key == Key.Escape)
                SimuliereButtonEingabe("AC");
            else if (e.Key == Key.Return || e.Key == Key.Enter)
                SimuliereButtonEingabe("=");
        }


        //keyboard input translation to button clicks (normal Keys)

        private void Window_TextInput(object sender, TextCompositionEventArgs e)
        {
            string input = e.Text;

            if ("0123456789.+-*/".Contains(input))
                SimuliereButtonEingabe(input);
        }


        // Simulates a button click using keyboard input

        private void SimuliereButtonEingabe(string eingabe)
        {
            Button_Click(new Button { Content = eingabe }, null);
        }
    }
}
