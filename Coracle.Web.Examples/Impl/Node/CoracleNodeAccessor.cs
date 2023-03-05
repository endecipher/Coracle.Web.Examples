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

using Coracle.Raft.Engine.Node;
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
