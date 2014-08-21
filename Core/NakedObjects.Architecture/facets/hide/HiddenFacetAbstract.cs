// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Interactions;
using NakedObjects.Architecture.Persist;

namespace NakedObjects.Architecture.Facets.Hide {
    public abstract class HiddenFacetAbstract : SingleWhenValueFacetAbstract, IHiddenFacet {
        protected HiddenFacetAbstract(When when, IFacetHolder holder)
            : base(Type, holder, when) {}

        public static Type Type {
            get { return typeof (IHiddenFacet); }
        }

        #region IHiddenFacet Members

        public virtual string Hides(InteractionContext ic, INakedObjectPersistor persistor) {
            return HiddenReason(ic.Target);
        }

        public virtual HiddenException CreateExceptionFor(InteractionContext ic, INakedObjectPersistor persistor) {
            return new HiddenException(ic, Hides(ic, persistor));
        }

        public abstract string HiddenReason(INakedObject nakedObject);

        #endregion
    }
}