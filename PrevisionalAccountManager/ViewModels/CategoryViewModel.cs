using System.ComponentModel;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;

namespace PrevisionalAccountManager.ViewModels
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        private readonly CategoryModel _model;

        public CategoryViewModel(CategoryModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Gets the underlying CategoryModel
        /// </summary>
        public CategoryModel Model => _model;

        /// <summary>
        /// Gets the category ID
        /// </summary>
        public int Id => _model.Id;

        /// <summary>
        /// Gets or sets the category name
        /// </summary>
        public string Name {
            get => _model.Name;
            set {
                if ( _model.Name != value )
                {
                    _model.Name = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// Gets the display name for UI binding (same as Name but can be extended)
        /// </summary>
        public string DisplayName => _model.Name;

        /// <summary>
        /// Determines if this category is valid for operations
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(_model.Name);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object? obj)
        {
            if ( obj is CategoryViewModel other )
            {
                return Id == other.Id;
            }
            if ( obj is CategoryModel model )
            {
                return Id == model.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}