using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StepCoin_v1
{
    public class BlockChain
    {
        public LinkedList<Block> MyChain { get; } = new LinkedList<Block>();
        public Hashtable Transactions { get; set; } = new Hashtable();
        public int Difficulty =4;
        //public List<Block> PendingBlocks { get; set; } = new List<Block>();
        public BlockChain()
        {
            MyChain.AddLast(CreateBlockZero());
        }
        public Block CreateBlockZero()
        {
            List<Transaction> list = new List<Transaction>();
            return new Block
            {
                Timestamp = DateTime.UtcNow.Ticks,
                ThisBlockTransactions = list,
                PreviousHash = "00",
                ThisHash = "00000"
            };
        }

        /// <summary>
        /// Метод, который создаст транзакцию на 100 монет новому эккаунту и создаст соответствующий блок
        /// и добавит его с блокчейн
        /// </summary>
        /// <param name="newAccount"></param>
        public void SendMoneyToNewAccountBlock(Account newAccount)
        {
            Transaction newTransaction = new Transaction("101010", newAccount.Address, 100.00m);
            List<Transaction> pendingTransaction = new List<Transaction>();
            pendingTransaction.Add(newTransaction);
            TransactionsValidator defaultTransactionValidator = new TransactionsValidator
            {
                PendingTransactions = pendingTransaction
            };
            Miner defaultMiner = new Miner(this, defaultTransactionValidator);
            

            Block newBlock = defaultMiner.CreateNewBlock();
            AddBlock(newBlock);
        }


        /// <summary>
        /// Метод добавляющий и блок и транзакции из этого блока
        /// </summary>
        /// <param name="newBlock"></param>
        public void AddBlock (Block newBlock)
        {          
            MyChain.AddLast(newBlock);
            foreach (Transaction instance in newBlock.ThisBlockTransactions)
            {
                string key = instance.CalculateTransactionHash();
                //if (Transactions.ContainsKey(key))
                //{
                    Transactions.Add(key, instance);
                //}
            }               
        }

        public Block GetLastBlock()
        {
            return MyChain.Last();
        }
    }
}