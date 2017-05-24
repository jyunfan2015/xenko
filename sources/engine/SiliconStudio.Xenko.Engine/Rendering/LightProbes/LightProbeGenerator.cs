// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.ComputeEffect.LambertianPrefiltering;
using SiliconStudio.Xenko.Rendering.Images.SphericalHarmonics;
using SiliconStudio.Xenko.Rendering.LightProbes;
using SiliconStudio.Xenko.Rendering.Skyboxes;

namespace SiliconStudio.Xenko.Rendering.LightProbes
{
    public static class LightProbeGenerator
    {
        public const int LambertHamonicOrder = 3;

        private static readonly ProfilingKey LightProbeProfilingKey = new ProfilingKey("LightProbes", ProfilingKeyFlags.GpuProfiling);
        private static readonly ProfilingKey PrefilterSH = new ProfilingKey(LightProbeProfilingKey, "Prefilter SphericalHarmonics", ProfilingKeyFlags.GpuProfiling);

        public static Dictionary<LightProbeComponent, FastList<Color3>> GenerateCoefficients(ISceneRendererContext context, LightProbeComponent[] lightProbes)
        {
            using (var cubemapRenderer = new CubemapSceneRenderer(context, 256))
            {
                // Create target cube texture
                var cubeTexture = Texture.NewCube(context.GraphicsDevice, 256, PixelFormat.R16G16B16A16_Float);

                // Prepare shader for SH prefiltering
                var lambertFiltering = new LambertianPrefilteringSHNoCompute(cubemapRenderer.DrawContext.RenderContext)
                {
                    HarmonicOrder = LambertHamonicOrder,
                    RadianceMap = cubeTexture,
                };

                var lightProbesCoefficients = new Dictionary<LightProbeComponent, FastList<Color3>>();

                using (cubemapRenderer.DrawContext.PushRenderTargetsAndRestore())
                {
                    // Render light probe
                    context.GraphicsContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Red, LightProbeProfilingKey);

                    int lightProbeIndex = 0;
                    foreach (var entity in context.SceneSystem.SceneInstance)
                    {
                        var lightProbe = entity.Get<LightProbeComponent>();
                        if (lightProbe == null)
                            continue;

                        var lightProbePosition = lightProbe.Entity.Transform.WorldMatrix.TranslationVector;
                        context.GraphicsContext.ResourceGroupAllocator.Reset(context.GraphicsContext.CommandList);

                        context.GraphicsContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Red, new ProfilingKey(LightProbeProfilingKey, $"LightProbes {lightProbeIndex}", ProfilingKeyFlags.GpuProfiling));
                        lightProbeIndex++;

                        cubemapRenderer.Draw(lightProbePosition, cubeTexture);

                        context.GraphicsContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Red, PrefilterSH);

                        // Compute SH coefficients
                        lambertFiltering.Draw(cubemapRenderer.DrawContext);

                        var coefficients = lambertFiltering.PrefilteredLambertianSH.Coefficients;
                        var lightProbeCoefficients = new FastList<Color3>();
                        for (int i = 0; i < coefficients.Length; i++)
                        {
                            lightProbeCoefficients.Add(coefficients[i]*SphericalHarmonics.BaseCoefficients[i]);
                        }

                        lightProbesCoefficients.Add(lightProbe, lightProbeCoefficients);

                        context.GraphicsContext.CommandList.GpuQueryProfiler.EndProfile(); // Prefilter SphericalHarmonics

                        context.GraphicsContext.CommandList.GpuQueryProfiler.EndProfile(); // Face XXX

                        // Debug render
                    }

                    context.GraphicsContext.CommandList.GpuQueryProfiler.EndProfile(); // LightProbes
                }

                cubeTexture.Dispose();

                return lightProbesCoefficients;
            }
        }

        public static unsafe void UpdateCoefficients(LightProbeRuntimeData runtimeData)
        {

            fixed (Color3* destColors = runtimeData.Coefficients)
            {
                for (var lightProbeIndex = 0; lightProbeIndex < runtimeData.LightProbes.Length; lightProbeIndex++)
                {
                    var lightProbe = runtimeData.LightProbes[lightProbeIndex];

                    // Copy coefficients
                    if (lightProbe.Coefficients != null)
                    {
                        var lightProbeCoefStart = lightProbeIndex * LambertHamonicOrder * LambertHamonicOrder;
                        for (var index = 0; index < LambertHamonicOrder * LambertHamonicOrder; index++)
                        {
                            destColors[lightProbeCoefStart + index] = index < lightProbe.Coefficients.Count ? lightProbe.Coefficients[index] : new Color3();
                        }
                    }
                }
            }
        }

        public static unsafe LightProbeRuntimeData GenerateRuntimeData(FastList<LightProbeComponent> lightProbes)
        {
            // TODO: Better check: coplanar, etc... (maybe the check inside BowyerWatsonTetrahedralization might be enough -- tetrahedron won't be in positive order)
            if (lightProbes.Count < 4)
                throw new InvalidOperationException("Can't generate lightprobes if less than 4 of them exists.");

            var lightProbePositions = new FastList<Vector3>();
            var lightProbeCoefficients = new Color3[lightProbes.Count * LambertHamonicOrder * LambertHamonicOrder];
            fixed (Color3* destColors = lightProbeCoefficients)
            {
                for (var lightProbeIndex = 0; lightProbeIndex < lightProbes.Count; lightProbeIndex++)
                {
                    var lightProbe = lightProbes[lightProbeIndex];

                    // Copy light position
                    lightProbePositions.Add(lightProbe.Entity.Transform.WorldMatrix.TranslationVector);

                    // Copy coefficients
                    if (lightProbe.Coefficients != null)
                    {
                        var lightProbeCoefStart = lightProbeIndex * LambertHamonicOrder * LambertHamonicOrder;
                        for (var index = 0; index < LambertHamonicOrder * LambertHamonicOrder; index++)
                        {
                            destColors[lightProbeCoefStart + index] = index < lightProbe.Coefficients.Count ? lightProbe.Coefficients[index] : new Color3();
                        }
                    }
                }
            }

            // Generate light probe structure
            var tetra = new BowyerWatsonTetrahedralization();
            var tetraResult = tetra.Compute(lightProbePositions);

            var matrices = new Vector4[tetraResult.Tetrahedra.Count * 3];
            var probeIndices = new Int4[tetraResult.Tetrahedra.Count];

            // Prepare data for GPU: matrices and indices
            for (int i = 0; i < tetraResult.Tetrahedra.Count; ++i)
            {
                var tetrahedron = tetraResult.Tetrahedra[i];
                var tetrahedronMatrix = Matrix.Identity;

                // Compute the tetrahedron matrix
                // https://en.wikipedia.org/wiki/Barycentric_coordinate_system#Barycentric_coordinates_on_tetrahedra
                var vertex3 = tetraResult.Vertices[tetrahedron.Vertices[3]];
                *((Vector3*)&tetrahedronMatrix.M11) = tetraResult.Vertices[tetrahedron.Vertices[0]] - vertex3;
                *((Vector3*)&tetrahedronMatrix.M12) = tetraResult.Vertices[tetrahedron.Vertices[1]] - vertex3;
                *((Vector3*)&tetrahedronMatrix.M13) = tetraResult.Vertices[tetrahedron.Vertices[2]] - vertex3;
                tetrahedronMatrix.Invert(); // TODO: Optimize 3x3 invert

                tetrahedronMatrix.Transpose();

                // Store position of last vertex in last row
                tetrahedronMatrix.M41 = vertex3.X;
                tetrahedronMatrix.M42 = vertex3.Y;
                tetrahedronMatrix.M43 = vertex3.Z;

                matrices[i * 3 + 0] = tetrahedronMatrix.Column1;
                matrices[i * 3 + 1] = tetrahedronMatrix.Column2;
                matrices[i * 3 + 2] = tetrahedronMatrix.Column3;

                probeIndices[i] = *(Int4*)tetrahedron.Vertices;
            }

            var lightProbesCopy = new LightProbeComponent[lightProbes.Count];
            for (int i = 0; i < lightProbes.Count; ++i)
                lightProbesCopy[i] = lightProbes[i];

            var result = new LightProbeRuntimeData
            {
                LightProbes = lightProbesCopy,
                Vertices = tetraResult.Vertices,
                UserVertexCount = tetraResult.UserVertexCount,
                Tetrahedra = tetraResult.Tetrahedra,
                Faces = tetraResult.Faces,

                Coefficients = lightProbeCoefficients,
                Matrices = matrices,
                LightProbeIndices = probeIndices,
            };

            return result;
        }
    }

    public class LightProbeRuntimeData
    {
        // Input data
        public LightProbeComponent[] LightProbes;

        // Computed data
        public Vector3[] Vertices;
        public int UserVertexCount;
        public FastList<BowyerWatsonTetrahedralization.Tetrahedron> Tetrahedra;
        public FastList<BowyerWatsonTetrahedralization.Face> Faces;

        // Data to upload to GPU
        public Color3[] Coefficients;
        public Vector4[] Matrices;
        public Int4[] LightProbeIndices;
    }
}
