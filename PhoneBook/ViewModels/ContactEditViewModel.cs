using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PhoneBook.Commands;
using PhoneBook.Data;
using PhoneBook.Models;

namespace PhoneBook.ViewModels;

public class ContactSavedEventArgs : EventArgs
{
    public ContactSavedEventArgs(int contactId)
    {
        ContactId = contactId;
    }

    public int ContactId { get; }
}

public class ContactEditViewModel : ViewModelBase
{
    private readonly IDbContextFactory<PhoneBookDbContext> _contextFactory;
    private int? _editingContactId;
    private string _nameInput = string.Empty;
    private string _phoneInput = string.Empty;
    private string _statusMessage = string.Empty;

    public ContactEditViewModel(IDbContextFactory<PhoneBookDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        ClearCommand = new RelayCommand(_ => StartNew());
    }

    public event EventHandler<ContactSavedEventArgs>? ContactSaved;

    public ICommand SaveCommand { get; }

    public ICommand ClearCommand { get; }

    public string NameInput
    {
        get => _nameInput;
        set
        {
            if (SetProperty(ref _nameInput, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string PhoneInput
    {
        get => _phoneInput;
        set
        {
            if (SetProperty(ref _phoneInput, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsEditing => _editingContactId.HasValue;

    public string FormTitle => IsEditing ? "Редактирование контакта" : "Новый контакт";

    public string PrimaryActionText => IsEditing ? "Сохранить" : "Добавить";

    public void StartNew()
    {
        _editingContactId = null;
        NameInput = string.Empty;
        PhoneInput = string.Empty;
        StatusMessage = string.Empty;
        OnEditModeChanged();
    }

    public void LoadContact(Contact? contact)
    {
        if (contact is null)
        {
            StartNew();
            return;
        }

        _editingContactId = contact.Id;
        NameInput = contact.Name;
        PhoneInput = contact.Phone;
        OnEditModeChanged();
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(NameInput)
            && !string.IsNullOrWhiteSpace(PhoneInput);
    }

    private void Save()
    {
        var name = NameInput.Trim();
        var phone = PhoneInput.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
        {
            StatusMessage = "Заполните имя и телефон.";
            return;
        }

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var wasEditing = _editingContactId.HasValue;
            Contact contact;

            if (_editingContactId is int contactId)
            {
                contact = context.Contacts.Find(contactId)
                    ?? throw new InvalidOperationException("Контакт не найден в базе данных.");

                contact.Name = name;
                contact.Phone = phone;
            }
            else
            {
                contact = new Contact
                {
                    Name = name,
                    Phone = phone
                };

                context.Contacts.Add(contact);
            }

            context.SaveChanges();
            _editingContactId = contact.Id;
            StatusMessage = wasEditing ? "Изменения сохранены." : "Контакт добавлен.";
            OnEditModeChanged();
            ContactSaved?.Invoke(this, new ContactSavedEventArgs(contact.Id));
        }
        catch (DbUpdateException exception)
        {
            StatusMessage = $"Ошибка сохранения: {exception.GetBaseException().Message}";
        }
        catch (InvalidOperationException exception)
        {
            StatusMessage = $"Ошибка сохранения: {exception.Message}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка сохранения: {exception.Message}";
        }
    }

    private void OnEditModeChanged()
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(PrimaryActionText));
        ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
    }
}
