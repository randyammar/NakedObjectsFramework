// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using NakedObjects.Facade;
using NakedObjects.Facade.Contexts;
using NakedObjects.Rest.Snapshot.Representations;
using NakedObjects.Rest.Snapshot.Utility;
using NakedObjects.Rest.Snapshot.Constants;

namespace NakedObjects.Rest.Snapshot.Strategies {
    [DataContract]
    public class PropertyWithDetailsRepresentationStrategy : AbstractPropertyRepresentationStrategy {
        private readonly bool inline;

        public PropertyWithDetailsRepresentationStrategy(bool inline, IOidStrategy oidStrategy, HttpRequestMessage req, PropertyContextFacade propertyContext, RestControlFlags flags) :
            base(oidStrategy, req, propertyContext, flags) {
            this.inline = inline;
        }

        public override bool ShowChoices() {
            return true;
        }

        public override LinkRepresentation[] GetLinks() {
            var links = new List<LinkRepresentation>(GetLinks(inline));

            if (!propertyContext.Target.IsTransient) {
                AddMutatorLinks(links);
            }

            AddPrompt(links, propertyContext.Target.IsTransient ? (Func<LinkRepresentation>) CreatePersistPromptLink : CreatePromptLink);

            return links.ToArray();
        }

        protected override bool AddChoices() {
            return propertyContext.Property.IsChoicesEnabled != Choices.NotEnabled &&
                   (propertyContext.Property.Specification.IsParseable || (propertyContext.Property.Specification.IsCollection && propertyContext.Property.ElementSpecification.IsParseable)) &&
                   !propertyContext.Property.GetChoicesParameters().Any();
        }
    }
}