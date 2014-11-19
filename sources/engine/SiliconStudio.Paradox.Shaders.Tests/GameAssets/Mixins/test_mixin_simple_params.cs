﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Paradox Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Paradox.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;


#line 3 "D:\Code\Paradox\sources\engine\SiliconStudio.Paradox.Shaders.Tests\GameAssets\Mixins\test_mixin_simple_params.pdxfx"
namespace Test7
{
    [DataContract]
#line 5
    public partial class TestParameters : ShaderMixinParameters
    {

        #line 7
        public static readonly ParameterKey<bool> param1 = ParameterKeys.New<bool>();

        #line 8
        public static readonly ParameterKey<int> param2 = ParameterKeys.New<int>(1);

        #line 9
        public static readonly ParameterKey<string> param3 = ParameterKeys.New<string>("ok");
    };

    #line 12
    internal partial class DefaultSimpleParams  : IShaderMixinBuilder
    {
        public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
        {

            #line 16
            context.Mixin(mixin, "A");

            #line 17
            context.Mixin(mixin, "B");

            #line 20
            if (context.GetParam(TestParameters.param1))
            {

                #line 23
                context.Mixin(mixin, "C");

                #line 26
                mixin.Mixin.AddMacro("param2", context.GetParam(TestParameters.param2));

                {

                    #line 29
                    var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };

                    #line 29
                    context.Mixin(__subMixin, "X");
                    mixin.Mixin.AddComposition("x", __subMixin.Mixin);
                }
            }

            #line 32
            else
            {

                #line 33
                context.Mixin(mixin, "D");

                #line 34
                mixin.Mixin.AddMacro("Test", context.GetParam(TestParameters.param3));

                {

                    #line 35
                    var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };

                    #line 35
                    context.Mixin(__subMixin, "Y");
                    mixin.Mixin.AddComposition("y", __subMixin.Mixin);
                }
            }
        }

        [ModuleInitializer]
        internal static void __Initialize__()

        {
            ShaderMixinManager.Register("DefaultSimpleParams", new DefaultSimpleParams());
        }
    }
}
