﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BackEnd;
using LiveCharts;
using LiveCharts.Wpf;

namespace WPF_UI
{
    public partial class ExpensesManager
    {
        private StatsCalculations StatsCalc { get; set; }
        public SeriesCollection PieChartSeriesCollection { get; set; } = new SeriesCollection();
        public static MonthExpenses MonthlyExpenses { get; set; } = new MonthExpenses();
        private Func<ChartPoint, string> PointLabel { get; set; }

        public ExpensesManager()
        {
            DataContext = this;
            InitializeComponent();

            MonthlyExpenses.Expenses.CollectionChanged += Expenses_CollectionChanged;

            PointLabel = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            StatsCalc = new StatsCalculations();
        }

        private async Task NotificationBoxClearAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            NotificationTextBox.Clear();
        }

        private async Task CalcPieStatsAsync()
        {
            foreach (var item in StatsCalc.StatsPerCategotyAndTotalCalc(MonthlyExpenses.Expenses.ToList()))
            {
                PieChartSeriesCollection.Add(new PieSeries
                {
                    Title = item.Key,
                    Values = new ChartValues<double> { item.Value },
                    PushOut = 5,
                    DataLabels = true,
                    LabelPoint = PointLabel
                });
            }
        }
        private async Task CalcBalanceAsync()
        {
            var fontColor = Brushes.Green;
            if (StatsCalc.Balance < 0)
            {
                fontColor = Brushes.Red;
            }

            TotalBalanceBox.Text = StatsCalc.Balance.ToString();
            TotalBalanceBox.Foreground = fontColor;
        }
        private async void TriggerStatsCalcAsync()
        {
            PieChartSeriesCollection.Clear();
            try
            {
                await CalcPieStatsAsync();
                await CalcBalanceAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async void DataGridNewRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatePicker.SelectedDate != null)
            {
                MonthlyExpenses.Expenses.Add(new ExpensesObj("new", "new", 0, 0, Guid.NewGuid()));
            }
            else
            {
                await UiNotification("Please select Moth to fill monthly expenses ");
            }
            
        }

        public async Task UiNotification(string message)
        {
            NotificationTextBox.Text = message;
            await NotificationBoxClearAsync();
        }

        private void Expenses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TriggerStatsCalcAsync();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ExpensesObj addedExpensesObj = MonthlyExpenses.Expenses.Last();
                    addedExpensesObj.ExpensesObjChanged += Item_ExpensesObjChanged;
                    MonthlyExpenses.UpdateExObj_ToDB(UpdateAction.Add, addedExpensesObj);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var removeExpensesObj = e.OldItems[0] as ExpensesObj;
                    MonthlyExpenses.UpdateExObj_ToDB(UpdateAction.Remove, removeExpensesObj);
                    break;
                default:
                    foreach (var item in MonthlyExpenses.Expenses)
                    {
                        item.ExpensesObjChanged += Item_ExpensesObjChanged;
                    }
                    break;
            }
        }
        private void Item_ExpensesObjChanged(Guid toUpdateObjGuid)
        {
            TriggerStatsCalcAsync();
            foreach (var expensesObj in MonthlyExpenses.Expenses)
            {
                if (expensesObj.IdGuid == toUpdateObjGuid)
                {
                    MonthlyExpenses.UpdateExObj_ToDB(UpdateAction.Update, expensesObj);
                    break;
                }
            }
        }
        private void OnDelete(object sender, RoutedEventArgs e)
        {
            //todo: if rowid 0 popup delete all
            ExpensesObj removeExpensesObj = ((FrameworkElement)sender).DataContext as ExpensesObj;
            MonthlyExpenses.Expenses.Remove(removeExpensesObj);
        }
        private void DataGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dg = sender as DataGrid;

            // alter this condition for whatever valid keys you want - avoid arrows/tab, etc.
            if (dg != null && !dg.IsReadOnly && StaticMethods.isAlphanumeric(e.Key))
            {
                var cell = dg.GetSelectedCell();
                if (cell != null && cell.Column is DataGridTemplateColumn)
                {
                    cell.Focus();
                    dg.BeginEdit();

                    TextBox textbox = FindVisualChild<TextBox>(cell);
                    if (textbox != null && textbox.IsFocused == false)
                    {
                        textbox.Focus();

                        textbox.Clear();
                        textbox.AppendText(StaticMethods.GetCharFromKey(e.Key).ToString());
                        textbox.CaretIndex = textbox.Text.Length;
                    }

                    e.Handled = true;
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
