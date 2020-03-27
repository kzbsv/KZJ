#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KZJ
{
    /// <summary>
    /// Class must define CloneBySerialization() method.
    /// </summary>
    public interface ICloneBySerialization
    {
        /// <summary>
        /// Return a copy of this instance created by serializing and deserializing the instance.
        /// </summary>
        /// <returns></returns>
        object CloneBySerialization();
    }

    /// <summary>
    /// Utility generic EventArgs class for events that return a single value.
    /// </summary>
    /// <typeparam name="T">Type of the value passed with the event.</typeparam>
    public class ValueEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The value passed with the event.
        /// </summary>
        public T Value;
        /// <summary>
        /// Creates an EventArgs instance containing a single value.
        /// </summary>
        /// <param name="value">The value to pass with the event.</param>
        public ValueEventArgs(T value) { Value = value; }
    }
}
