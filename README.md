# Coracle.Web.Examples
An example use-case of [Coracle.Raft](https://github.com/endecipher/Coracle.Raft) - the completely extensible implementation of the Raft consensus algorithm in .NET

![GitHub](https://img.shields.io/github/license/endecipher/Coracle.Web.Examples)
[![Dependency](https://badgen.net/badge/uses/Coracle.Raft.Examples/purple?icon=nuget)](https://www.nuget.org/packages/Coracle.Raft.Examples/)


<img src="https://github.com/endecipher/Coracle.Web.Examples/blob/main/Images/Coracle.Web.Examples%20-%20Workflow.gif" width="60%" height="60%" />

## Features

[Coracle.Raft.Examples](https://github.com/endecipher/Coracle.Raft/tree/main/Coracle.Raft.Examples)  
- [![NuGet version (Coracle.Raft.Examples)](https://img.shields.io/nuget/v/Coracle.Raft.Examples.svg?color=orange)](https://www.nuget.org/packages/Coracle.Raft.Examples/)
- `Coracle.Raft.Engine.Command.ICommand` and `Coracle.Raft.Engine.Command.IStateMachineHandler` are implemented for keeping track of "notes" or any string key-value pairs
- Namespace `Coracle.Raft.Examples.Registrar` exposes interfaces for service discovery and storage of enrolled nodes
- `Coracle.Raft.Engine.Command.IPersistentStateHandler` is implemented for all data operations. For understanding purposes, an in-memory extension was adopted. All expected operations are covered - inclusive of snapshot management, compaction, handling the replicated log chain etc

Additionally, the projects [Coracle.Web.Examples and Coracle.Web.Examples.Discovery](https://github.com/endecipher/Coracle.Web.Examples) implement the following  
- `Coracle.Raft.Engine.Remoting.IOutboundRequestHandler` is extended to use HTTP Web APIs for cross-node communication.
- `Coracle.Raft.Engine.Discovery.IDiscoveryHandler` is extended to use a discovery server for initial cluster identification.

## Documentation
- To know more about how Coracle.Raft is used here, check out this article - [Extensible Raft Consensus algorithm in .NET](https://medium.com/@ayan.choudhury329/extensible-raft-consensus-algorithm-in-net-1db4ba13efa2)


