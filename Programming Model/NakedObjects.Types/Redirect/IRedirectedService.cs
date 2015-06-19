﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;

namespace NakedObjects.Redirect {
    //Implemented by a 'stub' class that acts as proxy to a service implemented on another server
    //Note that, unlike IRedirectedObject, this defines functions, not properties,
    [Obsolete("Use IRedirected")]
    public interface IRedirectedService {
        //This should be a logical server name, translated to/from a physical address elsewhere.
        string ServerName();
        //The name of the service on the other server
        string ServiceName();
    }
}