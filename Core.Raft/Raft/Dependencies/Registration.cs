﻿using Coracle.Raft.Engine.Actions;
using Coracle.Raft.Engine.Command;
using Coracle.Raft.Engine.Configuration.Alterations;
using Coracle.Raft.Engine.Configuration.Cluster;
using Coracle.Raft.Engine.Discovery;
using Coracle.Raft.Engine.Helper;
using Coracle.Raft.Engine.Node;
using Coracle.Raft.Engine.Node.Validations;
using Coracle.Raft.Engine.Remoting;
using Coracle.Raft.Engine.States;
using Coracle.Raft.Engine.States.LeaderEntities;
using TaskGuidance.BackgroundProcessing.Dependencies;

namespace Coracle.Raft.Dependencies
{
    /// <summary>
    /// The following dependencies which handle extensible logic need an external implementation to be registered for Constructor Dependecy Injection -
    /// <list type="bullet">
    /// <item><see cref = "IStateMachineHandler" /> - For state machine and application of commands</item>
    /// <item><see cref = "IOutboundRequestHandler" /> - For outbound RPCs</item>
    /// <item><see cref = "IPersistentStateHandler" /> - For holding the node's persistent state, snapshots and logEntries</item>
    /// <item><see cref = "IDiscoveryHandler" /> - For initial discovery of other Coracle nodes</item>
    /// </list>
    /// 
    /// The implementation <see cref="EngineConfigurationSettings"/> of the interface <see cref="IEngineConfiguration"/> must be populated/modified externally before <see cref="ICoracleNode.InitializeConfiguration"/> is called.
    /// 
    /// <para/>
    /// <see cref="EngineRegistry.Register(IDependencyContainer)"/> must be called during Dependency Injection Container Setup to register necessary internal implementations. 
    /// 
    /// <para/>
    /// The server implementating <see cref="ICoracleNode"/> may call on the below via DI for Coracle Node operations - 
    /// <list type="bullet">
    /// <item><see cref = "ICommandExecutor" /> - For handling inbound client commands </item>
    /// <item><see cref = "ICoracleNode" /> - For initialization and start of the Coracle Node</item>
    /// <item><see cref = "IRemoteCallExecutor" /> - For handling inbound RPC calls from other Coracle nodes</item>
    /// <item><see cref = "IConfigurationRequestExecutor" /> - For handling cluster configuration change requests</item>
    /// </list>
    /// 
    /// </summary>
    public sealed class Registration : IDependencyRegistration
    {
        public void Register(IDependencyContainer container)
        {
            container.RegisterTransient<IRemoteCallExecutor, RemoteCallExecutor>(); 
            container.RegisterTransient<ICommandExecutor, CommandExecutor>();
            container.RegisterTransient<IConfigurationRequestExecutor, ConfigurationRequestExecutor>();
            container.RegisterSingleton<ICoracleNode, CoracleNode>(); 

            container.RegisterTransient<IStateChanger, StateChanger>();

            container.RegisterSingleton<INodeConfiguration, NodeConfiguration>(); 
            container.RegisterSingleton<IClusterConfiguration, ClusterConfiguration>(); 
            container.RegisterSingleton<ILeaderNodePronouncer, LeaderNodePronouncer>(); 
            container.RegisterSingleton<IElectionTimer, ElectionTimer>();
            container.RegisterSingleton<IHeartbeatTimer, HeartbeatTimer>();
            container.RegisterSingleton<IGlobalAwaiter, GlobalAwaiter>(); 
            container.RegisterSingleton<IEngineConfiguration, EngineConfigurationSettings>(); 
            container.RegisterSingleton<ICurrentStateAccessor, CurrentStateAccessor>();
            container.RegisterSingleton<IMembershipChanger, MembershipChanger>();
            container.RegisterSingleton<IAppendEntriesManager, AppendEntriesManager>();
            container.RegisterSingleton<IElectionManager, ElectionManager>();
            container.RegisterSingleton<ILeaderVolatileProperties, LeaderVolatileProperties>();
            container.RegisterSingleton<IEngineValidator, EngineValidator>();
            container.RegisterSingleton<ISystemClock, SystemClock>();
        }
    }
}
