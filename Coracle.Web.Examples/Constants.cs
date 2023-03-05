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

namespace Coracle.Web
{
    

    public class Constants
    {
        public class Routes
        {
            private const string Raft = nameof(Raft);
            
            public static string ConfigurationChangeEndpoint => Strings.Actions.ConfigurationChange + "/" + nameof(Controllers.ConfigurationChangeController.HandleChange);
            public static string CommandEndpoint => Strings.Actions.Command + "/" + nameof(Controllers.CommandController.HandleCommand);
            public static string AppendEntriesEndpoint => Raft + "/" + nameof(Controllers.RaftController.AppendEntries);
            public static string RequestVoteEndpoint => Raft + "/" + nameof(Controllers.RaftController.RequestVote);
            public static string InstallSnapshotEndpoint => Raft + "/" + nameof(Controllers.RaftController.InstallSnapshot);
        }

        public class Discovery
        {
            public const string Enroll = "discovery/enroll";
            public const string GetCluster = "discovery/get";
        }

        public class Hubs
        {
            public const string Log = "/logHub";
            public const string Raft = "/raftHub";
        }

        public class Strings
        {
            public class Actions
            {
                public const string Command = nameof(Command);
                public const string ConfigurationChange = nameof(ConfigurationChange);
                public const string Successful = $" successfully executed: ";
            }

            public class Errors
            {
                public const string NodeNotReady = $"{nameof(Coracle)} Node not started yet";
            }

            public class Configuration
            {
                public const string Development = nameof(Development);
                public const string EngineConfiguration = "Coracle:EngineConfiguration";
            }
        }
    }
}
