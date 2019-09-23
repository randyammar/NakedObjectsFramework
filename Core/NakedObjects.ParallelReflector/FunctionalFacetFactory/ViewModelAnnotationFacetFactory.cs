// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Common.Logging.Configuration;
using NakedFunctions;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.Utils;

namespace NakedObjects.ParallelReflect.FacetFactory {
    public sealed class ViewModelAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        public ViewModelAnnotationFacetFactory(int numericOrder) : base(numericOrder, FeatureType.ObjectsAndInterfaces, ReflectionType.Functional) { }

        private static bool IsSameType(ParameterInfo pi, Type toMatch)
        {
            return pi != null &&
                   pi.ParameterType == toMatch;
        }

        private static bool IsSameTypeAndReturnType(MethodInfo mi, Type toMatch) {
            var pi = mi.GetParameters().FirstOrDefault();

            return pi != null &&
                   pi.ParameterType == toMatch &&
                   mi.ReturnType == toMatch;

        }

        private MethodInfo GetDeriveMethod(Type type) {
            return FunctionalIntrospector.Functions.
                SelectMany(t => t.GetMethods()).
                Where(m => m.Name == "DeriveKeys").
                SingleOrDefault(m => IsSameType(m.GetParameters().FirstOrDefault(), type));

        }

        private MethodInfo GetPopulateMethod(Type type)
        {
            return FunctionalIntrospector.Functions.
                SelectMany(t => t.GetMethods()).
                Where(m => m.Name == "PopulateUsingKeys").
                SingleOrDefault(m => IsSameTypeAndReturnType(m, type));
        }

        private MethodInfo GetIsEditMethod(Type type)
        {
            return FunctionalIntrospector.Functions.
                SelectMany(t => t.GetMethods()).
                Where(m => m.Name == "IsEditView").
                SingleOrDefault(m => IsSameType(m.GetParameters().FirstOrDefault(), type));
        }

        public override IImmutableDictionary<string, ITypeSpecBuilder> Process(IReflector reflector, Type type, IMethodRemover methodRemover, ISpecificationBuilder specification, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            IFacet facet = null;

            if (type.GetCustomAttribute<ViewModelAttribute>() != null ||
                type.GetCustomAttribute<ViewModelEditAttribute>() != null) {
                MethodInfo deriveMethod = GetDeriveMethod(type);
                MethodInfo populateMethod = GetPopulateMethod(type);
                MethodInfo isEditMethod = GetIsEditMethod(type);

                if (deriveMethod != null && populateMethod != null) {
                    if (type.GetCustomAttribute<ViewModelEditAttribute>() != null) {
                        facet = new ViewModelEditFacetViaFunctionsConvention(specification, deriveMethod, populateMethod);
                    }
                    else if (isEditMethod != null) {
                        facet = new ViewModelSwitchableFacetViaFunctionsConvention(specification, deriveMethod, populateMethod, isEditMethod);
                    }
                    else {
                        facet = new ViewModelFacetViaFunctionsConvention(specification, deriveMethod, populateMethod);
                    }
                }
                else {
                    // log 
                }
            }

            FacetUtils.AddFacet(facet);

            return metamodel;
        }
    }
}