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

using Coracle.Raft.Engine.Configuration.Cluster;
using Coracle.Raft.Engine.Discovery;
using Coracle.Raft.Examples.Registrar;
using Microsoft.AspNetCore.Mvc;

namespace Coracle.Web.Discovery.Controllers
{
    public class DiscoveryController : Controller
    {
        public DiscoveryController(INodeRegistrar nodeRegistrar)
        {
            NodeRegistrar = nodeRegistrar;
        }

        public INodeRegistrar NodeRegistrar { get; }

        [HttpPost(Name = nameof(Enroll))]
        public async Task<DiscoveryResult> Enroll()
        {
            var obj = await HttpContext.Request.ReadFromJsonAsync<NodeConfiguration>(HttpContext.RequestAborted);

            return await NodeRegistrar.Enroll(obj, HttpContext.RequestAborted);
        }

        [HttpGet(Name = nameof(Get))]
        public async Task<DiscoveryResult> Get()
        {
            return await NodeRegistrar.GetAllNodes(HttpContext.RequestAborted);
        }

        [HttpGet(Name = nameof(Clear))]
        public async Task<DiscoveryResult> Clear()
        {
            await NodeRegistrar.Clear();
            
            return new DiscoveryResult
            {
                IsSuccessful = true,
            };
        }
    }
}
