using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RJPAccounts.Models;
using RJPProject.Models;
using RJPBalanceToUpdate;

namespace RJPAccounts.Services
{
    public class AccountsService
    {
        private readonly IMongoCollection<Account> _AccountCollection;
        private readonly IMongoCollection<Transaction> _TransactionCollection;
        private readonly IPublishEndpoint _publishEndpoint;
        

        public AccountsService (IOptions<AccountsTblSettings> accountsTableSettings, IPublishEndpoint publishEndpoint)
        {
            var mongoClient = new MongoClient(
            accountsTableSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                accountsTableSettings.Value.DatabaseName);

            _AccountCollection = mongoDatabase.GetCollection<Account>(
                accountsTableSettings.Value.BooksCollectionName);

            _publishEndpoint = publishEndpoint;
        }
        
        public async Task<List<Account>> GetAsync() =>
        await _AccountCollection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(Account newAccount) =>
        await _AccountCollection.InsertOneAsync(newAccount);

        public async Task<Account?> GetAsync(string id) =>
        await _AccountCollection.Find(x => x.id == id).FirstOrDefaultAsync();

        public async Task UpdateAsync(string id, Account updatedAccount) =>
        await _AccountCollection.ReplaceOneAsync(x => x.id == id, updatedAccount); 

        public async Task SendBalance(string id)
        {
            var account = await GetAsync(id);

            if(account is not null)
            {
                await _publishEndpoint.Publish<NewBalance>(new
                {
                    account.customerID,
                    account.balance
                });
            }

        }
    }
}
