/* Copyright 2019-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.TestHelpers;
using Xunit;

namespace MongoDB.Driver.Tests.Specifications.server_selection
{
    public class ServerSelectionTest
    {
        [Theory]
        [ParameterAttributeData]
        public void ReadPreference_should_not_be_sent_to_standalone_server(
            [Values(false, true)] bool async)
        {
            var eventCapturer = new EventCapturer().Capture<CommandStartedEvent>(e => e.CommandName.Equals("find") || e.CommandName.Equals("$query"));
            using (var client = CreateDisposableClient(eventCapturer, ReadPreference.PrimaryPreferred))
            {
                var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
                var collection = database.GetCollection<BsonDocument>(DriverTestConfiguration.CollectionNamespace.CollectionName);

                if (async)
                {
                    var _ = collection.FindAsync("{ x : 2 }").GetAwaiter().GetResult();
                }
                else
                {
                    var _ = collection.FindSync("{ x : 2 }");
                }

                BsonDocument sentCommand = ((CommandStartedEvent)eventCapturer.Events[0]).Command;;
                var serverVersion = client.Cluster.Description.Servers[0].Version;

                if (client.Cluster.Description.Type == ClusterType.Standalone)
                {
                    sentCommand.Contains("readPreference").Should().BeFalse();
                }
                else if (client.Cluster.Description.Type == ClusterType.Sharded &&
                         serverVersion < Feature.CommandMessage.FirstSupportedVersion)
                {
                    if (((CommandStartedEvent) eventCapturer.Events[0]).CommandName.Equals("$query"))
                    {
                        sentCommand.Contains("$readPreference").Should().BeTrue();
                    }
                    else if (((CommandStartedEvent) eventCapturer.Events[0]).CommandName.Equals("find"))
                    {
                        sentCommand.Contains("readPreference").Should().BeTrue();
                    }
                }
                else if (serverVersion >= Feature.CommandMessage.FirstSupportedVersion)
                {
                    sentCommand.Contains("$readPreference").Should().BeTrue();
                }
            }
        }

        // private methods
        private DisposableMongoClient CreateDisposableClient(EventCapturer eventCapturer, ReadPreference readPreference)
        {
            return DriverTestConfiguration.CreateDisposableClient((MongoClientSettings settings) =>
            {
                settings.ClusterConfigurator = c => c.Subscribe(eventCapturer);
                settings.ReadPreference = readPreference;
            });
        }
    }
}
