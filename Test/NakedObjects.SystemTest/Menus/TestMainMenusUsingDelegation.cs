﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture.Configuration;
using NakedObjects.Architecture.Menu;
using NakedObjects.Core.Configuration;
using NakedObjects.Menu;
using NakedObjects.Meta.Menu;
using NakedObjects.Xat;
using TestObjectMenu;
using Microsoft.Practices.Unity;


namespace NakedObjects.SystemTest.Menus.Service {
    [TestClass]
    public class TestMainMenusUsingDelegation : AbstractSystemTest<MenusDbContext> {
        [TestMethod]
        public virtual void TestMainMenus() {
            var menus = AllMainMenus();

            menus[0].AssertNameEquals("Foo Service");
            menus[1].AssertNameEquals("Bars"); //Picks up Named attribute on service
            menus[2].AssertNameEquals("Subs"); //Named attribute overridden in menu construction

            var foo = menus[0];
            foo.AssertItemCountIs(3);
            Assert.AreEqual(3, foo.AllItems().OfType<ITestMenuItem>().Count());

            foo.AllItems()[0].AssertNameEquals("Foo Action0");
            foo.AllItems()[1].AssertNameEquals("Foo Action1");
            foo.AllItems()[2].AssertNameEquals("Foo Action2");
        }

        #region Setup/Teardown

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc) {
            Database.Delete(MenusDbContext.DatabaseName);
            var context = Activator.CreateInstance<MenusDbContext>();

            context.Database.Create();
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            CleanupNakedObjectsFramework(new TestMainMenusUsingDelegation());
        }

        [TestInitialize()]
        public void TestInitialize() {
            InitializeNakedObjectsFrameworkOnce();
            StartTest();
        }

        [TestCleanup()]
        public void TestCleanup() {}

        #endregion

        #region System Config

        protected override object[] SystemServices {
            get {
                return new object[] {
                    new FooService(),
                    new ServiceWithSubMenus(),
                    new BarService(),
                    new QuxService()
                };
            }
        }

        protected override void RegisterTypes(IUnityContainer container) {
            base.RegisterTypes(container);
            container.RegisterType<IMenuFactory, MenuFactory>();
            container.RegisterInstance<IReflectorConfiguration>(MyReflectorConfig(), (new ContainerControlledLifetimeManager()));
        }

        private IReflectorConfiguration MyReflectorConfig() {
            return new ReflectorConfiguration(
                this.Types ?? new Type[] {},
                this.Services,
                Types.Select(t => t.Namespace).Distinct().ToArray(),
                LocalMainMenus.MainMenus);
        }

        #endregion
    }

    #region Classes used in test

    public class LocalMainMenus {
        public static IMenu[] MainMenus(IMenuFactory factory) {
            var menuDefs = new Dictionary<Type, Action<IMenu>>();
            menuDefs.Add(typeof (FooService), FooService.Menu);
            menuDefs.Add(typeof (BarService), BarService.Menu);
            menuDefs.Add(typeof (ServiceWithSubMenus), ServiceWithSubMenus.Menu);

            var menus = new List<IMenu>();
            foreach (var menuDef in menuDefs) {
                var menu = factory.NewMenu(menuDef.Key);
                menuDef.Value(menu);
                menus.Add(menu);
            }
            return menus.ToArray();
        }
    }

    public class FooService {
        public static void Menu(IMenu menu) {
            menu.Type = typeof (FooService);
            menu.AddRemainingNativeActions();
        }

        public void FooAction0() {}

        public void FooAction1() {}

        public void FooAction2(string p1, int p2) {}
    }

    [Named("Subs")]
    public class ServiceWithSubMenus {
        public static void Menu(IMenu menu) {
            menu.Type = typeof (ServiceWithSubMenus);
            var sub1 = menu.CreateSubMenu("Sub1");
            sub1.AddAction("Action1");
            sub1.AddAction("Action3");
            var sub2 = menu.CreateSubMenu("Sub2");
            sub2.AddAction("Action2");
            sub2.AddAction("Action0");
        }

        public void Action0() {}

        public void Action1() {}

        public void Action2() {}

        public void Action3() {}
    }

    [Named("Bars")]
    public class BarService {
        public static void Menu(IMenu menu) {
            menu.Type = typeof (BarService);
            menu.AddRemainingNativeActions();
        }

        [MemberOrder(10)]
        public void BarAction0() {}

        [MemberOrder(1)]
        public void BarAction1() {}

        public void BarAction2() {}

        public void BarAction3() {}
    }

    #endregion
}