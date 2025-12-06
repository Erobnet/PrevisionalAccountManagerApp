using System.ComponentModel;
using System.Runtime.CompilerServices;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;

namespace PrevisionalAccountManager.ViewModels;

public sealed class TransactionViewModel(TransactionModel? data = null) : ViewModel, ITransactionModel
{
    public TransactionModel Model {
        get;
        private set {
            if ( Equals(value, field) )
                return;

            field = value;
            Category = value?.Category != null
                ? new CategoryViewModel(value.Category)
                : null;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(Observations));
            OnPropertyChanged(nameof(Date));
            OnPropertyChanged(nameof(Category));
        }
    } = data ?? new();

    public TransactionViewModel(TransactionViewModel other) : this(new TransactionModel(other.Model))
    { }

    public TransactionViewModel ResetData(TransactionModel? data = null)
    {
        if ( data is not null )
        {
            Model = new(data);
        }
        else
        {
            Model.Observations = "";
            Model.Amount = 0;
            Model.Id = Guid.Empty;
            Model.Date = DateTime.Today;
        }

        return this;
    }

    public Guid Id {
        get => Model.Id;
        set {
            Model.Id = value;
            OnPropertyChanged();
        }
    }

    public Amount Amount {
        get => Model.Amount;
        set {
            Model.Amount = value;
            OnPropertyChanged();
        }
    }

    public string Observations {
        get => Model.Observations;
        set {
            Model.Observations = value;
            OnPropertyChanged();
        }
    }

    public DateTime Date {
        get => Model.Date;
        set {
            Model.Date = value;
            OnPropertyChanged();
        }
    }

    public CategoryViewModel? Category {
        get => field;
        set {
            field = value;
            Model.Category = value?.Model;
            Model.CategoryId = value?.Id;
            OnPropertyChanged();
        }
    } = data?.Category != null ? new(data.Category) : null;
}