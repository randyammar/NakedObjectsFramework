// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using NakedObjects.Core.Configuration;
using NakedObjects.Persistor.Entity.Configuration;
using RestfulObjects.Test.Data;

namespace RestfulObjects.Test.App {
    public class NakedObjectsSettings {
        private static Type[] Types {
            get { return new Type[] {
                    typeof (EntityCollection<object>), 
                    typeof (ObjectQuery<object>)
            }; }
        }

        private static Type[] MenuServices {
            get {
                return new Type[] {
                    typeof (RestDataRepository),
                    typeof (WithActionService)
                };
            }
        }

        private static Type[] ContributedActions {
            get {
                return new Type[] {
                    typeof (ContributorService)
                };
            }
        }

        private static Type[] SystemServices {
            get {
                return new Type[] {
                    typeof (TestTypeCodeMapper)
                };
            }
        }

        public static ReflectorConfiguration ReflectorConfig() {
            return new ReflectorConfiguration(Types, MenuServices, ContributedActions, SystemServices);
        }

        public static EntityObjectStoreConfiguration EntityObjectStoreConfig() {
            var config = new EntityObjectStoreConfiguration();
            config.UsingCodeFirstContext(() => new CodeFirstContext("RestTest"));
            return config;
        }
    }
}