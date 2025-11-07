using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Transactions;

namespace RJPAccounts.Models
{
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public string? customerID { get; set; }
        public decimal balance { get; set; } = 0;
        public DateTime? createdAt { get; set; }
    }

    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        [JsonIgnore]
        public string accountID { get; set; }
        public decimal amount { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class JoinedOutput
    {
        
        public string id { get; set; }
        public string customerID {  get; set; }
        public decimal balance { get; set; } = 0;
        public DateTime? createdAt { get; set; }
        public List<Transaction> Transactions { get; set; }
    }

    public class Amount  //Added for the post transaction to be able to send only the amount field as follow { "amount": 50.0 }
    {
        public decimal amount { get; set; }
    }

    public class newAccountInput
    {
        public string? customerID { get; set; }
        public decimal initialCredit { get; set; } = 0;
    }

    public class newAccountOutput //Added to display custom output when adding a new current account
    {
        public string accountid { get; set; }
        public string customerID { get; set; }
        public decimal balance { get; set; }
        public DateTime? createdAt { get; set; }
        public Transaction initialtransaction { get; set; }
    }
    //Get Customer payload
    public class Customer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public string? name { get; set; }
        public string surname { get; set; }
        public DateTime? createdAt { get; set; }
    }
    //Send updated balance to Customer API
    public class SendBalance
    {
        public string customerid {  get; set; }
        public decimal balance { get; set; }
    }
}
