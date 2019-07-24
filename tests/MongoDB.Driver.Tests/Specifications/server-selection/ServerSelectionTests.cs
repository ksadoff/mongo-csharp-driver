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
using MongoDB.Driver.TestHelpers;
using Xunit;

namespace MongoDB.Driver.Tests
{
    public class ServerSelectionTest
    {
        [Theory]
        [ParameterAttributeData]
        public void ReadPreference_should_not_be_sent_to_standalone_server(
            [Values(false, true)] bool async)
        {
            var eventCapturer = new EventCapturer().Capture<CommandStartedEvent>(e => e.CommandName.Equals("find"));
            using (var subject = CreateDisposableClient(eventCapturer, ReadPreference.Secondary))
            {
                var database = subject.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
                var collection = database.GetCollection<BsonDocument>(DriverTestConfiguration.CollectionNamespace.CollectionName);

                if (async)
                {
                    collection.FindAsync("{ x : 2 }").GetAwaiter().GetResult();
                }
                else
                {
                    collection.FindSync("{ x : 2 }");
                }

                var resultCommand = ((CommandStartedEvent)eventCapturer.Events[0]).Command;

                if (subject.Cluster.Description.Type == ClusterType.Standalone)
                {
                    resultCommand.Contains("$readPreference").Should().BeFalse();
                }
                else
                {
                    resultCommand.Contains("$readPreference").Should().BeTrue();
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