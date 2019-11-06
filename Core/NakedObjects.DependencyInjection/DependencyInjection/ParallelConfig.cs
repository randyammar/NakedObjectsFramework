﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using Microsoft.Extensions.DependencyInjection;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Menu;
using NakedObjects.Core.Authentication;
using NakedObjects.Core.Component;
using NakedObjects.Core.Spec;
using NakedObjects.Meta.Component;
using NakedObjects.Meta.Menu;
using NakedObjects.ParallelReflect;
using NakedObjects.ParallelReflect.Component;
using NakedObjects.Persistor.Entity.Component;
using NakedObjects.Service;

namespace NakedObjects.DependencyInjection {
    public static class ParallelConfig {
        public static void RegisterCoreSingletonTypes(IServiceCollection services) {
            services.AddSingleton<DefaultClassStrategy, DefaultClassStrategy>();
            services.AddSingleton<IClassStrategy, CachingClassStrategy>();
            services.AddSingleton<ISpecificationCache, ImmutableInMemorySpecCache>();
            services.AddSingleton<IReflector, ParallelReflector>();
            services.AddSingleton<IMetamodel, Metamodel>();
            services.AddSingleton<IMetamodelBuilder, Metamodel>();
            services.AddSingleton<IMenuFactory, MenuFactory>();
        }

        public static void RegisterCoreScopedTypes(IServiceCollection services) {
            services.AddScoped<INakedObjectAdapterMap, NakedObjectAdapterHashMap>();
            services.AddScoped<IIdentityAdapterMap, IdentityAdapterHashMap>();
            services.AddScoped<IDomainObjectInjector, DomainObjectContainerInjector>();
            services.AddScoped<IOidGenerator, EntityOidGenerator>();
            services.AddScoped<IPersistAlgorithm, FlatPersistAlgorithm>();
            services.AddScoped<IObjectStore, EntityObjectStore>();
            services.AddScoped<IIdentityMap, IdentityMapImpl>();
            services.AddScoped<ITransactionManager, TransactionManager>();
            services.AddScoped<INakedObjectManager, NakedObjectManager>();
            services.AddScoped<IObjectPersistor, ObjectPersistor>();
            services.AddScoped<IServicesManager, ServicesManager>();
            services.AddScoped<ILifecycleManager, LifeCycleManager>();
            services.AddScoped<IMetamodelManager, MetamodelManager>();
            services.AddScoped<IMessageBroker, MessageBroker>();
            services.AddScoped<INakedObjectsFramework, NakedObjectsFramework>();
            services.AddScoped<ISession, WindowsSession>();
            //services.AddScoped<IFrameworkResolver, UnityFrameworkResolver>();

            //Temporary scaffolding
            services.AddScoped<NakedObjectFactory, NakedObjectFactory>();
            services.AddScoped<SpecFactory, SpecFactory>();
        }

        public static void RegisterStandardFacetFactories(IServiceCollection services) {
            var factoryTypes = FacetFactories.StandardFacetFactories();
            for (int i = 0; i < factoryTypes.Length; i++) {
                ConfigHelpers.RegisterFacetFactory(factoryTypes[i], services, i);
            }
        }
    }
}