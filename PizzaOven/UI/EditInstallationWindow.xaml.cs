using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace PizzaOven.UI
{
    /// <summary>
    /// Interaction logic for EditInstallationWindow.xaml
    /// </summary>
    public partial class EditInstallationWindow : Window
    {
        public string newName = null;
        public EditInstallationWindow(string name)
        {
            InitializeComponent();
            if (!String.IsNullOrEmpty(name))
            {
                NameBox.Text = name;
                Title = $"Rename {name}";
            }
            else
                Title = "Add Installation";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Confirm();

        }
        private void Confirm()
        {
            newName = NameBox.Text;
            Close();
        }

        private void NameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Confirm();
            }
        }
    }
}
