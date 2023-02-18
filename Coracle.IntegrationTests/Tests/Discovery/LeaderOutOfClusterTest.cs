﻿using ActivityLogger.Logging;
using Coracle.Samples.ClientHandling.Notes;
using Coracle.Raft.Engine.Actions.Awaiters;
using Coracle.Raft.Engine.Actions.Core;
using Coracle.Raft.Engine.ClientHandling;
using Coracle.Raft.Engine.ClientHandling.Command;
using Coracle.Raft.Engine.Configuration;
using Coracle.Raft.Engine.Configuration.Cluster;
using Coracle.Raft.Engine.Helper;
using Coracle.Raft.Engine.States;
using EntityMonitoring.FluentAssertions.Structure;
using FluentAssertions;
using Xunit;
using Coracle.Raft.Engine.Remoting.RPC;

namespace Coracle.IntegrationTests.Framework
{
    [TestCaseOrderer($"Coracle.IntegrationTests.Framework.{nameof(ExecutionOrderer)}", $"Coracle.IntegrationTests")]
    public class LeaderOutOfClusterTest : BaseTest, IClassFixture<TestContext>
    {
        public LeaderOutOfClusterTest(TestContext context) : base(context)
        {
        }

        [Fact]
        [Order(1)]
        public async Task IsTurningToLeader()
        {
            //Arrange
            Context.GetService<IActivityMonitor<Activity>>().Start();

            var notifiableQueue = CaptureActivities();

            Exception caughtException = null;
            ClientHandlingResult clientHandlingResult = null;
            var (Command, Note) = TestAddCommand();

            var majorityAttained = notifiableQueue
                .AttachNotifier(x => x.Is(ElectionManager.Entity, ElectionManager.MajorityAttained));

            var commitIndexUpdated = notifiableQueue
                .AttachNotifier(x =>
                    x.Is(AbstractState.Entity, AbstractState.ApplyingLogEntry));

            StateCapture captureAfterCommand = null;

            //Act
            try
            {
                InitializeEngineConfiguration();
                CreateMockNode(MockNodeA);
                CreateMockNode(MockNodeB);
                RegisterMockNodeInRegistrar(MockNodeA);
                RegisterMockNodeInRegistrar(MockNodeB);

                InitializeNode();
                StartNode();

                EnqueueRequestVoteSuccessResponse(MockNodeA);
                EnqueueRequestVoteSuccessResponse(MockNodeB);

                var electionTimer = Context.GetService<IElectionTimer>() as TestElectionTimer;
                //This will make sure that the ElectionTimer callback invocation is approved for the Candidacy
                electionTimer.AwaitedLock.ApproveNext();

                ////Wait until Majority has been attained
                majorityAttained.Wait(EventNotificationTimeOut);

                var heartBeatTimer = Context.GetService<IHeartbeatTimer>() as TestHeartbeatTimer;

                EnqueueAppendEntriesSuccessResponse(MockNodeA);
                EnqueueAppendEntriesSuccessResponse(MockNodeB);

                //This will make sure that the Heartbeat callback invocation is approved for SendAppendEntries
                heartBeatTimer.AwaitedLock.ApproveNext();

                commitIndexUpdated.Wait(EventNotificationTimeOut);

                var isPronouncedLeaderSelf = Context.GetService<ILeaderNodePronouncer>().IsLeaderRecognized
                    && Context.GetService<ILeaderNodePronouncer>().RecognizedLeaderConfiguration.UniqueNodeId.Equals(SUT);

                await Task.Delay(1000);

                EnqueueAppendEntriesSuccessResponse(MockNodeA);
                EnqueueAppendEntriesSuccessResponse(MockNodeB);

                clientHandlingResult = await Context.GetService<IExternalClientCommandHandler>()
                    .HandleClientCommand(Command, CancellationToken.None);

                commitIndexUpdated.Wait(EventNotificationTimeOut);

                captureAfterCommand = new StateCapture(Context.GetService<ICurrentStateAccessor>().Get());
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            #region Assertions

            var assertableQueue = StartAssertions(notifiableQueue);

            caughtException
                .Should().Be(null, $" ");

            majorityAttained.IsConditionMatched.Should().BeTrue();
            commitIndexUpdated.IsConditionMatched.Should().BeTrue();

            Context.GetService<INotes>()
                .TryGet(Note.UniqueHeader, out var note)
                .Should().BeTrue();

            note.Should().NotBeNull("Note must exist");

            note.Text.Should().Be(Note.Text, "Note should match the testNote supplied");

            captureAfterCommand
                .CommitIndex
                .Should().Be(2, "The command entry having index 2 should have been replicated to other nodes and also comitted");

            captureAfterCommand
                .LastApplied
                .Should().Be(2, "The command entry having index 2 should have been replicated to other nodes and also comitted");

            captureAfterCommand
                .MatchIndexes
                .Values
                .Should()
                .Match((i) => i.All(_ => _.Equals(2)),
                    $"{MockNodeA} and {MockNodeB} should have had replicated all entries up until the leader's last log index");

            captureAfterCommand
                .NextIndexes
                .Values
                .Should()
                .Match((i) => i.All(_ => _.Equals(3)),
                    $"{MockNodeA} and {MockNodeB} should have had replicated all entries up until the leader's last log index, and the nextIndex to send for each peer mock node should be one greater, i.e 3");

            Cleanup();
            #endregion
        }



        //[Fact]
        //[Order(2)]
        //public async Task IsLeaderCommitingCommandEntries()
        //{
        //    #region Arrange

        //    var notifiableQueue = CaptureActivities();

        //    var commitIndexUpdated = notifiableQueue
        //        .AttachNotifier(x =>
        //            x.Is(AbstractState.Entity, AbstractState.ApplyingLogEntry));

        //    var snapshotBuilt = notifiableQueue
        //        .AttachNotifier(x =>
        //            x.Is(OnCompaction.ActionName, OnCompaction.LogCompacted)).RemoveOnceMatched();

        //    #endregion

        //    #region Act

        //    Exception caughtException = null;
        //    ClientHandlingResult clientHandlingResult = null;

        //    StateCapture captureAfterCommands = null; 
        //    var threshold = TestStateProperties.SnapshotThreshold;
            
        //    var lastIndex = await Context.GetService<IPersistentProperties>().GetLastIndex();
            
        //    (bool HasSnapshot, ISnapshotHeader Detail) info = (false, null);

        //    var nextIndexToStart = lastIndex + 1;
            
        //    try
        //    {
        //        // Add new commands until threshold surpasses
        //        for (int i = (int) nextIndexToStart; i <= threshold + 1; i++)
        //        {
        //            var (Command, Note) = TestAddCommand();

        //            EnqueueAppendEntriesSuccessResponse(MockNodeA);
        //            EnqueueAppendEntriesSuccessResponse(MockNodeB);

        //            clientHandlingResult = await Context.GetService<IExternalClientCommandHandler>()
        //                .HandleClientCommand(Command, CancellationToken.None);

        //            commitIndexUpdated.Wait(EventNotificationTimeOut);

        //            if (!commitIndexUpdated.IsConditionMatched)
        //                break;
        //        }

        //        captureAfterCommands = new StateCapture(Context.GetService<ICurrentStateAccessor>().Get());

        //        snapshotBuilt.Wait(EventNotificationTimeOut);

        //        info = await Context.GetService<IPersistentProperties>().HasCommittedSnapshot(lastIndex);

                 
        //    }
        //    catch (Exception e)
        //    {
        //        caughtException = e;
        //    }

        //    #endregion

        //    #region Assert

        //    var assertableQueue = StartAssertions(notifiableQueue);
            
        //    caughtException
        //        .Should().Be(null, $" ");

        //    captureAfterCommands
        //        .CommitIndex
        //        .Should().Be(threshold + 1, "The commit Index should be 6, as the comands have been replicated to other nodes");

        //    snapshotBuilt.IsConditionMatched.Should().BeTrue();

        //    info.HasSnapshot.Should().BeTrue();

        //    info.Detail.LastIncludedTerm.Should().Be(captureAfterCommands.CurrentTerm, "Term should be the same, as it has not changed");

        //    info.Detail.LastIncludedIndex.Should().Be(threshold, "The snapshot should encompass the appropriate index");

        //    (await Context.GetService<IPersistentProperties>().FetchLogEntryIndexPreviousToIndex(info.Detail.LastIncludedIndex))
        //        .Should()
        //        .Be(0, "As the Snapshot shohuld be the second log entry in the log chain, and sit next to the None entry from initialization");

        //    Cleanup();
        //    #endregion
        //}


        [Fact]
        [Order(2)]
        public async Task IsLeaderHandlingConfigurationChangeAndSendingSnapshot()
        {
            #region Arrange

            var notifiableQueue = CaptureActivities();

            var configurationChanged = notifiableQueue
                .AttachNotifier(x =>
                    x.Is(OnConfigurationChangeRequestReceive.ActionName, OnConfigurationChangeRequestReceive.ConfigurationChangeSuccessful)).RemoveOnceMatched();

            var nodesCaughtUp = notifiableQueue
                .AttachNotifier(x =>
                    x.Is(OnCatchUpOfNewlyAddedNodes.ActionName, OnCatchUpOfNewlyAddedNodes.NodesCaughtUp)).RemoveOnceMatched();

            var decommissioning = notifiableQueue
                .AttachNotifier(x =>
                    x.Is(AbstractState.Entity, AbstractState.Decommissioning)).RemoveOnceMatched();

            var currentConfiguration = Context.GetService<IClusterConfiguration>().CurrentConfiguration;

            var newConfiguration = currentConfiguration.Select(x => new NodeConfiguration
            {
                UniqueNodeId = x.UniqueNodeId

            }).Append(new NodeConfiguration
            {
                UniqueNodeId = MockNewNodeC //Adding NewNodeC

            }).Where(x=> !x.UniqueNodeId.Equals(SUT)) //Removing SUT (Which is leader)
            .ToList();

            CreateMockNode(MockNewNodeC);

            #endregion

            #region Act

            Exception caughtException = null;
            ConfigurationChangeResult changeResult = null;
            StateCapture captureBeforeConfigurationChange = null, captureAfterConfigurationChange = null, captureAfterDecommission = null;

            try
            {
                captureBeforeConfigurationChange = new StateCapture(Context.GetService<ICurrentStateAccessor>().Get());

                /// <remarks>
                /// Enqueuing Responses for old nodes
                /// </remarks>
                EnqueueAppendEntriesSuccessResponse(MockNodeA); //For Replication Approval of C-old,new
                EnqueueAppendEntriesSuccessResponse(MockNodeB); //For Replication Approval of C-old,new

                EnqueueAppendEntriesSuccessResponse(MockNodeA); //For Replication Approval of C-new
                EnqueueAppendEntriesSuccessResponse(MockNodeB); //For Replication Approval of C-new

                EnqueueAppendEntriesSuccessResponse(MockNodeA); //For any heartbeats being sent for the C-new entry to be replicated and then applying cluster configuration
                EnqueueAppendEntriesSuccessResponse(MockNodeB); //For any heartbeats being sent for the C-new entry to be replicated and then applying cluster configuration

                // New Node may respond with Success false, as it has just started up and needs replication
                Context.NodeContext.GetMockNode(MockNewNodeC).EnqueueNextAppendEntriesResponse(rpc => new AppendEntriesRPCResponse
                {
                    Term = rpc.Term,
                    FirstIndexOfConflictingEntryTermOnFailure = rpc.PreviousLogIndex,
                    ConflictingEntryTermOnFailure = rpc.PreviousLogTerm,
                    Success = false
                }, approveImmediately: true);

                EnqueueAppendEntriesSuccessResponse(MockNewNodeC); //Enqueue Success AppendEntries for the remaining entries
                EnqueueAppendEntriesSuccessResponse(MockNewNodeC); //For any heartbeats being sent for the C-new entry to be replicated and then applying cluster configuration

                changeResult = await Context.GetService<IExternalConfigurationChangeHandler>().IssueConfigurationChange(new ConfigurationChangeRPC
                {
                    UniqueId = Guid.NewGuid().ToString(),
                    NewConfiguration = newConfiguration

                }, CancellationToken.None);

                nodesCaughtUp.Wait(EventNotificationTimeOut);

                configurationChanged.Wait(EventNotificationTimeOut);

                captureAfterConfigurationChange = new StateCapture(Context.GetService<ICurrentStateAccessor>().Get());

                decommissioning.Wait(EventNotificationTimeOut);

                captureAfterDecommission = new StateCapture(Context.GetService<ICurrentStateAccessor>().Get());
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            #endregion

            #region Assert

            var assertableQueue = StartAssertions(notifiableQueue);

            caughtException
                .Should().Be(null, $" ");

            captureBeforeConfigurationChange
                .StateValue
                .Should().Be(StateValues.Leader, "As the leader state is has Leader value");

            var lastIndex = await Context.GetService<IPersistentProperties>().GetLastIndex();

            captureAfterConfigurationChange
                .StateValue
                .Should().Be(StateValues.Leader, "Since the leader doesn't decommission immediately after appending the C-new entry, the state is still Leader");

            captureAfterConfigurationChange
                .LastLogIndex
                .Should().Be(lastIndex, "As the C-new entry must have been appended to the logs");

            captureAfterConfigurationChange
                .CommitIndex
                .Should().Be(lastIndex - 1, "C-new entry is present in the logs, however, the replication to other nodes has not yet happened yet, and therefore, the commit index would point to the C-old,new entry being committed");

            captureAfterDecommission
                .CommitIndex
                .Should().Be(lastIndex, "The conf C-new entry should have been replicated to other nodes and also comitted");

            captureAfterDecommission
                .LastApplied
                .Should().Be(lastIndex, "The conf C-new entry should have been replicated to other nodes and also comitted");

            captureAfterDecommission
                .MatchIndexes
                .Values
                .Should()
                .Match((i) => i.All(_ => _.Equals(lastIndex)),
                    $"{MockNodeA} and {MockNodeB} and {MockNewNodeC} should have had replicated all entries up until the leader's last log index");

            captureAfterDecommission
                .NextIndexes
                .Values
                .Should()
                .Match((i) => i.All(_ => _.Equals(lastIndex + 1)),
                    $"{MockNodeA} and {MockNodeB} and {MockNewNodeC} should have had replicated all entries up until the leader's last log index, and the nextIndex to send for each peer mock node should be one greater");

            captureAfterDecommission
                .StateValue
                .Should().Be(StateValues.Abandoned, "As the leader state is now decommissioned");

            Cleanup();
            #endregion
        }
    }
}