﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class ItemToIndex : ValueConverterBase<ItemToIndex>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = parameter as IList;
            if (collection == null || collection.Count <= 0)
                return -1;

            // 1st attempt using the default item lookup
            var res = collection.IndexOf(value);
            if (res != -1 || value == null)
                return res;

            try
            {
                // 2nd attempt using a normalizing conversion (to double):
                var asDoubles = collection.Cast<object>().Select(x => (double)System.Convert.ChangeType(x, typeof(double))).ToList();
                Debug.Assert(asDoubles.SequenceEqual(asDoubles.OrderBy(d => d)));

                var search = asDoubles.BinarySearch((double)value);
                if (search < 0) // Note: BinarySearch returns a 1-complement of the index when an exact match is not found.
                    search = Math.Min(~search, collection.Count - 1);
                return search;
            }
            catch (FormatException) { }
            catch (InvalidCastException) { }
            catch (OverflowException) { }

            return -1;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = parameter as IList;
            if (collection == null)
                return null;

            var index = ConverterHelper.ConvertToInt32(value ?? -1, culture);
            if (index < 0 || index >= collection.Count)
                return null;

            return collection[index];
        }
    }
}
