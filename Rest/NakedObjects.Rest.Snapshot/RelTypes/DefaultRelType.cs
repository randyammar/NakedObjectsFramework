// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using NakedObjects.Facade;
using NakedObjects.Rest.Snapshot.Constants;

namespace NakedObjects.Rest.Snapshot.Utility {
    public class DefaultRelType : ObjectRelType {
        private readonly string actionId;
        private readonly string paramId;
        private readonly IActionParameterFacade parameter;
        private DefaultRelType(UriMtHelper helper) : base(RelValues.Default, helper) {}

        public DefaultRelType(IActionParameterFacade parameter, UriMtHelper helper)
            : this(helper) {
            this.parameter = parameter;
        }

        public DefaultRelType(string actionId, string paramId, UriMtHelper helper)
            : this(helper) {
            this.actionId = actionId;
            this.paramId = paramId;
        }

        public override string Name {
            get { return base.Name + (parameter == null ? helper.GetRelParametersFor(actionId, paramId) : helper.GetRelParametersFor(parameter)); }
        }
    }
}