// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Collections.Generic;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets.Actcoll.Typeof;
using NakedObjects.Architecture.Resolve;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Testing {
    class ProgrammableNakedObject : INakedObject {
        private  object poco;
        private readonly INakedObjectSpecification specification;
        private ResolveStateMachine resolveState;
        private IVersion version;
        private IOid oid;

        internal ProgrammableNakedObject(object poco, INakedObjectSpecification specification, IOid oid) {
            this.poco = poco;
            this.specification = specification;
            this.oid = oid;

            resolveState = new ResolveStateMachine(this, null, null);
        }

        public object Object {
            get { return poco; }
        }

        public INakedObjectSpecification Specification {
            get { return specification; }
        }

        public IOid Oid {
            get { return oid; }
        }

        public ResolveStateMachine ResolveState {
            get { return resolveState; }
        }

        public IVersion Version {
            get { return version; }
        }

        public IVersion OptimisticLock {
            set { version = value; }
        }

        public ITypeOfFacet TypeOfFacet {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public string IconName() {
            throw new System.NotImplementedException();
        }

        public string TitleString() {
            throw new System.NotImplementedException();
        }

        public string InvariantString() {
            throw new NotImplementedException();
        }

        public void CheckLock(IVersion otherVersion) {
            throw new System.NotImplementedException();
        }

        public void ReplacePoco(object poco) {
            this.poco = poco;
        }

        public void FireChangedEvent() {
            throw new System.NotImplementedException();
        }

        public string ValidToPersist() {
            throw new System.NotImplementedException();
        }

        public void SetATransientOid(IOid oid) {
            throw new NotImplementedException();
        }
    }
}
