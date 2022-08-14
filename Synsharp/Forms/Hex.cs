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

namespace Synsharp.Forms;

public class Hex
{
    private string _value;
    private Hex(string value)
    {
        _value = value;
    }

    public static implicit operator string(Hex d) => d._value;
    public static implicit operator Hex(string d) => new Hex(d);

    public override string ToString()
    {
        return _value;
    }

    public static Hex Parse(string s)
    {
        return new Hex(s);
    }
}