/*
 * Copyright 2022 Antoine Cailliau
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Synsharp.Response;

/// <summary>
/// Represents a server response to a login request.
/// </summary>
class LoginResponse
{
    [Newtonsoft.Json.JsonProperty("type")] private string Type { get; set; }
    [Newtonsoft.Json.JsonProperty("iden")] private string Iden { get; set; }
    [Newtonsoft.Json.JsonProperty("name")] private string Name { get; set; }
    [Newtonsoft.Json.JsonProperty("admin")] private bool Admin { get; set; }
    [Newtonsoft.Json.JsonProperty("email")] private string Email { get; set; }
    [Newtonsoft.Json.JsonProperty("locked")] private bool Locked { get; set; }
    [Newtonsoft.Json.JsonProperty("archived")] private bool Archived { get; set; }
        
    // TODO AuthGates
    // TODO Rules
    // TODO Roles
}