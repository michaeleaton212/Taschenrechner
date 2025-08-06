using System;
using System.Windows;
using System.Windows.Controls;

namespace Taschenrechner
{
    public partial class MainWindow : Window
    {
        private string zahl1 = "";
        private string zahl2 = "";
        private string op = "";
        private bool operatorGedrueckt = false;

        public MainWindow()
        {
            InitializeComponent();
            VerbindeButtons();
        }

  private void VerbindeButtons()
{
    if (Content is Grid hauptGrid)
    {
        foreach (var element in hauptGrid.Children)
        {
            if (element is Grid innerGrid)
            {
                foreach (var btn in innerGrid.Children)
                {
                    if (btn is Button b)
                        b.Click += Button_Click;
                }
            }
        }
    }
}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var inhalt = btn.Content.ToString();

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

            UpdateDisplay();

        }

        private void Berechne()
        {
            try
            {
                double num1 = double.Parse(zahl1);
                double num2 = double.Parse(zahl2);
                double result = 0;

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
                    {
                        Display.Text = "Fehler: /0";
                        return;
                    }
                    result = num1 / num2;
                }


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

        private void UpdateDisplay()
        {
            if (!operatorGedrueckt)
                Display.Text = zahl1;
            else
                Display.Text = zahl2;
        }
    }
}
