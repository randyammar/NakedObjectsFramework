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
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Core.Util;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.Utils;
using NakedObjects.Util;

namespace NakedObjects.ParallelReflect.FacetFactory {
    /// <summary>
    ///     Sets up all the <see cref="IFacet" />s for an action in a single shot
    /// </summary>
    public sealed class FunctionsFacetFactory : MethodPrefixBasedFacetFactoryAbstract, IMethodIdentifyingFacetFactory {
        private static readonly string[] FixedPrefixes = { };

        private static readonly ILog Log = LogManager.GetLogger(typeof(ActionMethodsFacetFactory));

        public FunctionsFacetFactory(int numericOrder)
            : base(numericOrder, FeatureType.ActionsAndActionParameters, ReflectionType.Functional) { }

        public override string[] Prefixes => FixedPrefixes;

        #region IMethodIdentifyingFacetFactory Members

        public override IImmutableDictionary<string, ITypeSpecBuilder> Process(IReflector reflector, MethodInfo actionMethod, IMethodRemover methodRemover, ISpecificationBuilder action, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            string capitalizedName = NameUtils.CapitalizeName(actionMethod.Name);

            Type type = actionMethod.DeclaringType;
            var facets = new List<IFacet>();
            var result = reflector.LoadSpecification(type, metamodel);
            metamodel = result.Item2;
            ITypeSpecBuilder onType = result.Item1;
            result = reflector.LoadSpecification(actionMethod.ReturnType, metamodel);
            metamodel = result.Item2;
            var returnSpec = result.Item1 as IObjectSpecBuilder;

            IObjectSpecImmutable elementSpec = null;
            bool isQueryable = IsQueryOnly(actionMethod) || CollectionUtils.IsQueryable(actionMethod.ReturnType);
            if (returnSpec != null && IsCollection(actionMethod.ReturnType)) {
                Type elementType = CollectionUtils.ElementType(actionMethod.ReturnType);
                result = reflector.LoadSpecification(elementType, metamodel);
                metamodel = result.Item2;
                elementSpec = result.Item1 as IObjectSpecImmutable;
            }

            RemoveMethod(methodRemover, actionMethod);
            facets.Add(new ActionInvocationFacetViaMethod(actionMethod, onType, returnSpec, elementSpec, action, isQueryable));

            DefaultNamedFacet(facets, actionMethod.Name, action); // must be called after the checkForXxxPrefix methods

            AddHideForSessionFacetNone(facets, action);
            AddDisableForSessionFacetNone(facets, action);

            FacetUtils.AddFacets(facets);

            return metamodel;
        }

        public override IImmutableDictionary<string, ITypeSpecBuilder> ProcessParams(IReflector reflector, MethodInfo method, int paramNum, ISpecificationBuilder holder, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            ParameterInfo parameter = method.GetParameters()[paramNum];
            var facets = new List<IFacet>();

            if (parameter.ParameterType.IsGenericType && (parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                facets.Add(new NullableFacetAlways(holder));
            }

            var result = reflector.LoadSpecification(parameter.ParameterType, metamodel);
            metamodel = result.Item2;
            var returnSpec = result.Item1 as IObjectSpecBuilder;

            if (returnSpec != null && IsParameterCollection(parameter.ParameterType)) {
                Type elementType = CollectionUtils.ElementType(parameter.ParameterType);
                result = reflector.LoadSpecification(elementType, metamodel);
                metamodel = result.Item2;
                var elementSpec = result.Item1 as IObjectSpecImmutable;
                facets.Add(new ElementTypeFacet(holder, elementType, elementSpec));
            }

            FacetUtils.AddFacets(facets);
            return metamodel;
        }

        public IList<MethodInfo> FindActions(IList<MethodInfo> candidates, IClassStrategy classStrategy) {
            return candidates.Where(methodInfo => methodInfo.GetCustomAttribute<NakedObjectsIgnoreAttribute>() == null &&
                                                  methodInfo.IsStatic &&
                                                  !methodInfo.IsGenericMethod &&
                                                  classStrategy.IsTypeToBeIntrospected(methodInfo.ReturnType) &&
                                                  ParametersAreSupported(methodInfo, classStrategy)).ToArray();
        }

        #endregion

        private bool IsQueryOnly(MethodInfo method) {
            return (method.GetCustomAttribute<IdempotentAttribute>() == null) &&
                   (method.GetCustomAttribute<QueryOnlyAttribute>() != null);
        }

        // separate methods to reproduce old reflector behaviour
        private bool IsParameterCollection(Type type) {
            return type != null && (
                       CollectionUtils.IsGenericEnumerable(type) ||
                       type.IsArray ||
                       type == typeof(string) ||
                       CollectionUtils.IsCollectionButNotArray(type));
        }

        private bool IsCollection(Type type) {
            return type != null && (
                       CollectionUtils.IsGenericEnumerable(type) ||
                       type.IsArray ||
                       type == typeof(string) ||
                       CollectionUtils.IsCollectionButNotArray(type) ||
                       IsCollection(type.BaseType) ||
                       type.GetInterfaces().Where(i => i.IsPublic).Any(IsCollection));
        }

        private bool ParametersAreSupported(MethodInfo method, IClassStrategy classStrategy) {
            foreach (ParameterInfo parameterInfo in method.GetParameters()) {
                if (!classStrategy.IsTypeToBeIntrospected(parameterInfo.ParameterType)) {
                    // log if not a System or NOF type
                    if (!TypeUtils.IsSystem(method.DeclaringType) && !TypeUtils.IsNakedObjects(method.DeclaringType)) {
                        Log.WarnFormat("Ignoring method: {0}.{1} because parameter '{2}' is of type {3}", method.DeclaringType, method.Name, parameterInfo.Name, parameterInfo.ParameterType);
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Must be called after the <c>CheckForXxxPrefix</c> methods.
        /// </summary>
        private static void DefaultNamedFacet(ICollection<IFacet> actionFacets, string name, ISpecification action) {
            actionFacets.Add(new NamedFacetInferred(name, action));
        }
    }
}