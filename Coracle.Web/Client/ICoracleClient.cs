﻿using Coracle.Raft.Engine.Configuration.Alterations;
using Coracle.Raft.Examples.ClientHandling;

namespace Coracle.Web.Client
{
    public interface ICoracleClient
    {
        Task<string> ExecuteCommand(NoteCommand command, CancellationToken token);
        Task<string> ChangeConfiguration(ConfigurationChangeRequest changeRPC, CancellationToken token);
    }
}
