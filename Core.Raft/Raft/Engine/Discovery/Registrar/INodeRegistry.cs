﻿using Coracle.Raft.Engine.Configuration.Cluster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coracle.Raft.Engine.Discovery.Registrar
{
    /// <summary>
    /// Can be used by <see cref="INodeRegistrar"/> to maintain registry of nodes
    /// </summary>
    public interface INodeRegistry
    {
        Task AddOrUpdate(NodeConfiguration configuration);
        Task<IEnumerable<NodeConfiguration>> GetAll();
        Task TryRemove(string nodeId);
    }
}