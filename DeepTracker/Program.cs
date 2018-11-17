using System;
using System.Linq;
using System.Reflection;
using DeepTracker1.ComponentModel;
using DeepTracker1.ComponentModel.Navigation;
using DeepTracker1.Extensions;
using DeepTracker1.Test;

namespace DeepTracker1
{
    class Program
    {
        #region Static members

        static void Main(string[] args)
        {
            var test = new MyClass();

            Console.WriteLine("Initialization...");
            //Console.ReadLine();

            //for (int i = 0; i < 100; i++)
            {
                test.Test();
            }

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        #endregion

        #region Nested type: MyClass

        class MyClass
        {
            #region Members

            public void Test()
            {
                var root = new TestObject("Root");

                using (var tracker = DeepTracker.Setup(root, Routes.WildcardRecursive)
                                                .Except(Routes.WildcardRecursive, "Child")
                                                .Create())
                {
                    tracker.ObjectPropertyChanged += (sender, args) =>
                    {
                        var message = $"PROPERTY '{args.Route}' changed: {args.OldValue ?? "<null>"} -> {args.NewValue ?? "<null>"}";
                        Console.WriteLine(message);
                    };
                    tracker.CollectionChanged += (sender, args) =>
                    {
                        var message = $"COLLECTION '{args.Route}' changed ({args.EventArgs.Action}): ";

                        var oldItems = args.EventArgs.OldItems.Enumerate().ToArray();
                        if (oldItems.Any()) message += $"Removed ({string.Join(", ", oldItems)}) ";

                        var newItems = args.EventArgs.NewItems.Enumerate().ToArray();
                        if (newItems.Any()) message += $"Added ({string.Join(", ", newItems)})";

                        Console.WriteLine(message);
                    };

                    tracker.Activate();
                    root.Child = new TestObject("Root_Initial_Child");
                    root.PropertyName = "asdasd";

                    root.Children = new TestCollection();
                    root.Children.Child = new TestObject("ddd");
                    root.Children = null;
                    root.Children = new TestCollection
                    {
                        new TestObject("Root_Initial_Collection_Child")
                    };

                    root.Children.Child = new TestObject("Collection_Child");

                    var rootRuntimeCollectionChild = new TestObject("Root_Runtime_Collection_Child");
                    root.Children.Add(rootRuntimeCollectionChild);
                    root.Children.Remove(rootRuntimeCollectionChild);

                    root.Child.Child = new TestObject("Root_Runtime_Child");
                    root.Child.Children = new TestCollection();

                    var childRuntimeCollectionChild = new TestObject("Child_Runtime_Collection_Child");
                    root.Child.Children.Add(childRuntimeCollectionChild);

                    childRuntimeCollectionChild.Child = new TestObject("Child_Runtime_Collection_Child_Child");
                }
            }

            #endregion
        }

        #endregion
    }
}