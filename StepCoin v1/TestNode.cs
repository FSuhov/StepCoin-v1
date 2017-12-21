using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StepCoin_v1
{
    public class TestNode
    {
        static int id_Counter = 64;
        static decimal differentiation_amount = 1.00m;
        public char ID { get; set; }
        public Account ThisNodeAccount { get; set; }
        public AccountList GeneralAccountList { get; set; }
        public TransactionsValidator GeneralTransactionValidator { get; set; }
        public BlockChain GeneralBlockChain { get; set; }
        public Validator GeneralValidator { get; set; }
        public Miner ThisNodeMiner { get; set; }
        public TestNode PeerNode { get; set; }
        public List<Block> PendingConfirmedBlocks { get; set; } = new List<Block>();
        //public event EventHandler NewTransaction;
        //public event EventHandler StartMining;
        //public List<Transaction> UnconfirmedTransactions { get; set; } = new List<Transaction>();
        public List<Transaction> ConfirmedTransactions { get; set; } = new List<Transaction>();


        public TestNode( AccountList accountList,
                         TransactionsValidator transValidator,
                         BlockChain blockChain,
                         Validator blockValidator,
                         Miner miner)
        {
            GeneralAccountList = accountList;
            GeneralTransactionValidator = transValidator;
            GeneralBlockChain = blockChain;
            GeneralValidator = blockValidator;
            ThisNodeMiner = miner;
            ID = (char)++id_Counter;
            //ThisNodeAccount = CreateAccount();
        }
        /// <summary>
        /// Метод создания эаккаунта. Рэндомно генерирует имя из 8 букв, 
        /// затем на основании имени считает адрес (алгоритм MD5), и на основаниии адреса
        /// создает секретные ключ (SHA256).Использует два вспомогательных метода
        /// GenerateAddress и GenerateSecretKey.
        /// </summary>
        private Account CreateAccount()
        {
            string name = RandomName(8);
            string address = GenerateAddress(name);
            string key = GenerateSecretKey(address);
            Account newAccount = new Account
            {
                Address = address,
                SecretKey = key
            };
            return newAccount;           
        }
        /// <summary>
        /// Присваивает эккаунт данному узлу
        /// </summary>
        /// <param name="someAccount"></param>
        public void RegisterAccount()
        {
            ThisNodeAccount = CreateAccount();
        }

        /// <summary>
        /// Метод, вызывающий добавление эккаунта в классе списка всех эккаунтов. И если эккаунт был в том 
        /// списке добавлен (то есть еще не существовал до этого), то вызывает такой же метод в
        /// соседнем узле
        /// </summary>
        /// <param name="someAccount"></param>
        public void AddAccount (Account someAccount)
        {
            bool isListOfAccountsUpdated = GeneralAccountList.AddNewAccount(someAccount);
            if (isListOfAccountsUpdated) PeerNode.GeneralAccountList.AddNewAccount(someAccount);
        }

        /// <summary>
        /// Метод, создающий транзакцию от этого узла к соседнему и возвращающий ее
        /// </summary>
        /// <param name="amount" ></param>- сумма, сколько хотим отправаить
        /// <returns></returns>
        public Transaction CreateNewTransaction(decimal amount)
        {
            Transaction transaction = new Transaction(ThisNodeAccount.Address, PeerNode.ThisNodeAccount.Address, amount);
            return transaction;
        }

        /// <summary>
        /// Метод валидации транзакции. Если правильная, то инкременитрует количество подтверждений в поле данной транзакции
        /// </summary>
        /// <param name="someTransaction"></param>
        public void ValidateTransaction (Transaction someTransaction)
        {
            if(GeneralTransactionValidator.IsValidTransaction(someTransaction, GeneralBlockChain.Transactions, GeneralAccountList.ListOfAllAccounts))
            {
                someTransaction.ConfirmationsCount++;
            }
        }

        public void ValidateBlock(Block someBlock)
        {
            GeneralValidator.NewlyCreatedBlock = someBlock;
            if(GeneralValidator.IsBlockValid())
            {
                someBlock.ConfirmationsCount++;
            }
        }
        /// <summary>
        /// Метод, создающий транзакцию, подтверждающий ее и добавляющий в список подтвержденных в этом и соседнем узлах
        /// </summary>
        public void TransactionSynch()
        {
            Transaction newTransaction = CreateNewTransaction(10.00m + (differentiation_amount++));
            PeerNode.ValidateTransaction(newTransaction);
            ValidateTransaction(newTransaction);
            if (newTransaction.ConfirmationsCount > 1) newTransaction.IsConfirmed = true;
            if (newTransaction.IsConfirmed == true)
            {
                ConfirmedTransactions.Add(newTransaction);
                PeerNode.ConfirmedTransactions.Add(newTransaction);
            }
        }

        public Block MineBlock()
        {
            Block newBlock = null;
            if(ConfirmedTransactions.Count!=0)
            {
                //GeneralTransactionValidator.PendingTransactions = ConfirmedTransactions;
                foreach(Transaction instance in ConfirmedTransactions)
                {
                    GeneralTransactionValidator.PendingTransactions.Add(instance);
                }
                ConfirmedTransactions.Clear();
                newBlock = ThisNodeMiner.CreateNewBlock();
            }
            return newBlock;
        }
        /// <summary>
        /// Метод, майнит блок, подтверждает его у соседа и у себя и если подтвержден, то добавит блок 
        /// в предварительный список блоков блокчейна для последующего выбора
        /// блока, который будет присоединен (на основании времени майнинга)
        /// </summary>
        public void AddConfirmedBlockToPendingBlocks()
        {
            Block newBlock = MineBlock();
            if (newBlock != null)
            {
                PeerNode.ValidateBlock(newBlock);
                ValidateBlock(newBlock);
                if(newBlock.ConfirmationsCount>1)
                {
                    newBlock.IsConfirmed = true;
                }
                if(newBlock.IsConfirmed)
                {
                    PendingConfirmedBlocks.Add(newBlock);
                    PeerNode.PendingConfirmedBlocks.Add(newBlock);
                }
            }
        }
        /// <summary>
        /// Метод, который выберет самый давний по времени создания блок из пендинг
        /// добавит его в блокчейн и распределит деньги по счетам,
        /// а также синхронизирует эти операции с соседом
        /// </summary>
        public void SelectAndAddBlockToBlockChain()
        {
            if(PendingConfirmedBlocks.Count>0)
            {
                int index = -1;
                long earliestCreationTime = DateTime.UtcNow.Ticks;
                for(int i = 0; i < PendingConfirmedBlocks.Count; i++)
                {
                    if(PendingConfirmedBlocks[i].Timestamp < earliestCreationTime)
                    {
                        earliestCreationTime = PendingConfirmedBlocks[i].Timestamp;
                        index = i;
                    }
                }
                GeneralValidator.NewlyCreatedBlock = PendingConfirmedBlocks[index];
                PeerNode.GeneralValidator.NewlyCreatedBlock = PeerNode.PendingConfirmedBlocks[index];
                PendingConfirmedBlocks.Clear();
                PeerNode.PendingConfirmedBlocks.Clear();
                GeneralValidator.AddNewBlock();
                PeerNode.GeneralValidator.AddNewBlock();
            }
        }

        private string RandomName(int numberOfSymbols)
        {
            string newRandomName = "";
            Random random = new Random();
            for(int i = 0; i < numberOfSymbols; i++)
            {
                char rndLetter = (char)random.Next(97, 122);
                newRandomName += rndLetter;
            }
            return newRandomName;
        }

        private string GenerateAddress(string name)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.Unicode.GetBytes(name);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private string GenerateSecretKey(string address)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] addressHash = hash.ComputeHash(enc.GetBytes(address));
                foreach (Byte b in addressHash)
                {
                    sb.Append(b.ToString("x2"));
                }
            }
            return sb.ToString();
        }

        public char GetId() => ID;
    }
}