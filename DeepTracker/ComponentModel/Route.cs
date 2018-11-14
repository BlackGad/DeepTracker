using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeepTracker.Extensions;

namespace DeepTracker.ComponentModel
{
    public class Route : IReadOnlyList<string>, IEquatable<Route>
    {
        #region Static members

        public static Route Enumeration { get; }

        public static bool operator ==(Route left, Route right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Route left, Route right)
        {
            return !Equals(left, right);
        }

        #endregion

        private readonly string _fullRoute;
        private readonly IReadOnlyList<string> _routes;

        #region Constructors

        static Route()
        {
            Enumeration = new Route("__Enumeration");
        }

        public Route(params object[] path)
        {
            var routes = new List<string>();
            foreach (var part in path.Enumerate())
            {
                switch (part)
                {
                    case string stringPart:
                        routes.AddRange(stringPart.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    case Route routePart:
                        routes.AddRange(routePart);
                        break;
                    default:
                        throw new NotSupportedException($"{part} could not be parsed as route part");
                }
            }

            _routes = routes;
            _fullRoute = string.Join(".", _routes.Select(r => $"({r})"));
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return _routes.Count == 0; }
        }

        #endregion

        #region Override members

        public override string ToString()
        {
            return _fullRoute;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Route)obj);
        }

        public override int GetHashCode()
        {
            return _fullRoute.GetHash();
        }

        #endregion

        #region IEquatable<Route> Members

        public bool Equals(Route other)
        {
            return string.Equals(_fullRoute, other?._fullRoute);
        }

        #endregion

        #region IReadOnlyList<string> Members

        public int Count
        {
            get { return _routes.Count; }
        }

        public string this[int index]
        {
            get { return _routes[index]; }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _routes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}