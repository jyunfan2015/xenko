// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    public class ShaderSourceComparer : EqualityComparer<ShaderSource>
    {
        private CompositionComparer compositionComparer;

        public ShaderSourceComparer()
        {
            compositionComparer = new CompositionComparer(this);
        }

        public override bool Equals(ShaderSource x, ShaderSource y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            if (x.GetType() != y.GetType())
                return false;

            if (x is ShaderClassSource)
            {
                var x1 = (ShaderClassSource)x;
                var y1 = (ShaderClassSource)y;
                return x1.ClassName == y1.ClassName
                       && ArrayExtensions.ArraysEqual(x1.GenericArguments, y1.GenericArguments);
            }
            if (x is ShaderMixinSource)
            {
                var x1 = (ShaderMixinSource)x;
                var y1 = (ShaderMixinSource)y;
                return ArrayExtensions.ArraysEqual(x1.Mixins, y1.Mixins, this)
                       && ArrayExtensions.ArraysEqual(x1.Compositions.OrderBy(item => item.Key).ToArray(), y1.Compositions.OrderBy(item => item.Key).ToArray(), compositionComparer);
            }
            if (x is ShaderArraySource)
            {
                var x1 = (ShaderArraySource)x;
                var y1 = (ShaderArraySource)y;
                return ArrayExtensions.ArraysEqual(x1.Values, y1.Values, this);
            }

            throw new InvalidOperationException("Invalid ShaderSource comparison.");
        }

        public override int GetHashCode(ShaderSource obj)
        {
            if (obj == null)
                return 0;

            unchecked
            {
                if (obj is ShaderClassSource)
                {
                    var obj1 = (ShaderClassSource)obj;
                    return obj1.ClassName.GetHashCode()
                           ^ ArrayExtensions.ComputeHash(obj1.GenericArguments);
                }
                if (obj is ShaderMixinSource)
                {
                    var obj1 = (ShaderMixinSource)obj;
                    return ArrayExtensions.ComputeHash(obj1.Mixins, this)
                           ^ ArrayExtensions.ComputeHash(obj1.Compositions.OrderBy(item => item.Key).ToArray(), compositionComparer);
                }
                if (obj is ShaderArraySource)
                {
                    var obj1 = (ShaderArraySource)obj;
                    return ArrayExtensions.ComputeHash(obj1.Values, this);
                }

                throw new InvalidOperationException("Invalid ShaderSource comparison.");
            }
        }

        private class CompositionComparer : EqualityComparer<KeyValuePair<string, ShaderSource>>
        {
            private ShaderSourceComparer shaderSourceComparer;

            public CompositionComparer(ShaderSourceComparer shaderSourceComparer)
            {
                this.shaderSourceComparer = shaderSourceComparer;
            }

            public override bool Equals(KeyValuePair<string, ShaderSource> x, KeyValuePair<string, ShaderSource> y)
            {
                return x.Key == y.Key && shaderSourceComparer.Equals(x.Value, y.Value);
            }

            public override int GetHashCode(KeyValuePair<string, ShaderSource> obj)
            {
                return obj.Key.GetHashCode() ^ shaderSourceComparer.GetHashCode(obj.Value);
            }
        }
    }
}
