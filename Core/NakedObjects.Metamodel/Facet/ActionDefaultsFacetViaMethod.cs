// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Util;

[assembly:InternalsVisibleTo("NakedObjects.ParallelReflector.Test")]
[assembly: InternalsVisibleTo("NakedObjects.Reflector.Test")]

namespace NakedObjects.Meta.Facet {
    [Serializable]
    public sealed class ActionDefaultsFacetViaMethod : ActionDefaultsFacetAbstract, IImperativeFacet {
        private readonly MethodInfo method;

        public ActionDefaultsFacetViaMethod(MethodInfo method, ISpecification holder)
            : base(holder) {
            this.method = method;
            MethodDelegate = DelegateUtils.CreateDelegate(method);
        }

        // for testing only 
        [field: NonSerialized]
        internal Func<object, object[], object> MethodDelegate { get; private set; }

        #region IImperativeFacet Members

        public MethodInfo GetMethod() {
            return method;
        }

        public Func<object, object[], object> GetMethodDelegate() {
            return MethodDelegate;
        }

        #endregion

        public override Tuple<object, TypeOfDefaultValue> GetDefault(INakedObjectAdapter nakedObjectAdapter) {
            // type safety is given by the reflector only identifying methods that match the 
            // parameter type
            var defaultValue = MethodDelegate(nakedObjectAdapter.GetDomainObject(), new object[] { });
            return new Tuple<object, TypeOfDefaultValue>(defaultValue, TypeOfDefaultValue.Explicit);
        }

        protected override string ToStringValues() {
            return "method=" + method;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            MethodDelegate = DelegateUtils.CreateDelegate(method);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}