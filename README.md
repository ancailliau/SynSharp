# SynSharp

A C# client for [Vertex Synapse](https://github.com/vertexproject/synapse). 

Currently, only a limited subset of node forms and types is supported. See
[https://github.com/ancailliau/SynSharp/tree/master/Synsharp/Forms](https://github.com/ancailliau/SynSharp/tree/master/Synsharp/Forms)
and 
[https://github.com/ancailliau/SynSharp/tree/master/Synsharp/Types](https://github.com/ancailliau/SynSharp/tree/master/Synsharp/Types)
for the complete up-to-date list.

## Examples

Connect to a server

	SynapseClient = new SynapseClient("https://localhost:8901");
	await SynapseClient.LoginAsync("root", "secret");

Executes a Storm query to retreive all IPv6 addresses.

	var response = await SynapseClient.StormAsync<InetIpV6>("inet:ipv6").ToListAsync();
	foreach (var item in response) {
		Console.WriteLine(item);
	}

To add a new IPv6 addresses.

    var response = await SynapseClient
                .Nodes.Add(InetIpV6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334"));

## Running the tests

Running the tests requires Docker, but can be run with the following command:

	 dotnet test
     
To run a specific test with the detailed output:

     dotnet test -l "console;verbosity=detailed" --filter 'TestGetIPv6'

## Licence

> Copyright 2022 Antoine Cailliau
> 
> Licensed under the Apache License, Version 2.0 (the "License");
> you may not use this file except in compliance with the License.
> You may obtain a copy of the License at
> 
>    http://www.apache.org/licenses/LICENSE-2.0
> 
> Unless required by applicable law or agreed to in writing, software
> distributed under the License is distributed on an "AS IS" BASIS,
> WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
> See the License for the specific language governing permissions and
> limitations under the License.
 