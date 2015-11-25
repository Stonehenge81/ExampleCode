using System;
using System.Linq;
using RSS.Framework;
using RSS.OMEGA.DataEntity;
using RSS.WCF.Contracts.OMEGA.Models;
using RSS.WCF.Contracts.Rights;

namespace RSS.WCF.Services.OMEGA.Repository
{
    sealed class RepositoryFantom<T>
        where T : OmegaEntity, new()
    {
        readonly DateTime dateMIN = new DateTime(1953, 1, 1);
        readonly DateTime dateMAX = new DateTime(9999, 12, 31);

        void CheckDate(DateTime date)
        {
            if (date < dateMIN || date > dateMAX)
            {
                throw new ArgumentOutOfRangeException(
                    String.Format("Значение фильтра даты должно быть в диапазоне [{0};{1}]", dateMIN, dateMAX));
            }
        }

        public FantomModel[] GetFantoms(FantomFilterModel filter)
        {
            if (FantomFilterModel.IsNull(filter))
            {
                throw new ArgumentNullException("Значение фильтра не было получено");
            }
            if (filter.PeriodFromDate.HasValue)
            {
                CheckDate(filter.PeriodFromDate.Value);
            }
            if (filter.PeriodToDate.HasValue)
            {
                CheckDate(filter.PeriodToDate.Value);
            }
            filter.SupplierName = filter.SupplierName.ToStringTrim();
            FantomModel[] rezult = null;
            Util.DoAction<T>(p =>
            {
                IQueryable<Pay> queryPay = p.Pays;
                IQueryable<Supplier> querySupplier = p.Suppliers;
                if (filter.PeriodFromDate.HasValue)
                {
                    var date = filter.PeriodFromDate.Value.Date.AddMilliseconds(-1);
                    queryPay = queryPay.Where(z => z.PeriodDate > date);
                }
                if (filter.PeriodToDate.HasValue)
                {
                    var date = filter.PeriodToDate.Value.Date.AddDays(1);
                    queryPay = queryPay.Where(z => z.PeriodDate < date);
                }
                if (filter.TypeAcc.HasValue)
                {
                    var type = (int)filter.TypeAcc.Value;
                    queryPay = queryPay.Where(z => z.Action == type);
                }
                if (filter.TypeCurrency.HasValue)
                {
                    var type = (int)filter.TypeCurrency.Value;
                    queryPay = queryPay.Where(z => z.Currency == type);
                }
                if (!String.IsNullOrWhiteSpace(filter.SupplierName))
                {
                    querySupplier = querySupplier.Where(z => z.Name.Contains(filter.SupplierName));
                }
                rezult = (from fan in queryPay
                          join s in querySupplier on fan.Supplier equals s.UID
                          select new FantomModel
                          {
                              UID = fan.UID,
                              Comment = fan.Commеnt,
                              EventDate = fan.EventDate,
                              Sum = fan.Sum,
                              PeriodDate = fan.PeriodDate,
                              SupplierName = s.Name,
                              Currency = (TypeCurrencyEnum)(int)fan.Currency,
                              TypeAccount = (TypeAccEnum)(int)fan.Action
                          })
                    .ToArray();
            });
            return rezult;
        }

        public FantomModel DeleteFantom(FantomModel item)
        {
            if (item == null || item.UID == Guid.Empty)
            {
                throw new ArgumentNullException("Не могу удалить запись.\r\nИдентификатор записи не был получен");
            }
            Util.DoActionTransaction<T>(p =>
            {
                var dbitem = p.Pays.SingleOrDefault(z => z.UID == item.UID);
                if (dbitem == null)
                {
                    throw new ArgumentNullException("Не могу удалить запись.\r\nЗапись отсутствует");
                }
                //TODO крайне не рекомендуемое действие, заказчик пожелал именно удалить
                p.Pays.DeleteObject(dbitem);
            });
            return item;
        }

        static Supplier AddNewSupplier(String supplierName, T p)
        {
            if (String.IsNullOrWhiteSpace(supplierName))
            {
                throw new ArgumentNullException("Поставщик не задан");
            }
            supplierName = supplierName.ToStringTrim();
            var supDb = p.Suppliers.SingleOrDefault(z => z.Name == supplierName);
            if (supDb == null)
            {
                supDb = new Supplier { UID = Guid.NewGuid(), Name = supplierName };
                p.AddToSuppliers(supDb);
            }
            return supDb;
        }

        public FantomModel UpdateFantom(FantomModel item, UserModel user)
        {
            if (item == null || item.UID == Guid.Empty)
            {
                throw new ArgumentNullException("Не могу обновить запись.\r\nИдентификатор записи не был получен");
            }
            if (item.Sum < 0.01m)
            {
                throw new NotSupportedException("Сумма должна быть больше 0.01");
            }
            item.Comment = item.Comment.ToStringTrim();
            if (!String.IsNullOrWhiteSpace(item.Comment) && item.Comment.Length > 100)
            {
                throw new NotSupportedException("Длина комментария не должна превышать 100 символов");
            }
            CheckDate(item.PeriodDate);
            Util.DoActionTransaction<T>(p =>
            {
                var dbitem = p.Pays.SingleOrDefault(z => z.UID == item.UID);
                if (dbitem == null)
                {
                    throw new ArgumentNullException("Не могу обновить запись.\r\nЗапись отсутствует");
                }
                dbitem.Supplier = AddNewSupplier(item.SupplierName, p).UID;
                dbitem.Commеnt = item.Comment;
                dbitem.Currency = (int)item.Currency;
                dbitem.EventDate = DateTime.Now;
                dbitem.PeriodDate = item.PeriodDate;
                dbitem.Action = (int)item.TypeAccount;
                dbitem.Sum = item.Sum;
                dbitem.UserID = user.ID;
            });
            return item;
        }

        public FantomModel CreateFantom(CreateFantomModel item, UserModel user)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Не могу создать запись.\r\nЗапись не была получена");
            }
            if (item.Sum < 0.01m)
            {
                throw new NotSupportedException("Сумма должна быть больше 0.01");
            }
            if (String.IsNullOrWhiteSpace(item.SupplierName))
            {
                throw new NotSupportedException("Поставщик не был получен");
            }
            item.SupplierName = item.SupplierName.ToStringTrim();
            item.Comment = item.Comment.ToStringTrim();
            if (!String.IsNullOrWhiteSpace(item.Comment) && item.Comment.Length > 100)
            {
                throw new NotSupportedException("Длина комментария не должна превышать 100 символов");
            }
            CheckDate(item.PeriodDate);
            var newitem = new FantomModel(item);
            Util.DoActionTransaction<T>(p =>
            {
                var dbitem = new Pay { UID = newitem.UID, EventDate = newitem.EventDate };
                dbitem.Supplier = AddNewSupplier(item.SupplierName, p).UID;
                dbitem.Commеnt = item.Comment;
                dbitem.Currency = (int)item.Currency;
                dbitem.EventDate = DateTime.Now;
                dbitem.PeriodDate = item.PeriodDate;
                dbitem.Action = (int)item.TypeAccount;
                dbitem.Sum = item.Sum;
                dbitem.UserID = user.ID;
                p.Pays.AddObject(dbitem);
            });
            return newitem;
        }
    }
}
