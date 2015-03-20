﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Mvc.Selenium.Test.Helper;
using OpenQA.Selenium;

namespace NakedObjects.Mvc.Selenium.Test {
    public abstract class ObjectEditTests : AWWebTest {
        private void FindCustomerAndEdit(string custId) {
            var f = wait.ClickAndWait("#CustomerRepository-FindCustomerByAccountNumber button", "#CustomerRepository-FindCustomerByAccountNumber-AccountNumber-Input");
            f.Clear();
            f.SendKeys(custId + Keys.Tab);
            var edit = wait.ClickAndWait(".nof-ok", ".nof-edit");
            wait.ClickAndWait(edit, ".nof-objectedit");
        }

        public void DoEditPersistedObject() {
            Login();
            FindCustomerAndEdit("AW00000546");

            //Check basics of edit view
            br.AssertPageTitleEquals("Field Trip Store, AW00000546");
            br.AssertElementExists(By.CssSelector("[title=Save]"));
            try {
                br.AssertElementDoesNotExist(By.CssSelector("[title=Edit]"));
            }
            catch (Exception e) {
                string m = e.Message;
            }
            //Test modifiable field
            br.FindElement(By.CssSelector("#Store-Name")).AssertIsModifiable();

            //Test unmodifiable field
            br.FindElement(By.CssSelector("#Store-AccountNumber")).AssertIsUnmodifiable();

            Assert.AreEqual("nof-collection-table", br.GetInternalCollection("Store-Addresses").FindElements(By.TagName("div"))[1].GetAttribute("class"));
            IWebElement table = br.GetInternalCollection("Store-Addresses").FindElement(By.TagName("table"));
            Assert.AreEqual(1, table.FindElements(By.TagName("tr")).Count); //First row is header
            Assert.AreEqual("Main Office: 2575 Rocky Mountain Ave. ...", table.FindElements(By.TagName("tr"))[0].FindElements(By.TagName("td"))[0].Text);

            // Collection Summary
            br.ViewAsSummary("Store-Addresses");
            IWebElement contents = br.GetInternalCollection("Store-Addresses").FindElement(By.ClassName("nof-object"));
            Assert.AreEqual("1 Customer Address", contents.Text);

            // Collection List
            br.ViewAsList("Store-Addresses");
            Assert.AreEqual("nof-collection-list", br.GetInternalCollection("Store-Addresses").FindElements(By.TagName("div"))[1].GetAttribute("class"));
            table = br.GetInternalCollection("Store-Addresses").FindElement(By.TagName("table"));
            Assert.AreEqual(1, table.FindElements(By.TagName("tr")).Count);
            Assert.AreEqual("Main Office: 2575 Rocky Mountain Ave. ...", table.FindElement(By.ClassName("nof-object")).Text);

            // Collection Table
            br.ViewAsTable("Store-Addresses");
            Assert.AreEqual("nof-collection-table", br.GetInternalCollection("Store-Addresses").FindElements(By.TagName("div"))[1].GetAttribute("class"));
            table = br.GetInternalCollection("Store-Addresses").FindElement(By.TagName("table"));
            Assert.AreEqual(1, table.FindElements(By.TagName("tr")).Count); //First row is header
            Assert.AreEqual("Main Office: 2575 Rocky Mountain Ave. ...", table.FindElements(By.TagName("tr"))[0].FindElements(By.TagName("td"))[0].Text);

            //Return to View via History

            br.FindElement(By.CssSelector(".nof-tab:first-of-type a")).Click();
            wait.ClickAndWait(".nof-tab:first-of-type a", ".nof-objectview");
        }

        public void DoEditTableHeader() {
            Login();

            var f = wait.ClickAndWait("#ProductRepository-FindProductByNumber button", "#ProductRepository-FindProductByNumber-Number-Input");
            f.Clear();
            f.SendKeys("BK-M38S-46" + Keys.Tab);
            var edit = wait.ClickAndWait(".nof-ok", ".nof-edit");
            wait.ClickAndWait(edit, ".nof-objectedit");

            // Collection Table
            //br.ViewAsTable("Product-ProductInventory"); - noew rendered eagerly
            Assert.AreEqual("nof-collection-table", br.GetInternalCollection("Product-ProductInventory").FindElements(By.TagName("div"))[1].GetAttribute("class"));
            IWebElement table = br.GetInternalCollection("Product-ProductInventory").FindElement(By.TagName("table"));
            Assert.AreEqual(3, table.FindElements(By.TagName("tr")).Count);
            Assert.AreEqual(4, table.FindElements(By.TagName("tr"))[0].FindElements(By.TagName("th")).Count);
            Assert.AreEqual(4, table.FindElements(By.TagName("tr"))[1].FindElements(By.TagName("td")).Count);
            Assert.AreEqual(4, table.FindElements(By.TagName("tr"))[2].FindElements(By.TagName("td")).Count);

            Assert.AreEqual("Quantity", table.FindElements(By.TagName("tr"))[0].FindElements(By.TagName("th"))[0].Text);
            Assert.AreEqual("Location", table.FindElements(By.TagName("tr"))[0].FindElements(By.TagName("th"))[1].Text);
            Assert.AreEqual("Shelf", table.FindElements(By.TagName("tr"))[0].FindElements(By.TagName("th"))[2].Text);
            Assert.AreEqual("Bin", table.FindElements(By.TagName("tr"))[0].FindElements(By.TagName("th"))[3].Text);

            Assert.AreEqual("60", table.FindElements(By.TagName("tr"))[1].FindElements(By.TagName("td"))[0].Text);
            Assert.AreEqual("Finished Goods Storage", table.FindElements(By.TagName("tr"))[1].FindElements(By.TagName("td"))[1].Text);
            Assert.AreEqual("N/A", table.FindElements(By.TagName("tr"))[1].FindElements(By.TagName("td"))[2].Text);
            Assert.AreEqual("0", table.FindElements(By.TagName("tr"))[1].FindElements(By.TagName("td"))[3].Text);

            Assert.AreEqual("104", table.FindElements(By.TagName("tr"))[2].FindElements(By.TagName("td"))[0].Text);
            Assert.AreEqual("Final Assembly", table.FindElements(By.TagName("tr"))[2].FindElements(By.TagName("td"))[1].Text);
            Assert.AreEqual("N/A", table.FindElements(By.TagName("tr"))[2].FindElements(By.TagName("td"))[2].Text);
            Assert.AreEqual("0", table.FindElements(By.TagName("tr"))[2].FindElements(By.TagName("td"))[3].Text);
        }

        public void DoCancelButtonOnObjectEdit() {
            Login();
            FindCustomerAndEdit("AW00000546");
            wait.ClickAndWait(".nof-cancel", ".nof-objectview");
            br.AssertPageTitleEquals("Field Trip Store, AW00000546");
        }

        public void DoSaveWithNoChanges() {
            Login();
            FindCustomerAndEdit("AW00000071");
            wait.ClickAndWait(".nof-save", ".nof-objectview");
        }

        public void DoChangeStringField() {
            Login();
            FindCustomerAndEdit("AW00000072");

            br.FindElement(By.CssSelector("#Store-Name")).AssertInputValueEquals("Outdoor Equipment Store").TypeText("Temporary Name", br);
            wait.ClickAndWait(".nof-save", ".nof-objectview");
            br.FindElement(By.CssSelector("#Store-Name")).AssertValueEquals("Temporary Name");
        }

        public void DoChangeDropDownField() {
            Login();
            FindCustomerAndEdit("AW00000073");

            br.FindElement(By.CssSelector("#Store-SalesTerritory")).AssertSelectedDropDownItemIs("Northwest").SelectDropDownItem("C", br);
            wait.ClickAndWait(".nof-save", ".nof-objectview");
            br.FindElement(By.CssSelector("#Store-SalesTerritory")).AssertObjectHasTitle("Central");
        }

        public void DoChangeReferencePropertyViaRecentlyViewed() {
            Login();

            FindSalesPerson("Ito");
            FindCustomerByAccountNumber("AW00000074");

            wait.Until(wd => wd.Title == "Parcel Express Delivery Service, AW00000074");

            br.FindElement(By.CssSelector("#Store-SalesPerson")).AssertObjectHasTitle("David Campbell");

            var recentlyViewed = wait.ClickAndWait(".nof-edit", "#Store-SalesPerson [title='Recently Viewed']");

            var select = wait.ClickAndWait(recentlyViewed, "#Store-SalesPerson [title='Select']:first-of-type");
            wait.ClickAndWaitGone(select, "#Store-SalesPerson [title='Select']");

            wait.ClickAndWait(".nof-save", ".nof-objectview");

            br.FindElement(By.CssSelector("#Store-SalesPerson")).AssertObjectHasTitle("Shu Ito");
        }

        public void DoChangeReferencePropertyViaRemove() {
            Login();
            FindCustomerByAccountNumber("AW00000076");
            br.FindElement(By.CssSelector("#Store-SalesPerson")).AssertObjectHasTitle("Jillian Carson");

            var remove = wait.ClickAndWait(".nof-edit", "#Store-SalesPerson [title=Remove]");

            wait.ClickAndWaitGone(remove, "#Store-SalesPerson img");
            wait.ClickAndWait(".nof-save", ".nof-objectview");
            br.FindElement(By.CssSelector("#Store-SalesPerson")).AssertIsEmpty();
        }

        public void DoChangeReferencePropertyViaAFindAction() {
            Login();
            FindCustomerByAccountNumber("AW00000075");
            br.FindElement(By.CssSelector("#Store-SalesPerson")).AssertObjectHasTitle("Jillian Carson");

            var finder = wait.ClickAndWait(".nof-edit", "#Store-SalesPerson-SalesRepository-FindSalesPersonByName");
            var lastName = wait.ClickAndWait(finder, "#SalesRepository-FindSalesPersonByName-LastName-Input");

            lastName.Clear();
            lastName.SendKeys("Vargas" + Keys.Tab);

            wait.ClickAndWait(".nof-ok", wd => wd.FindElement(By.CssSelector("#Store-SalesPerson-Select-AutoComplete")).GetAttribute("value") == "Garrett Vargas");

            wait.ClickAndWait(".nof-save", ".nof-objectview");
            br.FindElement(By.CssSelector("#Store-SalesPerson")).AssertObjectHasTitle("Garrett Vargas");
        }

        public void DoChangeReferencePropertyViaANewAction() {
            Login();

            var edit = wait.ClickAndWait("#WorkOrderRepository-RandomWorkOrder button", ".nof-edit");
            var newProduct = wait.ClickAndWait(edit, "#WorkOrder-Product-ProductRepository-NewProduct");

            wait.ClickAndWait(newProduct, "#Product-Name-Input");

            br.FindElement(By.CssSelector("#Product-Name")).TypeText("test", br);
            br.FindElement(By.CssSelector("#Product-ProductNumber")).TypeText("test", br);
            br.FindElement(By.CssSelector("#Product-ListPrice")).TypeText("10", br);
            br.FindElement(By.CssSelector("#Product-SafetyStockLevel")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-ReorderPoint")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-DaysToManufacture")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-SellStartDate")).TypeText("1/1/2020", br);
            br.FindElement(By.CssSelector("#Product-SellStartDate")).AppendText(Keys.Escape, br);
            br.FindElement(By.CssSelector("#Product-StandardCost")).TypeText("1", br);

            var select = wait.ClickAndWait("button[name='InvokeActionAsSave']", "button[title='Select']");
            var product = wait.ClickAndWait(select, "#WorkOrder-Product");

            product.AssertInputValueEquals("test ");
        }

        public void DoChangeReferencePropertyViaAutoComplete() {
            Login();

            var edit = wait.ClickAndWait("#WorkOrderRepository-RandomWorkOrder button", ".nof-edit");
            wait.ClickAndWait(edit, "#WorkOrder-Product-Select-AutoComplete");

            br.FindElement(By.CssSelector("#WorkOrder-Product-Select-AutoComplete")).Clear();
            br.FindElement(By.CssSelector("#WorkOrder-Product-Select-AutoComplete")).SendKeys("HL");
            br.WaitForAjaxComplete();
            br.FindElement(By.CssSelector("#WorkOrder-Product-Select-AutoComplete")).SendKeys(Keys.ArrowDown);
            br.FindElement(By.CssSelector("#WorkOrder-Product-Select-AutoComplete")).SendKeys(Keys.ArrowDown);
            br.FindElement(By.CssSelector("#WorkOrder-Product-Select-AutoComplete")).SendKeys(Keys.Tab);
            br.FindElement(By.CssSelector("#WorkOrder-Product")).AssertInputValueEquals("HL Crankset");
        }

        public void DoChangeReferencePropertyViaANewActionFailMandatory() {
            Login();

            var edit = wait.ClickAndWait("#WorkOrderRepository-RandomWorkOrder button", ".nof-edit");
            var newProduct = wait.ClickAndWait(edit, "#WorkOrder-Product-ProductRepository-NewProduct");

            wait.ClickAndWait(newProduct, "#Product-Name-Input");

            br.FindElement(By.CssSelector("#Product-Name")).TypeText("test", br);
            br.FindElement(By.CssSelector("#Product-ProductNumber")).TypeText("test", br);
            br.FindElement(By.CssSelector("#Product-ListPrice")).TypeText("10", br);

            br.FindElement(By.CssSelector("#Product-SafetyStockLevel")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-ReorderPoint")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-DaysToManufacture")).TypeText("1", br);
            //br.FindElement(By.CssSelector("#Product-SellStartDate")).TypeText(DateTime.Today.AddDays(1).ToShortDateString(), br); - missing mandatory
            br.FindElement(By.CssSelector("#Product-StandardCost")).TypeText("1", br);

            var error = wait.ClickAndWait(".nof-save", "span.field-validation-error");
            error.AssertTextEquals("Mandatory");
            Assert.AreEqual("test", br.FindElement(By.CssSelector("#Product-Name-Input")).GetAttribute("value"));
        }

        public void DoChangeReferencePropertyViaANewActionFailInvalid() {
            Login();

            var edit = wait.ClickAndWait("#WorkOrderRepository-RandomWorkOrder button", ".nof-edit");
            var newProduct = wait.ClickAndWait(edit, "#WorkOrder-Product-ProductRepository-NewProduct");

            wait.ClickAndWait(newProduct, "#Product-Name-Input");

            br.FindElement(By.CssSelector("#Product-Name")).TypeText("test", br);
            br.FindElement(By.CssSelector("#Product-ProductNumber")).TypeText("test", br);
            br.FindElement(By.CssSelector("#Product-ListPrice")).TypeText("10", br);

            br.FindElement(By.CssSelector("#Product-SafetyStockLevel")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-ReorderPoint")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-DaysToManufacture")).TypeText("1", br);
            br.FindElement(By.CssSelector("#Product-SellStartDate")).TypeText("1/1/2020", br);
            br.FindElement(By.CssSelector("#Product-SellStartDate")).AppendText(Keys.Escape, br);
            br.FindElement(By.CssSelector("#Product-StandardCost")).TypeText("test", br); // invalid

            var error = wait.ClickAndWait(".nof-save", "span.field-validation-error");
            error.AssertTextEquals("Invalid Entry");
            Assert.AreEqual("test", br.FindElement(By.CssSelector("#Product-Name-Input")).GetAttribute("value"));
        }

        public void DoCheckDefaultsOnFindAction() {
            Login();
            FindOrder("SO53144");

            var finder = wait.ClickAndWait(".nof-edit", "#SalesOrderHeader-CurrencyRate-OrderContributedActions-FindRate");

            wait.ClickAndWait(finder, wd => wd.FindElement(By.CssSelector("#OrderContributedActions-FindRate-Currency input")).GetAttribute("value") == "US Dollar");

            br.FindElement(By.CssSelector("#OrderContributedActions-FindRate-Currency1")).AssertInputValueEquals("Euro");
        }

        public void DoNoEditButtonWhenNoEditableFields() {
            Login();
            FindOrder("SO53144");
            br.ClickOnObjectLinkInField("SalesOrderHeader-CreditCard");
            br.AssertPageTitleEquals("**********7212");
            br.AssertElementDoesNotExist(By.CssSelector("[title=Edit]"));
        }

        public void DoRefresh() {
            Login();
            FindCustomerAndEdit("AW00000071");

            br.Navigate().Refresh();
            br.WaitForAjaxComplete();
            br.AssertContainsObjectEdit();
        }

        public void DoNoValidationOnTransientUntilSave() {
            Login();
            FindCustomerByAccountNumber("AW00000532");

            var ok = wait.ClickAndWait("#Store-CreateNewOrder button", ".nof-ok");
            wait.ClickAndWait(ok, "#SalesOrderHeader-ShipDate-Input");

            br.FindElement(By.Id("SalesOrderHeader-ShipDate")).TypeText(DateTime.Now.AddDays(-1).ToShortDateString(), br);
            br.FindElement(By.Id("SalesOrderHeader-Status")).SelectDropDownItem("Approved", br);
            br.FindElement(By.Id("SalesOrderHeader-StoreContact")).SelectDropDownItem("Diane Glimp", br);
            br.FindElement(By.Id("SalesOrderHeader-ShipMethod")).SelectDropDownItem("XRQ", br);
            br.FindElement(By.Id("SalesOrderHeader-ShipDate")).AssertNoValidationError();

            var error = wait.ClickAndWait(".nof-save", "span.field-validation-error:last-of-type");
            error.AssertTextEquals("Ship date cannot be before order date");

            br.AssertContainsObjectEdit();
        }

        #region abstract 

        public abstract void EditPersistedObject();

        public abstract void EditTableHeader();

        public abstract void CancelButtonOnObjectEdit();

        public abstract void SaveWithNoChanges();

        public abstract void ChangeStringField();

        public abstract void ChangeDropDownField();

        public abstract void ChangeReferencePropertyViaRecentlyViewed();

        public abstract void ChangeReferencePropertyViaRemove();

        public abstract void ChangeReferencePropertyViaAFindAction();

        public abstract void ChangeReferencePropertyViaNewAction();

        public abstract void NoValidationOnTransientUntilSave();

        public abstract void Refresh();

        public abstract void NoEditButtonWhenNoEditableFields();

        public abstract void CheckDefaultsOnFindAction();

        public abstract void ChangeReferencePropertyViaAutoComplete();

        public abstract void ChangeReferencePropertyViaANewActionFailMandatory();

        public abstract void ChangeReferencePropertyViaANewActionFailInvalid();

        #endregion
    }
}