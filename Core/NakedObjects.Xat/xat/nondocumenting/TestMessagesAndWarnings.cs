﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Objects;

namespace NakedObjects.Xat {
    public static class TestMessagesAndWarnings {

        /// <summary>
        /// Messages written by <see cref="IDomainObjectContainer.InformUser"/> - This clears once read !
        /// </summary>
        public static string[] Messages(IMessageBroker messageBroker) {
             return messageBroker.Messages; 
        }

        /// <summary>
        /// Warnings written by <see cref="IDomainObjectContainer.WarnUser"/> - This clears once read !
        /// </summary>
        public static string[] Warnings(IMessageBroker messageBroker) {
             return messageBroker.Warnings; 
        }

        /// <summary>
        /// Messages written by <see cref="IDomainObjectContainer.InformUser"/> - This clears all messages once asserted !
        /// </summary>
        public static void AssertLastMessageIs(string expected, IMessageBroker messageBroker) {
            Assert.AreEqual(expected, Messages(messageBroker).Last());
        }

        /// <summary>
        /// Warnings written by <see cref="IDomainObjectContainer.WarnUser"/> - This clears all warnings once asserted !
        /// </summary>
        public static void AssertLastWarningIs(string expected, IMessageBroker messageBroker) {
            Assert.AreEqual(expected, Warnings(messageBroker).Last());
        }

        /// <summary>
        /// Messages written by <see cref="IDomainObjectContainer.InformUser"/> - This clears all messages once asserted !
        /// </summary>
        public static void AssertLastMessageContains(string expected, IMessageBroker messageBroker) {
            string lastMessage = Messages(messageBroker).Last();
            Assert.IsTrue(lastMessage.Contains(expected), @"Last message expected to contain: '{0}' actual: '{1}'", expected, lastMessage);
        }

        /// <summary>
        /// Warnings written by <see cref="IDomainObjectContainer.WarnUser"/> - This clears all warnings once asserted  !
        /// </summary>
        public static void AssertLastWarningContains(string expected, IMessageBroker messageBroker) {
            string lastWarning = Warnings(messageBroker).Last();
            Assert.IsTrue(lastWarning.Contains(expected), @"Last warning expected to contain: '{0}' actual: '{1}'", expected, lastWarning);
        }
    }
}