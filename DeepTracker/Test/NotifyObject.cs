using DeepTracker.Extensions;

namespace DeepTracker.Test
{
    internal class NotifyObject : NotifyBase
    {
        private NotifyObject _child;
        private string _stringValue;

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

        public string StringValue
        {
            get { return _stringValue; }
            set
            {
                if (_stringValue.AreEqual(value)) return;
                _stringValue = value;
                OnPropertyChanged();
            }
        }
    }
}