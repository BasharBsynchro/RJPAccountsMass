using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RJPAccounts.Models;

using RJPAccounts.Services;
using System.Security.Principal;

namespace RJPAccounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountsService _accountsService;
        private readonly TransactionsServices _transactionsService;
        

        
        public AccountsController(AccountsService accountsService, TransactionsServices transactionsServices)
        {
            _accountsService = accountsService;
            _transactionsService = transactionsServices;
            
        }

 

        //Search All exisiting accounts (added For testing)
        [HttpGet]   
        public async Task<List<Account>> Get() =>
        await _accountsService.GetAsync();

        //Add new account
        [HttpPost("/accounts")]
        public async Task<IActionResult> CreateCurrentAccount(newAccountInput inputAccount)
        {
            //get values from custom input
            var customerid = inputAccount.customerID;
            var initialCredits = inputAccount.initialCredit;


            Account newcurrent = new Account();
            newcurrent.customerID = customerid;
            newcurrent.createdAt=DateTime.UtcNow;

            await _accountsService.CreateAsync(newcurrent);

            //Create custom output
            newAccountOutput createdact = new newAccountOutput();
            createdact.accountid = newcurrent.id;
            createdact.customerID = customerid;
            
            createdact.createdAt = newcurrent.createdAt;
            
            // If initialcredit > 0 create new transaction as well
            if (initialCredits > 0)
            {
                Transaction newt = new Transaction();
                newt.amount = initialCredits;
                newt.createdAt = DateTime.UtcNow;

                var act = await _accountsService.GetAsync(newcurrent.id);
                newt.accountID = act.id;

                await _transactionsService.CreateTran(newt);

                // Update account balance
                act.balance += initialCredits;
                await _accountsService.UpdateAsync(act.id, act);

                //Custom output if transaction exist
                createdact.initialtransaction = newt;
                createdact.balance = act.balance;

            }
            else if (initialCredits < 0) 
            {
                return BadRequest("Amount value cannot be less than 0!!!"); //Validation as requested, where 400 code is thrown when initialcredit is less than 0
            }

            

            return CreatedAtAction(nameof(Get), createdact);
        }

        //Add new transaction
        [HttpPost("/accounts/{accountid}/transactions")] 
        public async Task<IActionResult> AddNewTransaction(string accountid,[FromBody] Amount amountbody) {
            var amount = amountbody.amount;
            
            //Add new transaction
            Transaction newt = new Transaction();
            newt.accountID=accountid;
            newt.amount = amount;
            newt.createdAt = DateTime.UtcNow;
            await _transactionsService.CreateTran(newt);

            //Update account's balance
            var act = await _accountsService.GetAsync(accountid);
            decimal currbalance = act.balance;
            decimal newbalance = currbalance + amount;
            act.balance = newbalance;
            await _accountsService.UpdateAsync(act.id, act);

            //Send updated balnace to Customer API using MassTransit
            await _accountsService.SendBalance(act.id);

            //Old RabbitMQ send updated balance
            //Send updated balance via RabbitMQ
            //SendBalance sendBalance = new SendBalance();
            //sendBalance.balance = newbalance;
            //sendBalance.customerid = act.customerID;
            //await _accountsService.SendNewBalance(sendBalance);
            

            return CreatedAtAction(nameof(Get), newt);
        }

        //Search account by id
        [HttpGet("/accounts/{accountID}")]
        public async Task<ActionResult<JoinedOutput>> GetTransactions(string accountID)
        {
            
            var transactionslist = await _transactionsService.GetTransactions(accountID);
            var account = await _accountsService.GetAsync(accountID);

            var list = transactionslist.OrderByDescending(p => p.createdAt).ToList();

            JoinedOutput data = new JoinedOutput();
            data.id = accountID;
            data.customerID = account.customerID;
            data.balance = account.balance;
            data.createdAt = account.createdAt;
            data.Transactions = list;
            
            return data;

        }
    }
}
