using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepCoin_v1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Transaction N = new Transaction("Sender", "Beneficiar", 25.00m);
            //Console.WriteLine(N.ToString());

            //string newRandomName = "";
            //Random random = new Random();
            //for (int i = 0; i < 10; i++)
            //{
            //    char rndLetter = (char)random.Next(97, 122);
            //    newRandomName += rndLetter;
            //}
            //Console.WriteLine(newRandomName);

            //char id = 'A';
            //char id2 = (char)(++id);
            //Console.Write(id2);

            BlockChain blockChain_node1 = new BlockChain();
            BlockChain blockChain_node2 = new BlockChain();
            TransactionsValidator transValidator_node1 = new TransactionsValidator();
            TransactionsValidator transValidator_node2 = new TransactionsValidator();
            AccountList accountList_node1 = new AccountList();
            AccountList accountList_node2 = new AccountList();
            Validator blockValidator_node1 = new Validator();
            Validator blockValidator_node2 = new Validator();
            blockValidator_node1.CopyOfBlockChain = blockChain_node1;
            blockValidator_node1.CopyOfAccountList = accountList_node1;
            blockValidator_node2.CopyOfAccountList = accountList_node2;
            blockValidator_node2.CopyOfBlockChain = blockChain_node2;
            Miner miner_node1 = new Miner(blockChain_node1, transValidator_node1);
            Miner miner_node2 = new Miner(blockChain_node2, transValidator_node2);

            TestNode node_A = new TestNode(accountList_node1, transValidator_node1, blockChain_node1, blockValidator_node1, miner_node1);
            TestNode node_B = new TestNode(accountList_node2, transValidator_node2, blockChain_node2, blockValidator_node2, miner_node2);
            node_A.PeerNode = node_B;
            node_B.PeerNode = node_A;

            Console.WriteLine("Проверка создания тестовых узлов, выведет буквенный ID");
            Console.WriteLine(node_A.GetId());
            Console.WriteLine(node_B.GetId());
            Console.WriteLine("==========================");

            node_A.RegisterAccount();
            node_B.RegisterAccount();
            node_A.AddAccount(node_A.ThisNodeAccount);
            node_B.AddAccount(node_B.ThisNodeAccount);

            Console.WriteLine("Проверка синхронизации списка эаккаунтов");
            Console.WriteLine(node_A.GetId());
            Console.WriteLine(node_A.GeneralAccountList.ToString());
            Console.WriteLine("==========================");

            Console.WriteLine(node_B.GetId());
            Console.WriteLine(node_B.GeneralAccountList.ToString());
            Console.WriteLine("==========================");

            Console.WriteLine("Проверка подтверждения транзацции и синхронизации списка подтвержденных транзакций");
            Console.WriteLine("Каждый из узлов генерирует по одной транзакции, оба узла должны их подтвердить и");
            Console.WriteLine("добавить в список подтвержденных транзакций:");
            node_A.TransactionSynch();
            node_B.TransactionSynch();
            Console.WriteLine(node_A.GetId());
            foreach(Transaction instance in node_A.ConfirmedTransactions)
            {
                Console.WriteLine(instance.ToString());
            }

            Console.WriteLine(node_B.GetId());
            foreach (Transaction instance in node_B.ConfirmedTransactions)
            {
                Console.WriteLine(instance.ToString());
            }
            Console.WriteLine("==========================");

            node_A.AddConfirmedBlockToPendingBlocks();
            node_B.AddConfirmedBlockToPendingBlocks();
            Console.WriteLine("Проверка создан ли корректно стартовый блок. Выведет последний блок ToString:");
            Console.WriteLine(node_A.GeneralBlockChain.GetLastBlock().ToString());
            Console.WriteLine("==========================");
            Console.WriteLine("Проверка двух намайненных блоков - по одному из каждого узла:");
            foreach(Block block in node_A.PendingConfirmedBlocks)
            {
                Console.WriteLine(block.ToString());
            }
            Console.WriteLine("==========================");
            node_A.SelectAndAddBlockToBlockChain();
            Console.WriteLine("Проверка правильно ли выбран блок для добавления в блокчейн. Выведет последний блок");
            Console.WriteLine(node_A.GeneralBlockChain.GetLastBlock().ToString());
            Console.WriteLine("==========================");
            Console.WriteLine("Проверка правильности блокчейна - true/false");
            Console.WriteLine(node_A.GeneralValidator.IsChainValid());
            Console.WriteLine(node_B.GeneralValidator.IsChainValid());
            Console.WriteLine("==========================");
            Console.WriteLine("Проверка зачисления денег - метод Account.Balance():");
            foreach(Account instance in node_A.GeneralAccountList.ListOfAllAccounts)
            {
                Console.Write(instance.ToString());
                Console.WriteLine(instance.Balance().ToString());
            }

            //foreach(Block instance in node_A.GeneralBlockChain.MyChain)
            //{
            //    Console.WriteLine(instance.ToString());
            //}
            foreach(Transaction instance in node_A.GeneralBlockChain.GetLastBlock().ThisBlockTransactions)
            {
                Console.WriteLine(instance.ToString());
            }
            Console.WriteLine("==========================");
            foreach (DictionaryEntry instance in node_B.GeneralBlockChain.Transactions)
            {
                Console.WriteLine(instance.Value);
            }

        }
    }
}
