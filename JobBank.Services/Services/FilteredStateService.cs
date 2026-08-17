namespace JobBank.Services
{
    public class FilteredStateService
    {
        public DateTime? FromDate { get; private set; }
        public DateTime? ToDate { get; private set; }

        public event Action OnChange;

        public void UpdateFilters(DateTime? fromDate, DateTime? toDate)
        {
            FromDate = fromDate;
            ToDate = toDate;
            NotifyPropertyChanged();
        }

        private void NotifyPropertyChanged() => OnChange?.Invoke();
    }
}
