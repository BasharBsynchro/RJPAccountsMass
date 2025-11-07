using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RJPAccounts.Models;
using RJPProject.Models;

namespace RJPAccounts.Services
{
    public class TransactionsServices
    {
        private readonly IMongoCollection<Transaction> _TransactionCollection;

        public TransactionsServices (IOptions<TransactionsTblSettings> transactionsTableSettings)
        {
            var mongoClient = new MongoClient(
            transactionsTableSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                transactionsTableSettings.Value.DatabaseName);

            _TransactionCollection = mongoDatabase.GetCollection<Transaction>(
                transactionsTableSettings.Value.BooksCollectionName);
        }
        
        
        public async Task CreateTran(Transaction newtransaction) =>
        await _TransactionCollection.InsertOneAsync(newtransaction);

        public async Task<List<Transaction?>> GetTransactions(string accountID)
        {
            var filter = Builders<Transaction>.Filter.Eq(s => s.accountID, accountID);
            var transactionslist = await _TransactionCollection.Find(filter).ToListAsync();
            return transactionslist;
        }
    }
}
