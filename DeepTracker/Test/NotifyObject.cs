using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeepTracker.Extensions;

namespace DeepTracker.Test
{
    internal class NotifyObject : INotifyPropertyChanged
    {
        private NotifyObject _child;
        private Simple _stringValue;

        #region Properties

        public NotifyObject Child
        {
            get { return _child; }
            set
            {
                if (_child.AreEqual(value)) return;
                _child = value;
                OnPropertyChanged();
            }
        }

        public Simple StringValue
        {
            get { return _stringValue; }
            set
            {
                if (_stringValue.AreEqual(value)) return;
                _stringValue = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Members

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}