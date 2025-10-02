using ElectroShop.Models;
using System.Collections.Generic;
using System.Linq;

namespace ElectroShop.Services
{
    public class ProductService
    {
        private static readonly List<ElectronicProduct> _products = new List<ElectronicProduct>();
        private static int _nextId = 1;
        private static bool _initialized = false;

        // Статичный класс с "БД" в памяти

        public ProductService()
        {
            if (!_initialized)
            {
                InitializeTestData();
                _initialized = true;
            }
        }

        private void InitializeTestData()
        {
            _products.AddRange(new[]
            {
                new ElectronicProduct { Id = _nextId++, Title = "iPhone 15 Pro", Company = "Apple", Category = "Смартфоны", Price = 99990, StockQuantity = 15 },
                new ElectronicProduct { Id = _nextId++, Title = "Samsung Galaxy S24", Company = "Samsung", Category = "Смартфоны", Price = 79990, StockQuantity = 8 },
                new ElectronicProduct { Id = _nextId++, Title = "MacBook Pro 16\"", Company = "Apple", Category = "Ноутбуки", Price = 249990, StockQuantity = 5 },
                new ElectronicProduct { Id = _nextId++, Title = "Dell XPS 13", Company = "Dell", Category = "Ноутбуки", Price = 89990, StockQuantity = 12 },
                new ElectronicProduct { Id = _nextId++, Title = "Sony WH-1000XM5", Company = "Sony", Category = "Наушники", Price = 29990, StockQuantity = 25 },
                new ElectronicProduct { Id = _nextId++, Title = "AirPods Pro 2", Company = "Apple", Category = "Наушники", Price = 24990, StockQuantity = 18 }
            });
        }

        public List<ElectronicProduct> GetProducts()
        {
            return _products.ToList();
        }

        public void AddProduct(ElectronicProduct product)
        {
            product.Id = _nextId++;
            _products.Add(product);
        }

        public void UpdateProduct(ElectronicProduct product)
        {
            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                existing.Title = product.Title;
                existing.Company = product.Company;
                existing.Category = product.Category;
                existing.Price = product.Price;
                existing.StockQuantity = product.StockQuantity;
            }
        }

        public void DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
            }
        }
    }
}
