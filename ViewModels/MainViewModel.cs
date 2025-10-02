using ElectroShop.Models;
using ElectroShop.Services;
using ElectroShop.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;
using System;
using System.Globalization;

namespace ElectroShop
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
    public enum EditMode
    {
        View,
        Add,
        Edit
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ProductService _productService;
        private ElectronicProduct _selectedProduct;
        private ElectronicProduct _currentProduct;
        private ElectronicProduct _originalProduct; // Для отмены изменений
        private EditMode _currentMode;

        public MainViewModel()
        {
            _productService = new ProductService();
            Products = new ObservableCollection<ElectronicProduct>();
            CurrentMode = EditMode.View;

            LoadProducts();

            // Команды с правильными условиями доступности
            NewCommand = new RelayCommand(StartNewProduct, () => CurrentMode == EditMode.View);
            EditCommand = new RelayCommand(StartEditProduct, () => CurrentMode == EditMode.View && SelectedProduct != null);
            DeleteCommand = new RelayCommand(DeleteProduct, () => CurrentMode == EditMode.View && SelectedProduct != null);
            SaveCommand = new RelayCommand(SaveProduct, () => CurrentMode != EditMode.View && IsCurrentProductValid());
            CancelCommand = new RelayCommand(CancelEdit, () => CurrentMode != EditMode.View);
        }

        public ObservableCollection<ElectronicProduct> Products { get; set; }

        public EditMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInViewMode));
                OnPropertyChanged(nameof(IsInEditMode));
                OnPropertyChanged(nameof(ModeDescription));
                
                // Обновляем доступность команд
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsInViewMode => CurrentMode == EditMode.View;
        public bool IsInEditMode => CurrentMode != EditMode.View;

        public string ModeDescription
        {
            get
            {
                return CurrentMode switch
                {
                    EditMode.Add => "Добавление нового товара",
                    EditMode.Edit => "Редактирование товара",
                    _ => "Просмотр товаров"
                };
            }
        }

        public ElectronicProduct SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();

                // Обновляем CurrentProduct только в режиме просмотра
                if (CurrentMode == EditMode.View && value != null)
                {
                    CurrentProduct = CreateProductCopy(value);
                }
            }
        }

        public ElectronicProduct CurrentProduct
        {
            get
            {
                if (_currentProduct == null)
                    _currentProduct = new ElectronicProduct();
                return _currentProduct;
            }
            set
            {
                _currentProduct = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand NewCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        private void LoadProducts()
        {
            Products.Clear();
            var products = _productService.GetProducts();
            foreach (var product in products)
                Products.Add(product);
        }

        private void StartNewProduct()
        {
            CurrentMode = EditMode.Add;
            SelectedProduct = null;
            CurrentProduct = new ElectronicProduct();
            _originalProduct = null;
        }

        private void StartEditProduct()
        {
            if (SelectedProduct != null)
            {
                CurrentMode = EditMode.Edit;
                _originalProduct = CreateProductCopy(SelectedProduct);
                CurrentProduct = CreateProductCopy(SelectedProduct);
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct != null)
            {
                _productService.DeleteProduct(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
                CurrentProduct = new ElectronicProduct();
            }
        }

        private void SaveProduct()
        {
            if (!IsCurrentProductValid())
                return;

            if (CurrentMode == EditMode.Add)
            {
                _productService.AddProduct(CurrentProduct);
                LoadProducts();
                // Выбираем добавленный товар
                SelectedProduct = Products.FirstOrDefault(p => p.Title == CurrentProduct.Title && 
                                                              p.Company == CurrentProduct.Company);
            }
            else if (CurrentMode == EditMode.Edit)
            {
                _productService.UpdateProduct(CurrentProduct);
                LoadProducts();
                // Выбираем обновленный товар
                SelectedProduct = Products.FirstOrDefault(p => p.Id == CurrentProduct.Id);
            }

            CurrentMode = EditMode.View;
            _originalProduct = null;
        }

        private void CancelEdit()
        {
            if (CurrentMode == EditMode.Edit && _originalProduct != null)
            {
                // Восстанавливаем исходные данные
                CurrentProduct = CreateProductCopy(_originalProduct);
            }
            else if (CurrentMode == EditMode.Add)
            {
                // Очищаем поля при отмене добавления
                CurrentProduct = new ElectronicProduct();
                SelectedProduct = null;
            }

            CurrentMode = EditMode.View;
            _originalProduct = null;
        }

        private bool IsCurrentProductValid()
        {
            return !string.IsNullOrWhiteSpace(CurrentProduct.Title) &&
                   !string.IsNullOrWhiteSpace(CurrentProduct.Company) &&
                   !string.IsNullOrWhiteSpace(CurrentProduct.Category) &&
                   CurrentProduct.Price > 0 &&
                   CurrentProduct.StockQuantity >= 0;
        }

        private ElectronicProduct CreateProductCopy(ElectronicProduct source)
        {
            return new ElectronicProduct
            {
                Id = source.Id,
                Title = source.Title,
                Company = source.Company,
                Category = source.Category,
                Price = source.Price,
                StockQuantity = source.StockQuantity
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
