using System.ComponentModel;
namespace Investment_Analysis.Models
{
    public class AnalysisItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string title;
        private double value;
        private bool isVariable;
        private string remark;

        public string Title
        {
            get { return this.title; }
            set
            {
                this.title = value;
                OnPropertyChanged("Title");
            }
        }

        public double Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }
        
        public bool IsVariable
        {
            get { return this.isVariable; }
            set
            {
                this.isVariable = value;
                OnPropertyChanged("IsVariable");
            }
        }

        public string Remark
        {
            get { return this.remark; }
            set
            {
                this.remark = value;
                OnPropertyChanged("Remark");
            }
        }

        public AnalysisItem()
        {
            this.isVariable = false;
            this.remark = string.Empty;
            this.title = string.Empty;
            this.value = 0;
        }

        protected void OnPropertyChanged(string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
