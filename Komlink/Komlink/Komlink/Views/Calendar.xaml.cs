using Komlink.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Schema;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for Calendar.xaml
    /// </summary>
    public partial class Calendar : UserControl
    {

        private DateTime currentDate;
        private DateTime? selectedStartDate;
        private DateTime? selectedEndDate;

        public event EventHandler SelectedStartDateChanged;
        public event EventHandler SelectedEndDateChanged;

        public ObservableCollection<DayItem> Days { get; set; }
        public ICommand SelectDateCommand { get; set; }

        public Calendar()
        {
            InitializeComponent();

            LoadLanguage();
            Days = new ObservableCollection<DayItem>();
            DataContext = this;

            currentDate = DateTime.Today;
            UpdateCalendar();

            SelectDateCommand = new RelayCommand(SelectDate, CanSelectDate);
        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    SunText.Text = "AHA";
                    MonText.Text = "ISN";
                    TueText.Text = "SEL";
                    WedText.Text = "RAB";
                    ThuText.Text = "KHA";
                    FriText.Text = "JUM";
                    SatText.Text = "SAB";
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in Calendar.xaml.cs");

            }
        }

        private void UpdateCalendar()
        {
            MonthYearTextBlock.Text = currentDate.ToString("MMM yyyy");

            Days.Clear();

            //calculate the first and last day of the month
            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            //calculate previous month


            DateTime previousMonth = firstDayOfMonth.AddMonths(-1);
            int dayInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            int startDayOffset = (int)firstDayOfMonth.DayOfWeek;
            for(int i = dayInPreviousMonth - startDayOffset + 1; i <= dayInPreviousMonth; i ++)
            {
                
                DateTime currentDay = new DateTime(previousMonth.Year, previousMonth.Month, i);

                bool isInRange = selectedStartDate.HasValue && selectedEndDate.HasValue && currentDay > selectedStartDate.Value && currentDay < selectedEndDate.Value;

                bool isSelected = selectedStartDate.HasValue && currentDay ==  selectedStartDate.Value;

                bool isEndDate = selectedEndDate.HasValue && currentDay == selectedEndDate.Value;

                Days.Add(new DayItem { Day = i.ToString(), IsInRange = isInRange, IsSelected = isSelected, IsCurrentMonth = false, IsPreviousYear = true, IsNextYear = false, IsEndDate = isEndDate});
            }



            for(int i = 1; i <= lastDayOfMonth.Day; i ++)
            {
                DateTime currentDay = new DateTime(currentDate.Year, currentDate.Month, i);
                bool isInRange = selectedStartDate.HasValue && selectedEndDate.HasValue && currentDay > selectedStartDate.Value && currentDay < selectedEndDate.Value;
                bool isSelected = selectedStartDate.HasValue && currentDay == selectedStartDate.Value;
                bool isEndDate = selectedEndDate.HasValue && currentDay == selectedEndDate.Value;
                Days.Add(new DayItem { Day = i.ToString(), IsInRange = isInRange, IsSelected = isSelected ,IsCurrentMonth = true, IsPreviousYear =false, IsNextYear = false, IsEndDate = isEndDate });
            }

            // Add the incoming days from the next month
            int remainingDays = 42 - Days.Count;
            for (int i = 1; i <= remainingDays; i++)
            {
                if(currentDate.Month < 12)
                {
                    DateTime currentDay = new DateTime(currentDate.Year, currentDate.Month + 1, i);
                    bool isInRange = selectedStartDate.HasValue && selectedEndDate.HasValue && currentDay > selectedStartDate.Value && currentDay < selectedEndDate.Value;
                    bool isSelected = selectedStartDate.HasValue && currentDay == selectedStartDate.Value;
                    bool isEndDate = selectedEndDate.HasValue && currentDay == selectedEndDate.Value;
                    Days.Add(new DayItem { Day = i.ToString(), IsInRange = isInRange, IsSelected = isSelected, IsCurrentMonth = false, IsPreviousYear = false, IsNextYear = true, IsEndDate = isEndDate });
                }
                
            }

           

        }

       

        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(-1);
            UpdateCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(1);
            UpdateCalendar();
        }

        private void SelectDate(object parameter)
        {
            DayItem selectedDay = parameter as DayItem;
            DateTime? clickedDate = null;
            if (selectedDay.IsPreviousYear)
            {
                clickedDate = new DateTime(currentDate.Year, currentDate.Month - 1, int.Parse(selectedDay.Day));

            }
            else if (selectedDay.IsNextYear)
            {
                clickedDate = new DateTime(currentDate.Year , currentDate.Month + 1, int.Parse(selectedDay.Day));
            }
            else
            {
               clickedDate   = new DateTime(currentDate.Year, currentDate.Month, int.Parse(selectedDay.Day));
            }
          

            if (!selectedStartDate.HasValue || (selectedStartDate.HasValue && selectedEndDate.HasValue))
            {
                // First selection or both start and end dates have been selected
                selectedStartDate = clickedDate;
                SelectedStartDateChanged.DynamicInvoke(this.selectedStartDate, new EventArgs());
                selectedEndDate = null;
            }
            else if (selectedStartDate.HasValue && !selectedEndDate.HasValue && clickedDate >= selectedStartDate.Value)
            {
                // End date selection
                selectedEndDate = clickedDate;
                SelectedEndDateChanged.DynamicInvoke(this.selectedEndDate, new EventArgs());
                //setEndDate();

               
            }
            else if (selectedStartDate.HasValue && !selectedEndDate.HasValue && clickedDate < selectedStartDate.Value)
            {
                // Change start date
                selectedStartDate = clickedDate;

            }

            UpdateCalendar();

            //resetStartDate();
        }

        //private void resetStartDate()
        //{
        //    DayItem[] days = Days.ToArray();

        //    days[0].IsSelected = true;
        //}
        

        private bool CanSelectDate(object parameter)
        {
            // Add your own logic here if needed
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


public class DayItem : INotifyPropertyChanged
{

    private string day;
    private bool isInRange;
    private bool isSelected;
    private bool isCurrentMonth;
    private bool isPreviousYear;
    private bool isNextYear;
    private bool isEndDate;
    public string Day
    {
        get { return day; }
        set
        {
            day = value;
            OnPropertyChanged("Day");
        }
    }


    public bool IsInRange
    {
        get { return isInRange;}
        set
        {
            isInRange = value;
            OnPropertyChanged("IsInRange");
        }
    }

    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            OnPropertyChanged("IsSelected");
        }
    }

    public bool IsEndDate
    {
        get { return isEndDate; }
        set
        {
            isEndDate = value;
            OnPropertyChanged("IsEndDate");
        }
    }

    public bool IsCurrentMonth
    {
        get { return isCurrentMonth; }
        set
        {
            isCurrentMonth = value;
            
        }
    }

    public bool IsPreviousYear
    {
        get { return isPreviousYear; }
        set
        {
            isPreviousYear = value;
           
        }
    }

    public bool IsNextYear
    {
        get { return isNextYear; }
        set
        {
            isNextYear = value;
          

        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object> execute;
    private readonly Predicate<object> canExecute;


    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value;}
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
        return canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object parameter)
    {
       execute(parameter);
    }
}
