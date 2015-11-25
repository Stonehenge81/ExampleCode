using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Security;
using RSS.Framework;
using RSS.OMEGA.DataEntity;
using RSS.WCF.Contracts.OMEGA;
using RSS.WCF.Contracts.OMEGA.Models;
using RSS.WCF.Contracts.Rights;
using RSS.WCF.Services.Core;
using RSS.WCF.Services.OMEGA.Repository;
using data = RSS.WCF.Services.OMEGA.Repository.Repository<RSS.OMEGA.DataEntity.OmegaEntity>;

namespace RSS.WCF.Services.OMEGA
{
    // Use a data contract as illustrated in the sample below to add composite types to service operations    
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerSession)]
    public class OmegaService :
        IOmegaService,
        IAccountService,
        IFantomService
    {
        readonly Repository<OmegaEntity> _repository;
        readonly RepositoryAccount<OmegaEntity> _RepositoryAccount;
        readonly RepositoryFantom<OmegaEntity> _RepositoryFantom;

        public OmegaService()
        {
            _repository = new Repository<OmegaEntity>(new CasheCommunicator(!data.IsTestDb() ? "endPointCashe" : "endPointCasheTest"));
            _RepositoryAccount = new RepositoryAccount<OmegaEntity>();
            _RepositoryFantom = new RepositoryFantom<OmegaEntity>();
        }

        #region кредитные запчасти

        /// <summary>
        /// получить запчасти
        /// </summary>        
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        SparePartCredit[] IOmegaService.GetShipmentsSparePart(ApplicationVersion applicationVersion, FilterSparePartCredit filter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// изменить статус запчасти
        /// </summary>        
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        SparePartCredit IOmegaService.ChangeSparePartStatus(ApplicationVersion applicationVersion, SparePartCredit sparePart, StatusSparePartEnum status)
        {
            throw new NotImplementedException();
        }

        #endregion

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public ApplicationSettings GetUser(ApplicationVersion version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("Информация о приложении не была пролучена");
            }
            version.IsReleaseDb = !data.IsTestDb();
            return RunSecurity(version, user => new ApplicationSettings { UserModel = user, ApplicationVersion = version });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        void IOmegaService.UpdateSumRequest(ApplicationVersion applicationVersion, Guid requestUID, Decimal sum)
        {
            RunSecurity(applicationVersion, user =>
            {
                CheckRightsRequests(user);
                _repository.UpdateSumRequest(requestUID, sum, user);
                return true;
            });
        }

        GraphicType[] IOmegaService.GetGraphicTypes(ApplicationVersion applicationVersion)
        {
            return Report.GraphicDatas;
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        GraphicData IOmegaService.GetGraphicData(ApplicationVersion applicationVersion, GraphicType type)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightChangeBalance(user);
                return _repository.GetGraphicData(type);
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        StatusRealisationToPercent[] IOmegaService.GetAllStatusRealisationToPercent(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, user => _repository.GetAllStatusRealisationToPercent(user));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ExchangeRateModel IOmegaService.GetChangeCurenceUSD(ApplicationVersion applicationVersion, DateTime dateTime)
        {
            return _repository.GetChangeCurenceUSD(dateTime);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        FileModel IOmegaService.GetFile(ApplicationVersion applicationVersion, Guid fileUID)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightChangeBalance(user);
                return _repository.GetFile(fileUID);
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        TransactionAction IOmegaService.UpdateTransactionDate(ApplicationVersion version, TransactionAction transaction, DateTime dateTime)
        {
            return RunSecurity(version, user =>
            {
                CheckRightChangeBalance(user);
                _repository.UpdateTransactionDate(transaction, dateTime, user);
                transaction.TransactionDate = dateTime;
                return transaction;
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        VendorModel[] IOmegaService.GetAllVendors(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, user => _repository.GetAllVendors());
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void IOmegaService.AddVendor(ApplicationVersion applicationVersion, VendorModel vendor)
        {
            RunSecurity(applicationVersion, user =>
            {
                _repository.AddVendor(vendor, user);
                return true;
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void IOmegaService.UpdateVendor(ApplicationVersion applicationVersion, VendorModel vendor)
        {
            RunSecurity(applicationVersion, user =>
            {
                _repository.UpdateVendor(vendor, user);
                return true;
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        UpdateStatusPrioritet IOmegaService.UpdateStatusPrioritet(ApplicationVersion applicationVersion, UpdateStatusPrioritet item)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightsRequests(user);
                _repository.UpdateStatusPrioritet(item, user.ID);
                return item;
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        DeliveryRequestModel[] IOmegaService.GetActDelivery(ApplicationVersion applicationVersion, FilterActDelivery filter)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightsRequests(user);
                return _repository.GetActDelivery(filter);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ChangedStatusRequest IOmegaService.UpdateStatusRequest(ApplicationVersion applicationVersion, UpdateStatusRequest item, RequestRealisation requestRealisation)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightsRequests(user);
                return _repository.UpdateStatusRequest(item, user, requestRealisation);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        RequestModel IOmegaService.UpdateCommentRequest(ApplicationVersion applicationVersion, RequestModel request, String newComment)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightsRequests(user);
                return _repository.UpdateCommentRequest(user, request, newComment);
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        RepairPackageModel[] IOmegaService.GetRepairs(ApplicationVersion applicationVersion, FilterRepairModel filter)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightChangeBalance(user);
                return _repository.GetRepairs(filter);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        void IOmegaService.SaveRepairs(ApplicationVersion applicationVersion, RepairPackageModel[] data, Boolean isStrongSave, String comment)
        {
            RunSecurity(applicationVersion, (user) =>
            {
                CheckRightChangeBalance(user);
                _repository.SaveRepairs(user, data, isStrongSave, comment);
                return data;
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        TransactionReport IOmegaService.GetTransactionReport(ApplicationVersion applicationVersion, FilterTransactionReport filter)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetTransactionReport(filter));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        JobModel[] IOmegaService.GetAllJobModels(ApplicationVersion applicationVersion, OperationEnum value)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetAllJobModels(value));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        RuleStatusRequest[] IOmegaService.GetAllRuleStatusRequests(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, Repository<OmegaEntity>.GetAllRuleStatusRequests);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        CategoryToJobModel[] IOmegaService.GetAllCategoryToJob(ApplicationVersion applicationVersion, CategoryToJobFilterModel filter)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetAllCategoryToJob(filter));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        BalanceModel[] IOmegaService.GetBalanceSC(ApplicationVersion applicationVersion, BalanceFilterModel filter)
        {
            return RunSecurity(applicationVersion, user => _repository.GetBalanceSC(filter));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        SimpleBalance4LookOnlyModel[] IOmegaService.GetBalanceSC(ApplicationVersion applicationVersion, Guid[] sc)
        {
            return RunSecurity(applicationVersion, user => _repository.GetBalanceSC(sc));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        RequestModel[] IOmegaService.GetRequests(ApplicationVersion applicationVersion, RequestFilterModel filter)
        {
            return RunSecurity(applicationVersion, user =>
            {
                CheckRightsRequests(user);
                return _repository.GetRequests(filter, user);
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        SCInfoModel IOmegaService.GetSCInfo(ApplicationVersion applicationVersion, Guid sc_uid)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetSCInfo(sc_uid));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        StatusBalanceModel IOmegaService.GetStatusBalance(ApplicationVersion applicationVersion, SimpleSCModel sc)
        {
            return RunSecurity(applicationVersion, user => _repository.GetStatusBalance(sc));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        BalanceTransactionRepairModel[] IOmegaService.GetBalanceTransactionRepair(ApplicationVersion applicationVersion, BalanceTransactionRepairFilterModel balanceTransactionRepairFilter)
        {
            return RunSecurity(applicationVersion, user => _repository.GetBalanceTransactionRepair(balanceTransactionRepairFilter));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public TownModel[] GetAllTowns(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetAllTowns());
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public BankModel[] GetBanks(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetBanks());
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        SimpleSCModel[] IOmegaService.GetAllSC(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetAllSC(null));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        SimpleSCModel[] IOmegaService.GetAllSCByNOX(ApplicationVersion applicationVersion, int[] listSC, Boolean isNotCheck)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetAllSCByNOX(listSC, !isNotCheck));
        }


        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        CategoryModel[] IOmegaService.GetAllCategories(ApplicationVersion applicationVersion, OperationEnum operation)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetAllCategories(operation));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        JobModel[] IOmegaService.AddJob(ApplicationVersion applicationVersion, JobLookModel[] jobList, OperationEnum operation)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightChangeBalance(user);
                return _repository.AddJob(jobList, operation);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        CategoryModel[] IOmegaService.AddCategory(ApplicationVersion applicationVersion, CategoryLookModel[] newcategoryList, OperationEnum operation)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightChangeBalance(user);
                return _repository.AddCategory(newcategoryList, operation);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        CategoryToJobModel[] IOmegaService.AddJobToCategory(ApplicationVersion applicationVersion, JobModel[] jobs, CategoryModel category)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightChangeBalance(user);
                return _repository.AddJobToCategory(jobs, category);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        CreateBalanceModel[] IOmegaService.SaveDocumentBalance(ApplicationVersion applicationVersion, CreateBalanceModel[] document)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightChangeBalance(user);
                return _repository.SaveDocumentBalance(document, user);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        RSS.WCF.Contracts.OMEGA.Models.FinansistReportModel[] IOmegaService.GetFinansistReport(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightChangeBalance(user);
                return _repository.GetFinansistReport();
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        RequestModel IOmegaService.SaveDocumentRequest(ApplicationVersion applicationVersion, CreateRequestModel document)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightsRequests(user);
                return _repository.SaveDocumentRequest(document, user);
            });
        }

        static T RunSecurity<T>(ApplicationVersion applicationVersion, Func<UserModel, T> action)
        {
            if (applicationVersion == null) throw new ArgumentNullException("Версия приложения неопределена");

            //if (applicationVersion.Number < 10221&&applicationVersion.IsReleaseDb) throw new SecurityAccessDeniedException("Не буду работать. У вас устаревшая версия. Обновитесь");

            if (applicationVersion.IsReleaseDb == data.IsTestDb())
                throw new NotSupportedException(
                    "Внимание!!!\r\nВерсия клиента и сервера не совпадают. Перезапустите приложение. \r\nВы сейчас работаете с версией " +
                    (applicationVersion.IsReleaseDb ? "тестовой" : "рабочей"));

            var curent = OperationContext.Current;
            if (curent == null) throw new ArgumentNullException("Текущий контекст операции не был получен");
            if (curent.ServiceSecurityContext == null || curent.ServiceSecurityContext.WindowsIdentity == null)
                throw new ArgumentNullException("Контекст безопасности не установлен");

            var securiryContext = curent.ServiceSecurityContext.WindowsIdentity;
            var user = Repository<OmegaEntity>.GetUserID(/*securiryContext.Name*/applicationVersion);
            if (!user.Rights.Contains("Runing")) throw new SecurityAccessDeniedException("У вас нет прав на использование этого приложения");

            using (var impersonate = securiryContext.Impersonate())
            {
                return action(user);
            }
        }

        static void CheckRightChangeBalance(UserModel user)
        {
            if (!user.Rights.Contains("BalanceCreate")) throw new SecurityAccessDeniedException("У вас недостаточно прав для использованиЯ данной функции");
        }

        static void CheckRightsRequests(UserModel user)
        {
            if (user.Rights.Contains("SetStatusRequestOpen") || user.Rights.Contains("SetStatusRequestReady")) return;
            throw new SecurityAccessDeniedException("У вас недостаточно прав");
        }

        static void CheckRightsFantom(UserModel user)
        {
            if (user.Rights.Contains("Fantom")) return;
            throw new SecurityAccessDeniedException("У вас недостаточно прав");
        }

        #region FANTOM

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        FantomModel[] IFantomService.GetFantoms(ApplicationVersion applicationVersion, FantomFilterModel filter)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightsFantom(user);
                return _RepositoryFantom.GetFantoms(filter);
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        FantomModel IFantomService.DeleteFantom(ApplicationVersion applicationVersion, FantomModel item)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightsFantom(user);
                return _RepositoryFantom.DeleteFantom(item);
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        FantomModel IFantomService.UpdateFantom(ApplicationVersion applicationVersion, FantomModel item)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightsFantom(user);
                return _RepositoryFantom.UpdateFantom(item, user);
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        FantomModel IFantomService.CreateFantom(ApplicationVersion applicationVersion, CreateFantomModel item)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                CheckRightsFantom(user);
                return _RepositoryFantom.CreateFantom(item, user);
            });
        }

        #endregion

        #region СЧЕТА

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ReferenceModel[] IAccountService.GetServices(ApplicationVersion applicationVersion, FilterServiceModel filter)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetService(filter));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountModel[] IAccountService.GetAccounts(ApplicationVersion applicationVersion, AccountFilterModel filter)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetAccounts(filter));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        IEnumerable<AccountBillModel> IAccountService.GetAccountBills(ApplicationVersion applicationVersion, AccountBillFilterModel filter)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetAccountBills(filter));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountModel IAccountService.CreateAccount(ApplicationVersion applicationVersion, AccountRowModel data)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.CreateAccount(data, user));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ClientFinanceInfo[] IAccountService.GetClients(ApplicationVersion applicationVersion, FilterClientModel typeRecipient)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetClients(typeRecipient));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ReferenceModel[] IAccountService.GetServices(ApplicationVersion applicationVersion, FilterCreateAccount filter)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetService(filter));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountBillModel IAccountService.CreateAccountBill(ApplicationVersion applicationVersion, String accountNumber, DateTime dateAccountBill, IEnumerable<AccountBillLineRowModel> accountBillLines)
        {
            if (dateAccountBill == null) throw new ArgumentNullException("Дата с/фактуры не была получена\r\nНе могу создать счет/фактуру");
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.CreateAccountBill(accountNumber, user, dateAccountBill, accountBillLines));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        PaymentModel IAccountService.CreatePayment(ApplicationVersion applicationVersion, PaymentRowModel payment, FileModel file)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                if (!user.Rights.Contains("CreatePayment", StringComparer.InvariantCultureIgnoreCase))
                    throw new SecurityAccessDeniedException("У вас нет прав создать оплату");
                return _RepositoryAccount.CreatePayment(payment, file, user);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        PaymentLookModel IAccountService.UpdatePayment(ApplicationVersion applicationVersion, PaymentLookModel payment)
        {
            return RunSecurity(applicationVersion, (user) =>
            {
                if (!user.Rights.Contains("CreatePayment", StringComparer.InvariantCultureIgnoreCase))
                    throw new SecurityAccessDeniedException("У вас нет прав изменять оплату");
                return _RepositoryAccount.UpdatePayment(payment, user);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        void IAccountService.CreatePayments(ApplicationVersion applicationVersion, PaymentFrom1CModel[] data)
        {
            RunSecurity(applicationVersion, (user) =>
            {
                if (!user.Rights.Contains("CreatePayment", StringComparer.InvariantCultureIgnoreCase))
                    throw new SecurityAccessDeniedException("У вас нет прав создать оплату");
                _RepositoryAccount.CreatePayments(data, user);
                return true;
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        void IAccountService.DeletePayment(ApplicationVersion applicationVersion, Guid paymentUID)
        {
            RunSecurity(applicationVersion, (user) =>
            {
                if (!user.Rights.Contains("CreatePayment", StringComparer.InvariantCultureIgnoreCase))
                    throw new SecurityAccessDeniedException("У вас нет прав удалять оплату");
                _RepositoryAccount.DeletePayment(paymentUID, user);
                return true;
            });
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        PaymentLookModel[] IAccountService.GetPayments(ApplicationVersion applicationVersion, FilterPaymentModel filter)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetPayments(filter));
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountForPrintModel IAccountService.GetAccountForPrintModel(ApplicationVersion applicationVersion, Guid accountHeader)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetAccountForPrintModel(accountHeader));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ReferenceModel IAccountService.CreateService(ApplicationVersion applicationVersion, ReferenceRowModel referenceRow)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.CreateService(referenceRow));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ClientFinanceInfo IAccountService.CreateClient(ApplicationVersion applicationVersion, ClientFinanceInfoRow data)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.CreateClient(data, user));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        ClientFinanceUpdater IAccountService.UpdateClient(ApplicationVersion applicationVersion, ClientFinanceUpdater data)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.UpdateClient(data, user));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        FormModel[] IAccountService.GetForms(ApplicationVersion applicationVersion)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.GetForms());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        void IAccountService.DeleteAccount(ApplicationVersion applicationVersion, Guid accountGuid)
        {
            RunSecurity(applicationVersion, (user) =>
            {
                if (!user.Rights.Contains("CreatePayment", StringComparer.InvariantCultureIgnoreCase))
                    throw new NotSupportedException("Функция удаления счета вам недоступна.");

                _RepositoryAccount.DeleteAccountWithAccountBill(accountGuid);
                return true;
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountModel IAccountService.ChangeDateAccount(ApplicationVersion applicationVersion, AccountModel account, DateTime date)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.ChangeDateAccount(account, date, user));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountModel IAccountService.ChangeBank(ApplicationVersion applicationVersion, AccountModel account, BankModel newBank)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.ChangeBank(account, newBank, user));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountModel IAccountService.UpdateAccountLines(ApplicationVersion applicationVersion, AccountModel account)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.UpdateAccountLines(account, user));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountBillModel IAccountService.UpdateAccountBillLines(ApplicationVersion applicationVersion, AccountBillModel account)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.UpdateAccountBillLines(account, user));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        AccountModel IAccountService.ChangeDateAccountBill(ApplicationVersion applicationVersion, AccountModel account, DateTime date)
        {
            return RunSecurity(applicationVersion, (user) => _RepositoryAccount.ChangeDateAccountBill(account, date, user));
        }

        #endregion
        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public RepairsPaymentModel[] GetRepairsPaymentReport(ApplicationVersion applicationVersion, RepairsPaymentFilterModel filter)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetRepairsPaymentReport(filter));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public RetentionFullInfo ChangeRetentionInfo(ApplicationVersion applicationVersion, RetentionFullInfo data, Guid H_UID)
        {
            return RunSecurity(applicationVersion, (user) => _repository.ChangeRetentionInfo(data, H_UID));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public ProfitFullInfo ChangeProfitInfo(ApplicationVersion applicationVersion, ProfitFullInfo data, Guid H_UID)
        {
            return RunSecurity(applicationVersion, (user) => _repository.ChangeProfitInfo(data, H_UID));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public WorkFullInfo ChangeWorkInfo(ApplicationVersion applicationVersion, WorkFullInfo data, Guid H_UID)
        {
            return RunSecurity(applicationVersion, (user) => _repository.ChangeWorkInfo(data, H_UID));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public RetentionFullInfo DeleteRetention(ApplicationVersion applicationVersion, RetentionFullInfo data, Guid H_UID)
        {
            return RunSecurity(applicationVersion, (user) => _repository.DeleteRetenetion(data, H_UID));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public ProfitFullInfo DeleteProfit(ApplicationVersion applicationVersion, ProfitFullInfo data, Guid H_UID)
        {
            return RunSecurity(applicationVersion, (user) => _repository.DeleteProfit(data, H_UID));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public WorkFullInfo DeleteWork(ApplicationVersion applicationVersion, WorkFullInfo data, Guid H_UID)
        {
            return RunSecurity(applicationVersion, (user) => _repository.DeleteWork(data, H_UID));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public IEnumerable<VendorInfo> GetVendorInfo(ApplicationVersion applicationVersion, String vendorID)
        {
            return RunSecurity(applicationVersion, (user) => _repository.GetVendorInfo(vendorID));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public VendorInfo ChangeVendorInfo(ApplicationVersion applicationVersion, VendorInfo vendorInfo, DateTime? date)
        {
            return RunSecurity(applicationVersion, (user) => _repository.ChangeVendorInfo(vendorInfo, date, user));
        }
    }
}
