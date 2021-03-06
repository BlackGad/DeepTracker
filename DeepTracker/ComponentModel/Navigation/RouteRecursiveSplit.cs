﻿using System;

namespace DeepTracker1.ComponentModel.Navigation
{
    public class RouteRecursiveSplit
    {
        #region Constructors

        public RouteRecursiveSplit(Route prefix, Route recursive, Route postfix)
        {
            Recursive = recursive ?? throw new ArgumentNullException(nameof(recursive));
            Postfix = postfix ?? throw new ArgumentNullException(nameof(postfix));
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        #endregion

        #region Properties

        public Route Postfix { get; private set; }
        public Route Prefix { get; private set; }
        public Route Recursive { get; private set; }

        #endregion
    }
}