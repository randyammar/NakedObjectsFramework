﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Meta.Facet;

namespace NakedObjects.Reflect.FacetFactory {
    public class UnsupportedParameterTypesMethodFilteringFactory : FacetFactoryAbstract, IMethodFilteringFacetFactory {
        private static readonly ILog Log = LogManager.GetLogger(typeof (UnsupportedParameterTypesMethodFilteringFactory));

        public UnsupportedParameterTypesMethodFilteringFactory(IReflector reflector)
            : base(reflector, FeatureType.Action) {}

        #region IMethodFilteringFacetFactory Members

        public bool Filters(MethodInfo method) {
            var typeName = method.DeclaringType == null ? "Unknown" : method.DeclaringType.FullName;

            if (method.IsGenericMethod) {
                Log.InfoFormat("Ignoring method: {0}.{1} because it is generic", typeName, method.Name);
                return true;
            }

            if (Reflector.ClassStrategy.IsTypeUnsupportedByReflector(method.ReturnType)) {
                Log.InfoFormat("Ignoring method: {0}.{1} because return type is of type {3}", typeName, method.Name, method.ReturnType);
                return true;
            }

            foreach (ParameterInfo parameterInfo in method.GetParameters()) {
                if (Reflector.ClassStrategy.IsTypeUnsupportedByReflector(parameterInfo.ParameterType)) {
                    Log.InfoFormat("Ignoring method: {0}.{1} because parameter '{2}' is of type {3}", typeName, method.Name, parameterInfo.Name, parameterInfo.ParameterType);
                    return true;
                }
            }
            return false;
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}