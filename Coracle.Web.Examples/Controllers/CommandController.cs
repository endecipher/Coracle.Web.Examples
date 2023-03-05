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

using Coracle.Raft.Examples.ClientHandling;
using Coracle.Web.Client;
using Microsoft.AspNetCore.Mvc;

namespace Coracle.Web.Controllers
{
    public class CommandController : Controller
    {

        public CommandController(ICoracleClient coracleClient)
        {
            CoracleClient = coracleClient;
        }

        public ICoracleClient CoracleClient { get; }

        [HttpGet]
        public string Index()
        {
            return nameof(CommandController);
        }

        [HttpPost(Name = nameof(AddNote))]
        public async Task<string> AddNote([FromBody] Note obj, [FromQuery] string tag)
        {
            var command = NoteCommand.CreateAdd(obj);

            var result = await CoracleClient.ExecuteCommand(command, HttpContext.RequestAborted);

            return result;
        }

        [HttpGet(Name = nameof(GetNote))]
        public async Task<string> GetNote([FromQuery] string noteHeader)
        {
            var command = NoteCommand.CreateGet(new Note
            {
                UniqueHeader = noteHeader,
            });

            var result = await CoracleClient.ExecuteCommand(command, HttpContext.RequestAborted);

            return result;
        }

        [HttpPost(Name = nameof(HandleCommand))]
        public async Task<string> HandleCommand()
        {
            var command = await HttpContext.Request.ReadFromJsonAsync<NoteCommand>(HttpContext.RequestAborted);

            var result = await CoracleClient.ExecuteCommand(command, HttpContext.RequestAborted);

            return result;
        }
    }
}
