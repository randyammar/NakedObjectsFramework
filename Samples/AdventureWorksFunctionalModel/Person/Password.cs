using NakedObjects;
using System;
using System.ComponentModel.DataAnnotations;

namespace AdventureWorksModel {
    public  class Password : IHasRowGuid, IHasModifiedDate
    {
        public Password(
            int businessEntityID,
            string passwordHash,
            string passwordSalt,
            Guid rowguid,
            DateTime modifiedDate
            )
        {
            BusinessEntityID = businessEntityID;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            this.rowguid = rowguid;
            ModifiedDate = modifiedDate;
        }

        public Password() { }

        public virtual int BusinessEntityID { get; set; }
        public virtual string PasswordHash { get; set; }
        public virtual string PasswordSalt { get; set; }
        public virtual Guid rowguid { get; set; }
        [ConcurrencyCheck]
        public virtual DateTime ModifiedDate { get; set; }
    }

    public static class PasswordFunctions
    {
        public static Password Persisting(Password pw, [Injected] Guid guid, [Injected] DateTime now)
        {
            return Updating(pw, now).SetRowGuid(guid);
        }

        public static Password Updating(Password pw, [Injected] DateTime now)
        {
            return pw.UpdateModifiedDate(now);
        }
    }
}
