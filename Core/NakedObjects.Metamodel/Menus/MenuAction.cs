﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using NakedObjects.Architecture.Menu;
using NakedObjects.Architecture.SpecImmutable;

namespace NakedObjects.Meta.Menus {
    public class MenuAction : IMenuActionImmutable {
        public MenuAction(IActionSpecImmutable actionSpec, string renamedTo = null) {
            this.Action = actionSpec;
            if (renamedTo == null) {
                Name = actionSpec.Identifier.MemberName;
            }
            else {
                Name = renamedTo;
            }
        }

        #region IMenuActionImmutable Members

        public string Name { get; set; }

        public IActionSpecImmutable Action { get; set; }

        #endregion
    }
}