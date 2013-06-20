﻿using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Domain
{
    [Serializable]
    public class BankAccount : AggregateRoot<Guid>,
        IEventHandler<AccountOpened>,
        IEventHandler<Deposited>,
        IEventHandler<TransferedOut>,
        IEventHandler<TransferedIn>,
        IEventHandler<TransferOutRollbacked>
    {
        public string AccountNumber { get; private set; }
        public string Customer { get; private set; }
        public double Balance { get; private set; }

        public BankAccount() : base() { }
        public BankAccount(Guid accountId, string accountNumber, string customer) : base(accountId)
        {
            RaiseEvent(new AccountOpened(Id, accountNumber, customer));
        }

        public void Deposit(double amount)
        {
            RaiseEvent(new Deposited(Id, amount, string.Format("向账户{0}存入金额{1}", AccountNumber, amount)));
        }
        public void TransferOut(BankAccount targetAccount, Guid processId, TransferInfo transferInfo)
        {
            if (Balance < transferInfo.Amount)
            {
                throw new Exception(string.Format("账户{0}余额不足，不能转账！", AccountNumber));
            }
            RaiseEvent(new TransferedOut(processId, transferInfo, string.Format("{0}向账户{1}转出金额{2}", AccountNumber, targetAccount.AccountNumber, transferInfo.Amount)));
        }
        public void TransferIn(BankAccount sourceAccount, Guid processId, TransferInfo transferInfo)
        {
            RaiseEvent(new TransferedIn(processId, transferInfo, string.Format("{0}从账户{1}转入金额{2}", AccountNumber, sourceAccount.AccountNumber, transferInfo.Amount)));
        }
        public void RollbackTransferOut(Guid processId, TransferInfo transferInfo)
        {
            RaiseEvent(new TransferOutRollbacked(processId, transferInfo, string.Format("账户{0}取消转出金额{1}", AccountNumber, transferInfo.Amount)));
        }

        void IEventHandler<AccountOpened>.Handle(AccountOpened evnt)
        {
            AccountNumber = evnt.AccountNumber;
            Customer = evnt.Customer;
        }
        void IEventHandler<Deposited>.Handle(Deposited evnt)
        {
            Balance += evnt.Amount;
        }
        void IEventHandler<TransferedOut>.Handle(TransferedOut evnt)
        {
            Balance -= evnt.TransferInfo.Amount;
        }
        void IEventHandler<TransferedIn>.Handle(TransferedIn evnt)
        {
            Balance += evnt.TransferInfo.Amount;
        }
        void IEventHandler<TransferOutRollbacked>.Handle(TransferOutRollbacked evnt)
        {
            Balance += evnt.TransferInfo.Amount;
        }
    }
}