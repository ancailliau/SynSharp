# SynSharp

A C# client for [Vertex Synapse](https://github.com/vertexproject/synapse). 

Currently, only a limited subset of node types is supported:

* CryptoX509CRL
* CryptoX509Cert
* CryptoX509SAN
* FileBytes
* GUID
* HashMD5
* HashSHA1
* HashSHA256
* Hex
* ISOOID
* InetEmail
* InetFqdn
* InetIpV4
* InetIpV6
* InetPasswd
* InetPort
* InetUrl
* InetUser
* RSAKey

## Examples

Connect to a server

	using var loggerFactory = LoggerFactory.Create(builder =>
	{
		builder.SetMinimumLevel(LogLevel.Trace);
		builder.AddConsole(options => options.DisableColors = true);
	});
	var logger = loggerFactory.CreateLogger<SynapseClient>();
	    
	SynapseClient = new SynapseClient("https://localhost:8901", logger);
	await SynapseClient.LoginAsync("root", "secret");

Executes a Storm query to retreive all IPv6 addresses.

	var response = await SynapseClient.StormAsync<InetIpV6>("inet:ipv6").ToListAsync();
	foreach (var item in response) {
		Console.WriteLine(item);
	}

## Running the tests

Running the tests requires Docker, but can be run with the following command:

	 dotnet test

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
 