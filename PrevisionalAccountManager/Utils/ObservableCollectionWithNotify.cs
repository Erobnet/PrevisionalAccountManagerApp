using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using PrevisionalAccountManager.ViewModels;

namespace PrevisionalAccountManager.Utils;

public class ObservableCollectionWithItemNotify<T>(List<T> list) : ObservableCollection<T>(list)
    where T : INotifyPropertyChanged
{
    public event EventHandler<ItemPropertyChangedEventArgs<T>>? ItemPropertyChanged;

    public ObservableCollectionWithItemNotify() : this(new List<T>())
    {
        
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if ( e.OldItems != null )
        {
            foreach ( T item in e.OldItems )
            {
                item.PropertyChanged -= Item_PropertyChanged!;
            }
        }

        if ( e.NewItems != null )
        {
            foreach ( T item in e.NewItems )
            {
                item.PropertyChanged += Item_PropertyChanged!;
            }
        }

        base.OnCollectionChanged(e);
    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var itemPropertyChangedEventArgs = new ItemPropertyChangedEventArgs<T>((T)sender, e.PropertyName);
        ItemPropertyChanged?.Invoke(this, itemPropertyChangedEventArgs);
    }

    protected override void ClearItems()
    {
        foreach ( T item in Items )
        {
            item.PropertyChanged -= Item_PropertyChanged!;
        }
        base.ClearItems();
    }

    public void AddRange(IReadOnlyList<T> selectedItems)
    {
        foreach ( var viewModel in selectedItems )
        {
            base.Add(viewModel);
        }
    }
}

public readonly struct ItemPropertyChangedEventArgs<T>(T item, string? propertyName)
{
    public T Item { get; } = item;
    public string? PropertyName { get; } = propertyName;
}