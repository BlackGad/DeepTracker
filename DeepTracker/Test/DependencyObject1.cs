using System.Windows;

namespace DeepTracker.Test
{
    internal class DependencyObject1 : DependencyObject
    {
        #region Property definitions

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child",
                                        typeof(DependencyObject1),
                                        typeof(DependencyObject1),
                                        new PropertyMetadata(default(DependencyObject1)));

        public static readonly DependencyProperty StringValueProperty =
            DependencyProperty.Register("StringValue",
                                        typeof(Simple),
                                        typeof(DependencyObject1),
                                        new PropertyMetadata(default(Simple)));

        #endregion

        #region Properties

        public DependencyObject1 Child
        {
            get { return (DependencyObject1)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public Simple StringValue
        {
            get { return (Simple)GetValue(StringValueProperty); }
            set { SetValue(StringValueProperty, value); }
        }

        #endregion
    }
}