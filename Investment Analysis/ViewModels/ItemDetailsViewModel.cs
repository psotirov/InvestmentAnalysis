using Investment_Analysis.Commands;
using Investment_Analysis.Common;
using Investment_Analysis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Investment_Analysis.ViewModels
{
    public class ItemDetailsViewModel : BindableBase
    {
        private ICommand setItemsType;
        private ICommand addNewItem;
        private ICommand removeSelectedItem;
        private ProjectData project;
        // selecting the current type of item details:
        // 0 = Investment Details, 1 = Expenses details, 2 = Incomes details
        private int itemTypeSelector;
        private List<AnalysisItem>[] itemsData;

        public string ProjectName { get { return this.project.ProjectName; } }

        public string MeasureUnit { get { return this.project.MeasureUnit; } }

        public string TotalValue
        {
            get
            {
                double result = 0;
                switch (itemTypeSelector)
                {
                    case 0:
                        result = this.project.TotalInvestment;
                        break;
                    case 1:
                        result = this.project.TotalExpenses;
                        break;
                    case 2:
                        result = this.project.TotalIncomes;
                        break;
                    default:
                        break;
                }

                return result.ToString("N2");
            }
        }
        
        private ObservableCollection<AnalysisItem> items;

        public IEnumerable<AnalysisItem> Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new ObservableCollection<AnalysisItem>();
                    this.items.CollectionChanged +=
                        new NotifyCollectionChangedEventHandler(HandleItemsCollectionChanged);
                    if (this.itemTypeSelector >= 0)
                    {
                        this.Items = this.itemsData[this.itemTypeSelector];
                    }
                }

                return items;
            }
            set
            {
                if (value != null)
                {
                    if (this.items == null)
                    {
                        this.items = new ObservableCollection<AnalysisItem>();
                        this.items.CollectionChanged +=
                            new NotifyCollectionChangedEventHandler(HandleItemsCollectionChanged);
                    }

                    this.items.Clear();
                    foreach (var item in value)
                    {
                        this.items.Add(item);
                    }

                    OnPropertyChanged("Items");
                }
            }
        }

        public ItemDetailsViewModel()
        {
            this.project = (ProjectData)App.Current.Resources["ProjectData"];
            this.itemTypeSelector = -1; // no default value
            this.itemsData = new List<AnalysisItem>[]
            {
                this.project.InvestmentItems,
                this.project.ExpenseItems,
                this.project.IncomeItems,
            };

        }

        public ICommand SetItemsType
        {
            get
            {
                if (this.setItemsType == null)
                {
                    this.setItemsType = new DelegateCommand<string>(HandleSetItemsType);
                }
                return this.setItemsType;
            }
        }
        
        public ICommand AddNewItem
        {
            get
            {
                if (this.addNewItem == null)
                {
                    this.addNewItem = new DelegateCommand<object>(HandleAddNewItem);
                }
                return this.addNewItem;
            }
        }

        public ICommand RemoveSelectedItem
        {
            get
            {
                if (this.removeSelectedItem == null)
                {
                    this.removeSelectedItem = new DelegateCommand<object>(HandleRemoveSelectedItem);
                }
                return this.removeSelectedItem;
            }
        }

        private void HandleSetItemsType(string itemType)
        {
            switch (itemType)
            {
                case "Investment":
                    this.itemTypeSelector = 0;
                    break;
                case "Expenses":
                    this.itemTypeSelector = 1;
                    break;
                case "Incomes":
                    this.itemTypeSelector = 2;
                    break;
                default:
                    break;
            }

            if (this.itemTypeSelector >= 0)
            {
                this.Items = this.itemsData[this.itemTypeSelector];
                OnPropertyChanged("TotalValue");
            }
        }

        private void HandleAddNewItem(object parameter)
        {
            int selectedIndex = (int)parameter;
            if (selectedIndex < 0 || selectedIndex >= this.items.Count)
            {
                this.items.Add(new AnalysisItem());
            }
            else
            {
                this.items.Insert(selectedIndex, new AnalysisItem());
            }
            
            this.HandleItemChanged(null, null);
            OnPropertyChanged("Items");
        }

        private void HandleRemoveSelectedItem(object parameter)
        {
            int selectedIndex = (int)parameter;
            if (selectedIndex < 0 || selectedIndex >= this.items.Count)
            {
                return;
            }

            this.items.RemoveAt(selectedIndex);
            this.HandleItemChanged(null, null);
            OnPropertyChanged("Items");
        }

        private void HandleItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (var oldItem in args.OldItems)
                {
                    (oldItem as AnalysisItem).PropertyChanged -= HandleItemChanged;
                }
            }

            if (args.NewItems != null)
            {
                foreach (var newItem in args.NewItems)
                {
                    (newItem as AnalysisItem).PropertyChanged += HandleItemChanged;
                }
            }
        }

        private void HandleItemChanged(object sender, PropertyChangedEventArgs args)
        {
            if (this.items != null && this.itemTypeSelector >= 0)
            {
                var container = this.itemsData[this.itemTypeSelector];
                double result = 0;
                container.Clear();
                foreach (var item in this.items)
                {
                    container.Add(item);
                    result += item.Value;
                }

                switch (this.itemTypeSelector)
                {
                    case 0: 
                        this.project.TotalInvestment = result;
                        break;
                    case 1:
                        this.project.TotalExpenses = result;
                        break;
                    case 2:
                        this.project.TotalIncomes = result;
                        break;
                    default:
                        break;
                }

                OnPropertyChanged("TotalValue");
            }
        }

    }
}
