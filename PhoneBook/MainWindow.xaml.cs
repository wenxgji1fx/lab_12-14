using System.Windows;
using PhoneBook.ViewModels;

namespace PhoneBook;

public partial class MainWindow : Window
{
    public MainWindow(ContactsListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
