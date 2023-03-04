﻿using Coracle.Raft.Engine.Node;
using Coracle.Web.Impl.Remoting;
using Microsoft.Extensions.Options;

namespace Coracle.Web.Impl.Node
{
    public interface ICoracleNodeAccessor
    {
        ICoracleNode CoracleNode { get; }
        IEngineConfiguration EngineConfig { get; }
        void Ready();
    }

    public class CoracleNodeAccessor : ICoracleNodeAccessor
    {
        private object _lock = new object();
        private ICoracleNode _node = null;

        public IEngineConfiguration EngineConfig { get; }

        public ICoracleNode CoracleNode
        {
            get
            {
                lock (_lock)
                {
                    return _node;
                }
            }

            private set
            {
                lock (_lock)
                {
                    if (_node == null) 
                        _node = value;
                }
            }
        }

        public CoracleNodeAccessor(IAppInfo appInfo, IEngineConfiguration engineConfig, ICoracleNode node, IOptions<EngineConfigurationOptions> engineConfigurationOptions)
        {
            (engineConfig as EngineConfigurationSettings).ApplyFrom(engineConfigurationOptions.Value);

            (engineConfig as EngineConfigurationSettings).NodeUri = appInfo.GetCurrentAppUri();

            EngineConfig = engineConfig;
            
            CoracleNode = node;
        }

        public void Ready()
        {
            if(!CoracleNode.IsInitialized)
                CoracleNode.InitializeConfiguration();
            
            if(!CoracleNode.IsStarted)
                CoracleNode.Start();
        }
    }
}
