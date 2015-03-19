﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;

namespace NakedObjects.Mvc.Selenium.Test.Helper {
    public static class SeHelpers {
        #region new helpers

        // Find then click action and wait unitil field selector returns an element 
        public static IWebElement ClickAndWait(this SafeWebDriverWait wait, string actionSelector, string fieldSelector, int delay = 0) {
            IWebElement action = wait.Driver.FindElement(By.CssSelector(actionSelector));
            return wait.ClickAndWait(action, fieldSelector);
        }

        // Click action and wait unitil field selector returns an element 
        public static IWebElement ClickAndWait(this SafeWebDriverWait wait, IWebElement action, string fieldSelector, int delay = 0) {
            if (delay > 0) {
                Thread.Sleep(delay);
            }
            action.Click();
            IWebElement field = null;
            wait.Until(wd => (field = wd.FindElement(By.CssSelector(fieldSelector))) != null);
            Assert.IsNotNull(field);
            return field;
        }


        // Find then click action and wait until field selector stops returning an element 
        public static void ClickAndWaitGone(this SafeWebDriverWait wait, string actionSelector, string fieldSelector, int delay = 0) {
            IWebElement action = wait.Driver.FindElement(By.CssSelector(actionSelector));
            wait.ClickAndWaitGone(action, fieldSelector);
        }

        private static IWebElement SafeFunc(Func<IWebElement> f) {
            try {
                return f();
            }
            catch {
                return null;
            }
        }

        // Click action and wait until field selector stops returning an element 
        // must exist before action is invoked
        public static void ClickAndWaitGone(this SafeWebDriverWait wait, IWebElement action, string fieldSelector, int delay = 0) {
            if (delay > 0) {
                Thread.Sleep(delay);
            }
            IWebElement field = wait.Driver.FindElement(By.CssSelector(fieldSelector));
            Assert.IsNotNull(field);
            action.Click();
            wait.Until(wd => (field = SafeFunc(() => wd.FindElement(By.CssSelector(fieldSelector)))) == null);
            Assert.IsNull(field);
        }


        // find then click action then wait until f returns true
        public static void ClickAndWait(this SafeWebDriverWait wait, string actionSelector, Func<IWebDriver, bool> f) {
            IWebElement action = wait.Driver.FindElement(By.CssSelector(actionSelector));
            wait.ClickAndWait(action, f);
        }

        // click action then wait until f returns true
        public static void ClickAndWait(this SafeWebDriverWait wait, IWebElement action, Func<IWebDriver, bool> f) {
            action.Click();
            wait.Until(f);
        }

        #endregion

        #region Asserts

        private static IWebDriver AssertContainsElementWithClass(this IWebDriver webDriver, string className) {
            try {
                webDriver.FindElement(By.ClassName(className));
            }
            catch (NoSuchElementException) {
                Assert.Fail();
            }

            return webDriver;
        }

        public static IWebDriver AssertContainsObjectView(this IWebDriver webDriver) {
            return webDriver.AssertContainsElementWithClass("nof-objectview");
        }

        public static IWebDriver AssertContainsObjectEdit(this IWebDriver webDriver) {
            return webDriver.AssertContainsElementWithClass("nof-objectedit");
        }

        public static IWebDriver AssertContainsObjectEditTransient(this IWebDriver webDriver) {
            try {
                IWebElement elem = webDriver.FindElement(By.ClassName("nof-objectedit"));
                var cls = elem.GetAttribute("class");

                Assert.IsTrue(cls.Contains("nof-objectedit"));
                Assert.IsTrue(cls.Contains("nof-transient"));
                Assert.IsTrue(cls.Replace("nof-transient", "").Replace("nof-objectedit", "").Trim().Length == 0);
            }
            catch (NoSuchElementException) {
                Assert.Fail();
            }

            return webDriver;
        }

        public static IWebDriver AssertPageTitleEquals(this IWebDriver webDriver, string expectedTitle) {
            Assert.AreEqual(expectedTitle, webDriver.Title);
            return webDriver;
        }

        public static void AssertElementExists(this IWebDriver webDriver, By by) {
            try {
                webDriver.FindElement(by);
            }
            catch (WebDriverException) //Should be NoSuchElementException, but this doesn't work on Firefox
            {
                Assert.Fail("Element should exist");
            }
        }

        public static void AssertElementDoesNotExist(this IWebDriver webDriver, By by) {
            try {
                webDriver.FindElement(by);
                Assert.Fail("Element should  not exist");
            }
            catch (WebDriverException) {
                //Should be NoSuchElementException, but this doesn't work on Firefox
                //As expected; test is OK
            }
        }

        #endregion

        public static IWebElement BrowserSpecificCheck(this IWebElement element, IWebDriver webDriver) {
            if (webDriver is InternetExplorerDriver) {
                element.SendKeys(Keys.Space);
            }
            else {
                element.Click();
            }
            return element;
        }

        #region Page level operations

        #endregion

        #region Tabbed History

        public static IWebDriver ClickTabLink(this IWebDriver webDriver, int index) {
            webDriver.FindElements(By.CssSelector(".nof-tab a"))[index].Click();
            webDriver.WaitForAjaxComplete();
            return webDriver;
        }

        private enum ClearType {
            ClearThis,
            ClearOthers,
            ClearAll
        };

        private static IWebDriver ClickClearContextMenu(this IWebDriver webDriver, int index, ClearType clearType) {
            var tab = webDriver.FindElements(By.CssSelector(".nof-tab"))[index];
            var loc = (ILocatable) tab;
            var mouse = ((IHasInputDevices) webDriver).Mouse;
            mouse.ContextClick(loc.Coordinates);
            webDriver.WaitForAjaxComplete();

            tab.FindElements(By.CssSelector("li a"))[(int) clearType].Click();
            webDriver.WaitForAjaxComplete();

            var firstTabImg = webDriver.FindElements(By.CssSelector(".nof-tab img")).FirstOrDefault();

            if (firstTabImg != null) {
                firstTabImg.Click();
            }

            return webDriver;
        }

        public static IWebDriver ClickClearItem(this IWebDriver webDriver, int index) {
            return webDriver.ClickClearContextMenu(index, ClearType.ClearThis);
        }

        public static IWebDriver ClickClearOthers(this IWebDriver webDriver, int index) {
            return webDriver.ClickClearContextMenu(index, ClearType.ClearOthers);
        }

        public static IWebDriver ClickClearAll(this IWebDriver webDriver, int index) {
            return webDriver.ClickClearContextMenu(index, ClearType.ClearAll);
        }


        #endregion

        #region Actions

      

        public static IWebDriver ClickSave(this IWebDriver webDriver) {
            return ClickSingleGenericButton(webDriver, "Save");
        }

        //Navigation buttons
        public static IWebDriver ClickFirst(this IWebDriver webDriver) {
            return ClickSingleGenericButton(webDriver, "First");
        }

        public static IWebDriver ClickPrevious(this IWebDriver webDriver) {
            return ClickSingleGenericButton(webDriver, "Previous");
        }

        public static IWebDriver ClickNext(this IWebDriver webDriver) {
            return ClickSingleGenericButton(webDriver, "Next");
        }

        public static IWebDriver ClickLast(this IWebDriver webDriver) {
            return ClickSingleGenericButton(webDriver, "Last");
        }

        // Table Format Buttons 

        public static IWebDriver ClickList(this IWebDriver webDriver) {
            return ClickSingleGenericButton(webDriver, "List");
        }

        public static IWebDriver ClickTable(this IWebDriver webDriver) {
            return ClickSingleGenericButton(webDriver, "Table");
        }

        private static IWebDriver ClickSingleGenericButton(this IWebDriver webDriver, string title) {
            IWebElement button = webDriver.FindElement(By.CssSelector("button[title=" + title + "]"));

            //webDriver.ScrollTo(button);
            button.Click();
            webDriver.WaitForAjaxComplete();
            return webDriver;
        }

        #endregion

        #region Fields

        public static IWebElement GetField(this IWebDriver webDriver, string fieldId) {
            IWebElement field = webDriver.FindElement(By.Id(fieldId));
            webDriver.WaitForAjaxComplete();
            return field;
        }

        public static IWebDriver ClickOnObjectLinkInField(this IWebDriver webDriver, string fieldId) {
            webDriver.GetField(fieldId).FindElement(By.TagName("a")).Click();
            Thread.Sleep(1000); // hack for unknown error
            webDriver.WaitForAjaxComplete();
            return webDriver;
        }

        public static IWebElement AssertValueEquals(this IWebElement field, string expectedValue) {
            return field.FindElement(By.ClassName("nof-value")).AssertTextEquals(expectedValue);
        }

        public static IWebElement AssertTextEquals(this IWebElement field, string expectedValue) {
            Assert.AreEqual(expectedValue, field.Text);
            return field;
        }

        public static IWebElement AssertInputValueEquals(this IWebElement field, string expectedValue) {
            Assert.AreEqual(expectedValue, field.FindElement(By.TagName("input")).GetAttribute("value"));
            return field;
        }

        public static IWebElement AssertInputValueNotEquals(this IWebElement field, string expectedValue) {
            Assert.AreNotEqual(expectedValue, field.FindElement(By.TagName("input")).GetAttribute("value"));
            return field;
        }

        public static IWebElement AssertIsEmpty(this IWebElement field) {
            ReadOnlyCollection<IWebElement> links = field.FindElement(By.ClassName("nof-object")).FindElements(By.TagName("a"));
            Assert.AreEqual(0, links.Count());
            return field;
        }

        public static IWebElement AssertHasMandatoryIndicator(this IWebElement field) {
            try {
                field.FindElement(By.ClassName("nof-mandatory-field-indicator"));
            }
            catch (NoSuchElementException) {
                Assert.Fail();
            }

            return field;
        }

        public static IWebElement AssertValidationErrorIs(this IWebElement field, string error) {
            try {
                Assert.AreEqual(error, field.FindElement(By.CssSelector("span.field-validation-error")).Text);
            }
            catch (NoSuchElementException) {
                Assert.Fail();
            }

            return field;
        }

        public static IWebElement AssertNoValidationError(this IWebElement field) {
            try {
                field.FindElement(By.CssSelector("span.field-validation-error"));
                Assert.Fail("unexpected validation error");
            }
            catch (NoSuchElementException) {
                // expected  
            }
            return field;
        }

        public static IWebElement AssertObjectHasTitle(this IWebElement field, string expectedTitle) {
            string actual = field.FindElement(By.ClassName("nof-object")).FindElement(By.TagName("a")).Text;
            Assert.AreEqual(expectedTitle, actual);
            return field;
        }

        public static IWebElement AssertIsModifiable(this IWebElement field) {
            Assert.AreEqual(1, field.FindElements(By.TagName("input")).Count);
            Assert.AreEqual(field.GetAttribute("id") + "-Input", field.FindElement(By.TagName("input")).GetAttribute("id"));
            return field;
        }

        public static IWebElement AssertIsUnmodifiable(this IWebElement field) {
            Assert.AreEqual(0, field.FindElements(By.TagName("input")).Count);
            return field;
        }

        public static IWebElement TypeText(this IWebElement field, string text, IWebDriver br, bool repeat = true) {
            IWebElement textField = GetTextField(field);
            textField.Clear();
            textField.SendKeys(text);
            return field;
        }

        private static IWebElement GetTextField(IWebElement field) {
            string fieldId = field.GetAttribute("id") + "-Input";
            return field.FindElement(By.Id(fieldId));
        }

        public static IWebElement AppendText(this IWebElement field, string text, IWebDriver br) {
            IWebElement textField = GetTextField(field);
            textField.SendKeys(text);
            return field;
        }

        #region Using the Find menu

     

       

        public static IWebElement ClickFinderAction(this IWebDriver webDriver, string fieldId, string actionId) {
            IWebElement field = webDriver.GetField(fieldId);
            field.FindElement(By.Id(actionId)).Click();
            webDriver.WaitForAjaxComplete();
            return field;
        }

       


        #endregion

        #region Fields with Drop downs

        public static IWebElement AssertSelectedDropDownItemIs(this IWebElement field, string expected) {
            Assert.AreEqual(expected, field.FindElements(By.TagName("option")).Where(o => o.GetAttribute("selected") != null).Select(o => o.Text).SingleOrDefault());
            return field;
        }

        public static IWebElement SelectDropDownItem(this IWebElement field, string name, IWebDriver br) {
            field.FindElement(By.TagName("select")).SendKeys(name);
            field.FindElement(By.TagName("select")).SendKeys(Keys.Tab);
            return field;
        }

        public static IWebElement SelectListBoxItems(this IWebElement field, IWebDriver br, params string[] names) {
            IWebElement select = field.FindElement(By.TagName("select"));
            IWebElement[] options = select.FindElements(By.TagName("option")).Where(o => names.Contains(o.Text)).ToArray();
            Assert.AreEqual(names.Count(), options.Count(), "all options not found in list");

            foreach (string name in names) {
                IWebElement option = options.Single(o => o.Text == name);

                IKeyboard kb = ((IHasInputDevices) br).Keyboard;

                kb.PressKey(Keys.Control);
                option.Click();
                kb.ReleaseKey(Keys.Control);
            }
            select.SendKeys(Keys.Tab);
            br.WaitForAjaxComplete();

            return field;
        }

        #endregion

        #endregion

        #region Collections

        public static IWebElement GetStandaloneTable(this IWebDriver webDriver) {
            return webDriver.FindElement(By.ClassName("nof-collection-table"));
        }

        public static IWebElement GetStandaloneList(this IWebDriver webDriver) {
            return webDriver.FindElement(By.ClassName("nof-collection-list"));
        }

        public static IWebElement GetInternalCollection(this IWebDriver webDriver, string collectionId) {
            IWebElement coll = GetField(webDriver, collectionId);
            AssertIsCollection(coll);
            return coll;
        }

        private static void AssertIsCollection(IWebElement collection) {
            try {
                collection.FindElement(By.CssSelector("div.nof-collection-table, div.nof-collection-list, div.nof-collection-summary"));
            }
            catch (NoSuchElementException) {
                Assert.Fail("Selected Div is not a Collection");
            }
        }

        public static IWebElement ViewAsTable(this IWebDriver webDriver, string collectionId) {
            return ViewAs(webDriver, collectionId, "nof-table");
        }

        public static IWebElement ViewAsList(this IWebDriver webDriver, string collectionId) {
            return ViewAs(webDriver, collectionId, "nof-list");
        }

        public static IWebElement ViewAsSummary(this IWebDriver webDriver, string collectionId) {
            return ViewAs(webDriver, collectionId, "nof-summary");
        }

        public static IWebElement ViewAs(this IWebDriver webDriver, string collectionId, string buttonName) {
            IWebElement collection = GetInternalCollection(webDriver, collectionId);
            AssertIsCollection(collection);
            IWebElement button = collection.FindElement(By.ClassName(buttonName));
            button.Click();
            webDriver.WaitForAjaxComplete();
            return collection;
        }

        public static IWebElement AssertSummaryEquals(this IWebElement collection, string expected) {
            AssertIsCollection(collection);
            Assert.IsTrue(collection.FindElements(By.TagName("div"))[1].GetAttribute("class") == "nof-collection-summary", "Collection is not in Summary view");
            Assert.AreEqual(expected, collection.FindElement(By.CssSelector("div.nof-object")).Text);
            return collection;
        }

        public static IWebElement CheckAll(this IWebElement collection, IWebDriver br) {
            IWebElement all = collection.FindElement(By.Id("checkboxAll"));
            Assert.IsFalse(all.Selected, "Box is already checked");
            all.BrowserSpecificCheck(br);
            Thread.Sleep(1000); // for javascript to run 
            return collection;
        }

        public static IWebElement UnCheckAll(this IWebElement collection, IWebDriver br) {
            IWebElement all = collection.FindElement(By.Id("checkboxAll"));

            Assert.IsTrue(all.Selected, "Box is not checked");
            all.BrowserSpecificCheck(br);
            Thread.Sleep(1000); // for javascript to run 
            return collection;
        }

        #region Row-based operations

        public static IWebElement GetRow(this IWebElement collection, int row) {
            IWebElement table = collection.FindElement(By.TagName("table"));
            int rowNumber = row;
            //if (collection.Div(Find.ByClass("Collection-List")).Exists) {
            //    rowNumber--; //Table rows numbered from zero; not necessary for table view as row zero is the header
            //}
            ReadOnlyCollection<IWebElement> rows = table.FindElements(By.TagName("tr"));
            Assert.IsTrue(rowNumber >= 0 && rowNumber <= rows.Count - 1, "Row number is out of range for table");
            return rows.ElementAt(rowNumber);
        }

        public static IWebElement CheckRow(this IWebElement row, IWebDriver br) {
            IWebElement box = CheckBox(row);
            Assert.IsFalse(box.Selected, "Box is already checked");
            box.BrowserSpecificCheck(br);
            return row;
        }

        public static IWebElement UnCheckRow(this IWebElement row, IWebDriver br) {
            IWebElement box = CheckBox(row);
            Assert.IsTrue(box.Selected, "Box is not checked");
            box.BrowserSpecificCheck(br);
            return row;
        }

        private static IWebElement CheckBox(IWebElement row) {
            return row.FindElement(By.CssSelector("input[type=checkbox]"));
        }

        public static void ClickRemove(this IWebElement row, IWebDriver webDriver) {
            row.FindElement(By.ClassName("nof-remove")).Click();
        }

        #endregion

        #region Cell-based operations

        /// <summary>
        /// Returns the text contents of a table cell which may be a value, or the title of a reference object
        /// </summary>
        /// <param name="coll">Collection that is expected to contain a Table</param>
        /// <param name="rowNumber">'0' is typically the header row</param>
        /// <param name="column">Leftmost column is '0', which may be a selector box</param>
        /// <returns></returns>
        public static string TextContentsOfCell(this IWebElement coll, int rowNumber, int column) {
            return coll.FindElements(By.TagName("tr")).ElementAt(rowNumber).FindElements(By.TagName("td")).ElementAt(column).Text;
        }

        #endregion

        #endregion

        #region Waiting for AJAX (2)

        static SeHelpers() {
            RunningAjax = true; // default to true
        }

        public static bool RunningAjax { get; set; }

        public static void WaitForAjaxComplete(this IWebDriver webDriver) {
            WaitForAjaxComplete(webDriver, 500);
        }

        public static void WaitForAjaxComplete(this IWebDriver webDriver, int pollingIntervalInMs) {
            if (!RunningAjax) {
                Thread.Sleep(5000);
                return;
            }

            Thread.Sleep(pollingIntervalInMs);
            while (true) {
                var executor = (IJavaScriptExecutor) webDriver;

                if (executor.ExecuteScript("return typeof nakedObjects").Equals("undefined") || executor.ExecuteScript("return nakedObjects.ajaxCount").Equals(0L)) {
                    break;
                }

                Thread.Sleep(pollingIntervalInMs/10);
            }
        }

        #endregion
    }
}