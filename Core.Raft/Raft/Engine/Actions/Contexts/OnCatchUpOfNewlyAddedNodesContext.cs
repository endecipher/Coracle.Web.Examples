﻿using Coracle.Raft.Engine.Node;
using Coracle.Raft.Engine.States;

namespace Coracle.Raft.Engine.Actions.Contexts
{
    internal sealed class OnCatchUpOfNewlyAddedNodesContextDependencies
    {
        public ICurrentStateAccessor CurrentStateAccessor { get; set; }
        public IEngineConfiguration EngineConfiguration { get; set; }
    }

    internal sealed class OnCatchUpOfNewlyAddedNodesContext : IActionContext
    {
        public OnCatchUpOfNewlyAddedNodesContext(long logEntryIndex, string[] nodesToCheck, OnCatchUpOfNewlyAddedNodesContextDependencies dependencies)
        {
            LogEntryIndex = logEntryIndex;
            NodesToCheck = nodesToCheck;
            Dependencies = dependencies;
        }

        public bool IsContextValid
        {
            get
            {
                var state = CurrentStateAccessor.Get();
                return !state.IsDisposed && !state.StateValue.IsAbandoned() && !state.StateValue.IsStopped();
            }
        }

        internal string[] NodesToCheck { get; }
        internal long LogEntryIndex { get; }

        OnCatchUpOfNewlyAddedNodesContextDependencies Dependencies { get; set; }

        #region Action Dependencies

        internal ICurrentStateAccessor CurrentStateAccessor => Dependencies.CurrentStateAccessor;
        internal IEngineConfiguration EngineConfiguration => Dependencies.EngineConfiguration;

        #endregion

        public void Dispose()
        {
            Dependencies = null;
        }
    }
}
