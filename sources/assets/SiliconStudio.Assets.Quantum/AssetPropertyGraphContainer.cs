// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetPropertyGraphContainer
    {
        private readonly Dictionary<AssetId, AssetPropertyGraph> registeredGraphs = new Dictionary<AssetId, AssetPropertyGraph>();

        public AssetPropertyGraphContainer([NotNull] AssetNodeContainer nodeContainer)
        {
            NodeContainer = nodeContainer ?? throw new ArgumentNullException(nameof(nodeContainer));
        }

        [NotNull]
        public AssetNodeContainer NodeContainer { get; }

        public bool PropagateChangesFromBase { get; set; } = true;

        [CanBeNull]
        public AssetPropertyGraph InitializeAsset([NotNull] AssetItem assetItem, ILogger logger)
        {
            // SourceCodeAssets have no property
            if (assetItem.Asset is SourceCodeAsset)
                return null;

            var graph = AssetQuantumRegistry.ConstructPropertyGraph(this, assetItem, logger);
            RegisterGraph(graph);
            return graph;
        }

        [CanBeNull]
        public AssetPropertyGraph TryGetGraph(AssetId assetId)
        {
            registeredGraphs.TryGetValue(assetId, out var graph);
            return graph;
        }

        public void RegisterGraph([NotNull] AssetPropertyGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            registeredGraphs.Add(graph.Id, graph);
        }

        public bool UnregisterGraph(AssetId assetId)
        {
            return registeredGraphs.Remove(assetId);
        }
    }
}
