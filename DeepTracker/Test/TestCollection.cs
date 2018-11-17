using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DeepTracker1.Test
{
    internal class TestCollection : ObservableCollection<TestObject>
    {
        private TestObject _child;

        #region Properties

        public TestObject Child
        {
            get { return _child; }
            set
            {
                _child = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Child)));
            }
        }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"[{string.Join(", ", this)}]";
        }

        #endregion
    }
}