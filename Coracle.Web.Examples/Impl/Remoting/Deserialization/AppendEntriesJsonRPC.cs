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
using Coracle.Raft.Engine.Logs;
using Coracle.Raft.Engine.Remoting.RPC;
using Coracle.Raft.Examples.ClientHandling;
using Coracle.Raft.Examples.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coracle.Web.Impl.Remoting.Deserialization
{
    [JsonConverter(typeof(AppendEntriesConverter))]
    public class AppendEntriesJsonRPC : AppendEntriesRPC
    {

    }

    class AppendEntriesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var obj = JObject.Load(reader);
            AppendEntriesJsonRPC item = new AppendEntriesJsonRPC();

            var objProperty = obj.GetValue(nameof(AppendEntriesRPC.Entries), StringComparison.OrdinalIgnoreCase);

            using (var subReader = obj.CreateReader())
            {
                serializer.Populate(subReader, item);
            }

            var entries = new List<LogEntry>();

            if (objProperty != null)
            {
                foreach (var child in objProperty.Children())
                {
                    var entry = new LogEntry();
                    entries.Add(entry);

                    using (var childReader = child.CreateReader())
                    {
                        serializer.Populate(childReader, entry);
                    }

                    var data = child.SelectToken(nameof(LogEntry.Content).ToLower());

                    switch (entry.Type)
                    {
                        case LogEntry.Types.None:
                        case LogEntry.Types.NoOperation:
                            entry.Content = null;
                            break;
                        case LogEntry.Types.Command:
                            entry.Content = data.ToObject<NoteCommand>();
                            break;
                        case LogEntry.Types.Configuration:
                            entry.Content = data.ToObject<NodeConfiguration[]>();
                            break;
                        case LogEntry.Types.Snapshot:
                            entry.Content = data.ToObject<SnapshotHeader>();
                            break;
                        default:
                            throw new InvalidCastException($"{entry.Type} is not supported for {nameof(AppendEntriesJsonRPC)} conversion");
                    }
                }
            }

            item.Entries = entries;
            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

        }
    }
}
