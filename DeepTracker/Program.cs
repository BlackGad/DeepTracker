using System;
using DeepTracker.ComponentModel.Navigation;
using DeepTracker.Test;

namespace DeepTracker
{
    class Program
    {
        #region Static members

        static void Main(string[] args)
        {
            var tracker = new ComponentModel.DeepTracker.DeepTracker();
            var test = new MyClass();

            Console.WriteLine("Initial");
            Console.ReadLine();
            for (int i = 0; i < 10000; i++)
            {
                test.Test(tracker);
            }

            Console.WriteLine("after scope lose");
            Console.ReadLine();
        }

        #endregion

        #region Nested type: MyClass

        class MyClass
        {
            #region Members

            public void Test(ComponentModel.DeepTracker.DeepTracker tracker)
            {
                var root = new DependencyObject1
                {
                    Child = new DependencyObject1()
                };

                root.Child.Child = root;

                tracker.ObjectPropertyChanged += (sender, eventArgs) => { };

                tracker.Track(root, Route.Create(Route.WildcardRecursive));

                tracker.Activate();

                root.StringValue = new Simple();
                root.Child = new DependencyObject1();
                root.Child.StringValue = new Simple();

                tracker.Deactivate();
                tracker.Untrack(root, Route.Create(Route.WildcardRecursive));
            }

            #endregion
        }

        #endregion
    }
}