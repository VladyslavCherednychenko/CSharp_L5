using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;

namespace CSharp_L5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public class ProductModel
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private int _unitsInStock;
            public int UnitsInStock
            {
                get { return _unitsInStock; }
                set
                {
                    if (_unitsInStock != value)
                    {
                        _unitsInStock = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnitsInStock)));
                    }
                }
            }

            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                    }
                }
            }

            private decimal _purchasePrice;
            public decimal PurchasePrice
            {
                get { return _purchasePrice; }
                set
                {
                    if (_purchasePrice != value)
                    {
                        _purchasePrice = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchasePrice)));
                    }
                }
            }

            private decimal _sellingPrice;
            public decimal SellingPrice
            {
                get { return _sellingPrice; }
                set
                {
                    if (_sellingPrice != value)
                    {
                        _sellingPrice = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SellingPrice)));
                    }
                }
            }
        }

        public class CustomerAction
        {
            public string ProductName;
            public int UnitsPurchased;
            public int UnitsInStock;
            public decimal AmountPaid;
            public decimal Casa;
        }

        public class ReAccountingAction
        {
            public string ProductName;
            public int UnitsReplenished;
            public int UnitsInStock;
            public decimal AmountPaid;
            public decimal Casa;
        }

        private List<ProductModel> _products;
        private decimal _money = 0.0m;
        private Random _random;

        public ObservableCollection<ProductModel> Products
        {
            get { return new ObservableCollection<ProductModel>(_products); }
        }

        public MainWindow()
        {
            InitializeComponent();

            _products = new List<ProductModel>
            {
                new ProductModel { Name = "Product A", UnitsInStock = 50, PurchasePrice = 10.0m, SellingPrice = 20.0m },
                new ProductModel { Name = "Product B", UnitsInStock = 30, PurchasePrice = 15.0m, SellingPrice = 30.0m },
                new ProductModel { Name = "Product C", UnitsInStock = 20, PurchasePrice = 20.0m, SellingPrice = 40.0m }
            };

            _random = new Random();

            DataContext = this;
        }

        private void OpenForCustomersModeButton_Click(object sender, RoutedEventArgs e)
        {
            Thread customerThread = new Thread(() =>
            {
                for (int i = 0; i < _random.Next(1, 5); i++)
                {
                    int productIndex = _random.Next(0, _products.Count);
                    ProductModel selectedProduct = _products[productIndex];
                    int numUnitsToPurchase = _random.Next(1, selectedProduct.UnitsInStock + 1);
                    decimal amountPaid = numUnitsToPurchase * selectedProduct.SellingPrice;

                    if (selectedProduct.UnitsInStock - numUnitsToPurchase > 0)
                    {
                        selectedProduct.UnitsInStock -= numUnitsToPurchase;
                        _money += amountPaid;

                        CustomerAction customerAction = new CustomerAction
                        {
                            ProductName = selectedProduct.Name,
                            UnitsPurchased = numUnitsToPurchase,
                            UnitsInStock = selectedProduct.UnitsInStock,
                            AmountPaid = amountPaid,
                            Casa = _money
                        };

                        try
                        {
                            SerializeToXml(customerAction);
                        }
                        catch (IOException ex)
                        {
                            Debug.WriteLine($"Failed to serialize customer action: {ex.Message}");
                        }
                    }

                    Thread.Sleep(_random.Next(1000, 2000));
                }
            });

            customerThread.Start();
        }

        private void ReAccountingModeButton_Click(object sender, RoutedEventArgs e)
        {
            Thread reAccountingThread = new Thread(() =>
            {
                for (int i = 0; i < _random.Next(1, 5); i++)
                {
                    int productIndex = _random.Next(0, _products.Count);
                    ProductModel selectedProduct = _products[productIndex];
                    int numUnitsToPurchase = _random.Next(10, 20);
                    decimal amountPaid = numUnitsToPurchase * selectedProduct.PurchasePrice;

                    if (_money - amountPaid > 0)
                    {
                        selectedProduct.UnitsInStock += numUnitsToPurchase;
                        _money -= amountPaid;

                        ReAccountingAction reAccountingAction = new ReAccountingAction
                        {
                            ProductName = selectedProduct.Name,
                            UnitsReplenished = numUnitsToPurchase,
                            UnitsInStock = selectedProduct.UnitsInStock,
                            AmountPaid = amountPaid,
                            Casa = _money
                        };

                        try
                        {
                            OnPropertyChanged(nameof(_products));
                            SerializeToXml(reAccountingAction);
                        }
                        catch (IOException ex)
                        {
                            Debug.WriteLine($"Failed to serialize re-accounting action: {ex.Message}");
                        }
                    }

                    Thread.Sleep(_random.Next(1000, 2000));
                }
            });

            reAccountingThread.Start();
        }

        private void SerializeToXml<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (TextWriter writer = new StreamWriter("log.xml", true))
            {
                serializer.Serialize(writer, obj);
            }
        }
    }
}
