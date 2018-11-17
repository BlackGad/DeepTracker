using System.Windows;

namespace DeepTracker1.Test
{
    internal class TestObject : DependencyObject
    {
        #region Property definitions

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child",
                                        typeof(TestObject),
                                        typeof(TestObject),
                                        new PropertyMetadata(default(TestObject)));

        public static readonly DependencyProperty ChildrenProperty =
            DependencyProperty.Register("Children",
                                        typeof(TestCollection),
                                        typeof(TestObject),
                                        new PropertyMetadata(default(TestCollection)));

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName",
                                        typeof(string),
                                        typeof(TestObject),
                                        new FrameworkPropertyMetadata(default(string)));
        #endregion

        private readonly string _name;

        #region Constructors

        public TestObject(string name)
        {
            _name = name;
        }

        #endregion

        #region Properties

        public TestObject Child
        {
            get { return (TestObject)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public TestCollection Children
        {
            get { return (TestCollection)GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }

        #endregion

        #region Override members

        public override string ToString()
        {
            return _name;
        }

        #endregion
    }
}