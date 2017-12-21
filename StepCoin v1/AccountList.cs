using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StepCoin_v1
{
    public class AccountList
    {
        public List<Account> ListOfAllAccounts { get; set; } = new List<Account>();

        public bool AddNewAccount(Account someAccount)
        {            
            foreach(Account instance in ListOfAllAccounts)
            {
                if(someAccount.Address == instance.Address)
                {
                    return false;
                }
            }
            ListOfAllAccounts.Add(someAccount);
            return true;            
        }
        /// <summary>
        /// !!!Здесь есть проблема: при тестировании транзакции задваивались в ListOfAllAccounts. В данный
        /// момент проблема решена введением проверки на существование такой же транзакции, но 
        /// это не правильно. Я не нашел причину по которой они задваиваиюися. 
        /// Если причина так и не будет найдена, то уж лучше будет делать не List для Outcoming и Incoming транзакций
        /// а тоже Hashtable (как и в классе BlockChain) и сравнивать хэши транзакций (ключи), 
        /// которые должны будут также включать и время совершения транзации!!!
        /// </summary>
        /// <param name="someTransaction"></param>
        public void DepositeWithdraw(Transaction someTransaction)
         {
            foreach(Account instance in ListOfAllAccounts)
            {
                if(someTransaction.Sender == instance.Address && !instance.OutcomingTransactions.Contains(someTransaction))
                {
                    instance.OutcomingTransactions.Add(someTransaction);
                }
                if(someTransaction.Recipient == instance.Address && !instance.IncomingTransactions.Contains(someTransaction))
                {
                    instance.IncomingTransactions.Add(someTransaction);
                }
            }
        }

        public override string ToString()
        {
            string accounts = "";
            foreach(Account instance in ListOfAllAccounts)
            {
                accounts += instance.ToString();
            }
            return accounts;
        }
    }
}