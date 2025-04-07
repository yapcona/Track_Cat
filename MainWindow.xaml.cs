using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace finance_tracker_track_cat
{
    /// <summary>
    /// interaction logic for mainwindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // currency at the beginning
        private string selectedCurrency = "€";
        private ListBoxItem editingItem;

        public MainWindow()
        {
            InitializeComponent();
            LoadTransactions();
        }

        private void IncomeTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            // ignore. code won't work without it xD if you know why, please let me know
        }

        private void AddIncomeClick(object sender, RoutedEventArgs e)
        {
            // get the description and amount from the textboxes
            string description = DescriptionTextBox.Text;
            string amountText = AmountTextBox.Text;

            // check if description or amount is empty
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(amountText) || description == "Add Description" || amountText == "Enter Amount")
            {
                MessageBox.Show("please enter a valid description and amount.");
                return;
            }

            // determine the emoji based on the amount
            string emoji = "➖"; // default minus
            if (double.TryParse(amountText, out double amount))
            {
                if (amount >= 0)
                {
                    emoji = "➕"; // positive amounts
                }
            }

            // create a new transaction string with the appropriate emoji
            string transaction = $"{emoji} {description}: {amountText} {selectedCurrency}";

            // create a new listboxitem
            ListBoxItem item = new ListBoxItem();
            item.Content = transaction;

            // set the color based on the amount
            if (amount < 0)
            {
                item.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                item.Foreground = new SolidColorBrush(Colors.Green);
            }

            // if editingItem is not null, remove it from the list
            if (editingItem != null)
            {
                AllTransactionsBox.Items.Remove(editingItem);
                editingItem = null;
            }

            // add the transaction to AllTransactionsBox
            AllTransactionsBox.Items.Add(item);

            // clear the textboxes
            DescriptionTextBox.Text = "Add Description";
            AmountTextBox.Text = "Enter Amount";

            // set focus back to DescriptionTextBox
            DescriptionTextBox.Focus();

            // update the totals
            UpdateTotals();

            // save transactions to file
            string filePath = @"C:\Users\HAN\Desktop\transactions.json";
            SaveTransactions(filePath);
        }

        private void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            // get the selected transaction
            var selectedTransaction = AllTransactionsBox.SelectedItem as ListBoxItem;
            if (selectedTransaction != null)
            {
                // remove the selected transaction from the list
                AllTransactionsBox.Items.Remove(selectedTransaction);
                // update totals
                UpdateTotals();
            }
            else
            {
                MessageBox.Show("Please select a transaction to delete.");
            }

            // save transaction to file
            string filePath = @"C:\Users\HAN\Desktop\transactions.json";
            SaveTransactions(filePath);
        }

        private void EditTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTransaction = AllTransactionsBox.SelectedItem as ListBoxItem;
            if (selectedTransaction != null)
            {
                string? content = selectedTransaction.Content.ToString();
                string[] parts = content.Split(':');
                if (parts.Length == 2)
                {
                    DescriptionTextBox.Text = parts[0].Trim().Substring(2); // Remove the emoji and space
                    AmountTextBox.Text = parts[1].Trim().Split(' ')[0];
                    editingItem = selectedTransaction;
                }
            }
            else
            {
                MessageBox.Show("Please select a transaction to edit.");
            }
        }

        private void AddTagClick(object sender, RoutedEventArgs e)
        {
            // get the tag from the textbox
            string tag = TagTextBox.Text;

            // check if tag is empty
            if (string.IsNullOrWhiteSpace(tag) || tag == "Add Tag")
            {
                MessageBox.Show("please enter a valid tag.");
                return;
            }

            // add the tag to the list (you can implement the logic to store tags as needed)
            // for now, just clear the textbox
            TagTextBox.Text = "Add Tag";
        }

        private void TagTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // add tag when enter is pressed in TagTextBox
            if (e.Key == Key.Enter)
            {
                AddTagClick(sender, e);
            }
        }

        private void UpdateTotals()
        {
            double totalIncome = 0;
            double totalExpense = 0;

            // calculate total income and expenses
            foreach (ListBoxItem item in AllTransactionsBox.Items)
            {
                string? content = item.Content.ToString();
                string amountText = content.Split(':')[1].Trim().Split(' ')[0];
                if (double.TryParse(amountText, out double amount))
                {
                    if (amount < 0)
                    {
                        totalExpense += amount;
                    }
                    else
                    {
                        totalIncome += amount;
                    }
                }
            }

            // update total income box
            TotalIncomeBox.Items.Clear();
            ListBoxItem incomeItem = new ListBoxItem();
            incomeItem.Content = $"{totalIncome} {selectedCurrency}";
            incomeItem.Foreground = new SolidColorBrush(Colors.Green);
            TotalIncomeBox.Items.Add(incomeItem);

            // update total expense box
            TotalExpenseBox.Items.Clear();
            ListBoxItem expenseItem = new ListBoxItem();
            expenseItem.Content = $"{totalExpense} {selectedCurrency}";
            expenseItem.Foreground = new SolidColorBrush(Colors.Red);
            TotalExpenseBox.Items.Add(expenseItem);

            // update total total box
            double total = totalIncome + totalExpense; // totalExpense is negative
            TotalTotalBox.Items.Clear();
            ListBoxItem totalItem = new ListBoxItem();
            totalItem.Content = $"{total} {selectedCurrency}";
            totalItem.Foreground = new SolidColorBrush(Colors.Black);
            TotalTotalBox.Items.Add(totalItem);
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            // clear the default text when the textbox gets focus
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                if (textBox.Text == "Add Description" || textBox.Text == "Enter Amount")
                {
                    textBox.Text = string.Empty;
                }
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            // restore the default text if the textbox is empty when it loses focus
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "DescriptionTextBox")
                    {
                        textBox.Text = "Add Description";
                    }
                    else if (textBox.Name == "AmountTextBox")
                    {
                        textBox.Text = "Enter Amount";
                    }
                }
            }
        }

        //////////////////////////// keydown section ////////////////////////////
        private void EditTransaction_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditTransactionButton_Click(sender, e);
        }
        private void DeleteTransaction_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteTransactionButton_Click(sender, e);
            }
        }
        private void DescriptionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // move focus to amounttextbox when enter is pressed
            if (e.Key == Key.Enter)
            {
                AmountTextBox.Focus();
            }
        }

        private void AmountTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // add income when enter is pressed in AmountTextBox
            if (e.Key == Key.Enter)
            {
                AddIncomeClick(sender, e);
            }
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // only allow numbers, minus, and comma in AmountTextBox
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9,-]+"); // only allow numbers, minus, and comma
            return !regex.IsMatch(text);
        }

        private void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // update selected currency when the selection changes
            ComboBox? comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                ComboBoxItem? selectedItem = comboBox.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    selectedCurrency = selectedItem.Content.ToString() == "Euro" ? "€" : "$";
                    UpdateTotals(); // update totals to reflect the new currency
                }
            }
        }

        private void SaveTransactions(string filePath)
        {
            var transactions = new List<string>();
            foreach (ListBoxItem item in AllTransactionsBox.Items)
            {
                transactions.Add(item.Content.ToString());
            }
            File.WriteAllText(filePath, JsonSerializer.Serialize(transactions));
        }

        private void LoadTransactions()
        {
            string filePath = @"C:\Users\HAN\Desktop\transactions.json";
            if (File.Exists(filePath))
            {
                var transactions = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filePath));
                foreach (var transaction in transactions)
                {
                    var item = new ListBoxItem { Content = transaction };
                    string amountText = transaction.Split(':')[1].Trim().Split(' ')[0];
                    if (double.TryParse(amountText, out double amount))
                    {
                        item.Foreground = amount < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    }
                    AllTransactionsBox.Items.Add(item);
                }
            }
        }

        // show information of transaction
        private void AllTransactionsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // nyaaa~ :3
        }
    }
}