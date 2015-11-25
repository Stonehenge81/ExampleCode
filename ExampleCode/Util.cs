using System;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Transactions;
using RSS.OMEGA.DataEntity;

namespace RSS.WCF.Services.OMEGA.Repository
{
    static class Util
    {       
        internal static void DoAction<T>(Action<T> action)
        where T : OmegaEntity, new()
        {
            using (var contextDb = new T())
            {
                action(contextDb);
            }
        }

        internal static void DoActionTransaction<T>(Action<T> action)
        where T : OmegaEntity, new()
        {
            using (var contextDb = new T())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 60, 0)))
                {
                    action(contextDb);
                    contextDb.SaveChanges();
                    transaction.Complete();
                }
            }
        }

        public static void AttachUpdated(this ObjectContext obj, EntityObject objectDetached)
        {
            if (objectDetached.EntityState == EntityState.Detached)
            {
                object original = null;
                if (obj.TryGetObjectByKey(objectDetached.EntityKey, out original))
                    obj.ApplyPropertyChanges(objectDetached.EntityKey.EntitySetName, objectDetached);
                else
                    throw new ObjectNotFoundException();
            }
        }
    }
}
