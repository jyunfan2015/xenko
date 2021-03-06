// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Diagnostics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Represents a unique pointer that is or was on the screen and information about it
    /// </summary>
    public class PointerPoint
    {
        /// <summary>
        /// Last position of the pointer
        /// </summary>
        public Vector2 Position = Vector2.Zero;

        /// <summary>
        /// Pointer delta
        /// </summary>
        public Vector2 Delta;

        /// <summary>
        /// Is the pointer currently down
        /// </summary>
        public bool IsDown;

        /// <summary>
        /// The pointer ID, from the device
        /// </summary>
        public int Id;

        /// <summary>
        /// The device to which this pointer belongs
        /// </summary>
        public IPointerDevice Pointer;

        public override string ToString()
        {
            return $"Pointer [{Id}] {nameof(Position)}: {Position}, {nameof(Delta)}: {Delta}, {nameof(IsDown)}: {IsDown}, {nameof(Pointer)}: {Pointer}";
        }
    }
}