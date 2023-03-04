﻿using ActivityLogger.Logging;
using Coracle.Raft.Engine.Configuration.Cluster;
using Coracle.Raft.Engine.Remoting;
using Coracle.Raft.Engine.ActivityLogger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coracle.Raft.Engine.States
{
    internal interface ICandidateDependencies : IStateDependencies
    {
        IElectionManager ElectionManager { get; set; }
        IOutboundRequestHandler RemoteManager { get; set; }
    }

    internal sealed class Candidate : AbstractState, ICandidateDependencies
    {
        #region Constants
        public new const string Entity = nameof(Candidate);
        public const string StartingElection = nameof(StartingElection);
        public const string incrementedTerm = nameof(incrementedTerm);
        #endregion

        #region Additional Dependencies
        public IElectionManager ElectionManager { get; set; }
        public IOutboundRequestHandler RemoteManager { get; set; }

        #endregion

        public Candidate() : base()
        {
            StateValue = StateValues.Candidate;
        }

        /// <remarks>
        /// The third possible outcome is that a candidate neither wins nor loses the election: if many followers become candidates at the same time, 
        /// votes could be split so that no candidate obtains a majority.When this happens, each candidate will time out and start a new election 
        /// by incrementing its term and initiating another round of RequestVote RPCs. 
        /// 
        /// However, without extra measures split votes could repeat indefinitely. 
        /// 
        /// Raft uses randomized election timeouts to ensure that split votes are rare and that they are resolved quickly. 
        /// To prevent split votes in the first place, election timeouts are chosen randomly from a fixed interval (e.g., 150–300ms).
        /// This spreads out the servers so that in most cases only a single server will time out; it wins the election and sends heartbeats before 
        /// any other servers time out. The same mechanism is used to handle split votes.
        /// Each candidate restarts its randomized election timeout at the start of an election, and it waits for that timeout to elapse before starting 
        /// the next election; this reduces the likelihood of another split vote in the new election.
        /// <see cref="Section 5.2 Leader Election"/>
        /// </remarks>
        protected override void OnElectionTimeout(object state)
        {
            StartElection().Wait();
        }

        /// <summary>
        /// On conversion to candidate, start election:
        /// • Increment currentTerm
        /// • Vote for self
        /// • Reset election timer
        /// • Send RequestVote RPCs to all other servers
        /// • If votes received from majority of servers: become leader
        /// • If AppendEntries RPC received from new leader: convert to
        /// follower
        /// • If election timeout elapses: start new election
        /// </summary>
        /// <returns></returns>
        public async Task StartElection()
        {
            var term = await PersistentState.IncrementCurrentTerm();

            await PersistentState.SetVotedFor(EngineConfiguration.NodeId);

            ActivityLogger.Log(new CoracleActivity
            {
                EntitySubject = Entity,
                Event = StartingElection,
                Level = ActivityLogLevel.Debug
            }
            .With(ActivityParam.New(incrementedTerm, term))
            .WithCallerInfo());

            ElectionManager.Initiate(term);

            ElectionTimer.ResetWithDifferentTimeout();
        }

        public override async Task OnStateChangeBeginDisposal()
        {
            ElectionManager.CancelSessionIfExists();

            await base.OnStateChangeBeginDisposal();
        }

        public override async Task OnStateEstablishment()
        {
            await base.OnStateEstablishment();

            await StartElection();
        }

        public override void UpdateMembership(IEnumerable<INodeConfiguration> newPeerNodeConfigurations)
        {
            ElectionManager.UpdateMembership(newPeerNodeConfigurations);
        }
    }
}
