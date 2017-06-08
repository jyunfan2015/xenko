﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Assets.Tests.Helpers;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Quantum.Tests.Helpers
{
    public static class SerializationHelper
    {
        public static readonly AssetId BaseId = (AssetId)GuidGenerator.Get(1);
        public static readonly AssetId DerivedId = (AssetId)GuidGenerator.Get(2);

        public static void SerializeAndCompare(AssetItem assetItem, AssetPropertyGraph graph, string expectedYaml, bool isDerived)
        {
            assetItem.Asset.Id = isDerived ? DerivedId : BaseId;
            Assert.AreEqual(isDerived, assetItem.Asset.Archetype != null);
            if (isDerived)
                assetItem.Asset.Archetype = new AssetReference(BaseId, assetItem.Asset.Archetype?.Location);
            graph.PrepareForSave(null, assetItem);
            var stream = new MemoryStream();
            AssetFileSerializer.Save(stream, assetItem.Asset, assetItem.YamlMetadata, null);
            stream.Position = 0;
            var streamReader = new StreamReader(stream);
            var yaml = streamReader.ReadToEnd();
            Assert.AreEqual(expectedYaml, yaml);
        }

        public static void SerializeAndCompare(object instance, YamlAssetMetadata<OverrideType> overrides, string expectedYaml)
        {
            var stream = new MemoryStream();
            var metadata = new AttachedYamlAssetMetadata();
            metadata.AttachMetadata(AssetObjectSerializerBackend.OverrideDictionaryKey, overrides);
            AssetFileSerializer.Default.Save(stream, instance, metadata, null);
            stream.Position = 0;
            var streamReader = new StreamReader(stream);
            var yaml = streamReader.ReadToEnd();
            Assert.AreEqual(expectedYaml, yaml);
        }
    }
}