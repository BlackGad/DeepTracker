using DeepTracker.Test;

namespace DeepTracker
{
    class Program
    {
        #region Static members

        static void Main(string[] args)
        {
            var root = new NotifyObject();
            var tracker = new ComponentModel.DeepTracker.DeepTracker(root);

            tracker.Track(nameof(root.StringValue));
            tracker.Track(nameof(root.Child), nameof(root.StringValue));

            tracker.Activate();

            root.StringValue = "42";
        }

        #endregion
    }
}