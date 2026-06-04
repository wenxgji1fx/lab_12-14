using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PhoneBook.Commands;
using PhoneBook.Data;
using PhoneBook.Models;

namespace PhoneBook.ViewModels;

public class ContactsListViewModel : ViewModelBase
{
    private readonly IDbContextFactory<PhoneBookDbContext> _contextFactory;
    private Contact? _selectedContact;
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;

    public ContactsListViewModel(IDbContextFactory<PhoneBookDbContext> contextFactory, ContactEditViewModel editor)
    {
        _contextFactory = contextFactory;
        Editor = editor;

        NewContactCommand = new RelayCommand(_ => StartNewContact());
        DeleteContactCommand = new RelayCommand(_ => DeleteSelectedContact(), _ => SelectedContact is not null);
        RefreshCommand = new RelayCommand(_ => LoadContacts(SelectedContact?.Id));

        Editor.ContactSaved += (_, args) =>
        {
            LoadContacts(args.ContactId);
            StatusMessage = "Данные сохранены в базе данных.";
        };
        LoadContacts();
    }

    public ObservableCollection<Contact> Contacts { get; } = new();

    public ContactEditViewModel Editor { get; }

    public ICommand NewContactCommand { get; }

    public ICommand DeleteContactCommand { get; }

    public ICommand RefreshCommand { get; }

    public Contact? SelectedContact
    {
        get => _selectedContact;
        set
        {
            if (SetProperty(ref _selectedContact, value))
            {
                Editor.LoadContact(value);
                ((RelayCommand)DeleteContactCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                LoadContacts(SelectedContact?.Id);
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private void LoadContacts(int? selectedContactId = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var contacts = context.Contacts
                .AsNoTracking()
                .OrderBy(contact => contact.Name)
                .ToList();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim();
                contacts = contacts
                    .Where(contact =>
                        contact.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                        || contact.Phone.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }

            Contacts.Clear();
            foreach (var contact in contacts)
            {
                Contacts.Add(contact);
            }

            SelectedContact = selectedContactId is null
                ? null
                : Contacts.FirstOrDefault(contact => contact.Id == selectedContactId.Value);

            StatusMessage = $"Контактов в списке: {Contacts.Count}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка чтения данных: {exception.Message}";
        }
    }

    private void StartNewContact()
    {
        SelectedContact = null;
        Editor.StartNew();
    }

    private void DeleteSelectedContact()
    {
        if (SelectedContact is null)
        {
            return;
        }

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var contact = context.Contacts.Find(SelectedContact.Id);
            if (contact is null)
            {
                StatusMessage = "Контакт уже отсутствует в базе данных.";
                LoadContacts();
                return;
            }

            context.Contacts.Remove(contact);
            context.SaveChanges();

            var deletedName = contact.Name;
            LoadContacts();
            Editor.StartNew();
            StatusMessage = $"Контакт «{deletedName}» удалён.";
        }
        catch (DbUpdateException exception)
        {
            StatusMessage = $"Ошибка удаления: {exception.GetBaseException().Message}";
        }
        catch (InvalidOperationException exception)
        {
            StatusMessage = $"Ошибка удаления: {exception.Message}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка удаления: {exception.Message}";
        }
    }
}
