// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// Stores some default parameters for the game.
    /// </summary>
    [DataContract("GameSettings")]
    [ContentSerializer(typeof(DataContentSerializer<GameSettings>))]
    public sealed class GameSettings
    {
        public const string AssetUrl = "GameSettings";

        public GameSettings()
        {
            EffectCompilation = EffectCompilationMode.Local;
        }

        public Guid PackageId { get; set; }

        public string PackageName { get; set; }

        public string DefaultSceneUrl { get; set; }

        public string DefaultGraphicsCompositorUrl { get; set; }

        public string SplashScreenUrl { get; set; }

        public Color4 SplashScreenColor { get; set; }

        /// <summary>
        /// Gets or sets the compilation mode used.
        /// </summary>
        public CompilationMode CompilationMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether effect compile should be allowed, and if yes, should it be done locally (if possible) or remotely?
        /// </summary>
        public EffectCompilationMode EffectCompilation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether effect compile (local or remote) should be recorded and sent to effect compile server for GameStudio notification.
        /// </summary>
        public bool RecordUsedEffects { get; set; }

        /// <summary>
        /// Gets or sets configuration for the actual running platform as compiled during build
        /// </summary>
        public PlatformConfigurations Configurations { get; set; }
    }
}
