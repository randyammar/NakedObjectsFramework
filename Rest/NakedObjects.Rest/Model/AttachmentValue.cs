// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using NakedObjects.Facade;
using NakedObjects.Rest.Snapshot.Utility;

namespace NakedObjects.Rest.Model {
    public class AttachmentValue : IValue {
        private readonly byte[] internalValue;

        public AttachmentValue(byte[] value) {
            internalValue = value;
        }

        #region IValue Members

        public object GetValue(IFrameworkFacade facade, UriMtHelper helper, IOidStrategy oidStrategy) {
            return internalValue;
        }

        #endregion
    }
}