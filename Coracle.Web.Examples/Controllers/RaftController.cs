#region License
// Copyright (c) 2023 Ayan Choudhury
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using ActivityLogger.Logging;
using Coracle.Raft.Engine.Remoting;
using Coracle.Raft.Engine.Remoting.RPC;
using Coracle.Raft.Examples.Logging;
using Coracle.Web.Impl.Node;
using Coracle.Web.Impl.Remoting.Deserialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Coracle.Web.Constants;

namespace Coracle.Web.Controllers
{
    public class RaftController : Controller
    {
        public const string Raft = nameof(Raft);
        public const string Entity = nameof(RaftController);
        public const string InternalError = nameof(InternalError);
        public const string ControllerError = nameof(ControllerError);
        public const string exception = nameof(exception);


        public RaftController(IRemoteCallExecutor externalRpcHandler, IActivityLogger activityLogger, ICoracleNodeAccessor nodeAccessor)
        {
            ExternalRpcHandler = externalRpcHandler;
            ActivityLogger = activityLogger;
            NodeAccessor = nodeAccessor;
        }

        public IRemoteCallExecutor ExternalRpcHandler { get; }
        public IActivityLogger ActivityLogger { get; }
        public ICoracleNodeAccessor NodeAccessor { get; }

        public async Task<RemoteCallResult<IAppendEntriesRPCResponse>> AppendEntries()
        {
            if (!IsNodeCommunicable<IAppendEntriesRPCResponse>(out var result))
                return result;

            return await Handle(() =>
            {
                HttpContext.Request.EnableBuffering();

                string rpcContent;

                using (var sr = new StreamReader(HttpContext.Request.Body))
                {
                    rpcContent = sr.ReadToEndAsync().GetAwaiter().GetResult();
                }

                var rpc = JsonConvert.DeserializeObject<AppendEntriesJsonRPC>(rpcContent);

                return ExternalRpcHandler.RespondTo(rpc, HttpContext.RequestAborted).GetAwaiter().GetResult();
            });
        }

        public async Task<RemoteCallResult<IInstallSnapshotRPCResponse>> InstallSnapshot()
        {
            if (!IsNodeCommunicable<IInstallSnapshotRPCResponse>(out var result))
                return result;

            return await Handle(() =>
            {
                HttpContext.Request.EnableBuffering();

                string rpcContent;

                using (var sr = new StreamReader(HttpContext.Request.Body))
                {
                    rpcContent = sr.ReadToEndAsync().GetAwaiter().GetResult();
                }

                var rpc = JsonConvert.DeserializeObject<InstallSnapshotJsonRPC>(rpcContent);

                return ExternalRpcHandler.RespondTo(rpc, HttpContext.RequestAborted).GetAwaiter().GetResult();
            });
        }

        public async Task<RemoteCallResult<IRequestVoteRPCResponse>> RequestVote()
        {
            if (!IsNodeCommunicable<IRequestVoteRPCResponse>(out var result))
                return result;

            return await Handle(() =>
            {
                var rpc = HttpContext.Request.ReadFromJsonAsync<RequestVoteRPC>().GetAwaiter().GetResult();

                return ExternalRpcHandler.RespondTo(rpc, HttpContext.RequestAborted).GetAwaiter().GetResult();
            });
        }

        #region RPC Processing

        private bool IsNodeCommunicable<TResponse>(out RemoteCallResult<TResponse> result) where TResponse : IRemoteResponse
        {
            result = null;

            if (!NodeAccessor.CoracleNode.IsInitialized || !NodeAccessor.CoracleNode.IsStarted)
            {
                result = new RemoteCallResult<TResponse>
                {
                    Exception = new Exception(Strings.Errors.NodeNotReady)
                };

                return false;
            }

            return true;
        }

        private async Task<RemoteCallResult<TResponse>> Handle<TResponse>(Func<RemoteCallResult<TResponse>> func) where TResponse : IRemoteResponse
        {
            RemoteCallResult<TResponse> operationResult = new RemoteCallResult<TResponse>();

            try
            {
                operationResult = await Task.Run(func);

                if (operationResult?.Exception != null)
                {
                    ActivityLogger?.Log(new ImplActivity
                    {
                        EntitySubject = Entity,
                        Level = ActivityLogLevel.Error,
                        Event = InternalError,
                    }
                    .With(ActivityParam.New(exception, operationResult.Exception)));
                }
            }
            catch (Exception ex)
            {
                ActivityLogger?.Log(new ImplActivity
                {
                    EntitySubject = Entity,
                    Level = ActivityLogLevel.Error,
                    Event = ControllerError,
                }
                .With(ActivityParam.New(exception, ex)));
            }

            return operationResult;
        }

        #endregion
    }
}
