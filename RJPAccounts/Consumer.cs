using MassTransit;
using RJPAccounts.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using RJPBalanceToUpdate;
using RJPAccounts.Services;

public class NewAccountConsumer : IConsumer<NewAccount>
{
    private readonly AccountsService _accountsService;

    public NewAccountConsumer(AccountsService accountsService)
    {
        _accountsService = accountsService;
    }
    public async Task Consume(ConsumeContext<NewAccount> context)
    {
        var jsonMessage = JsonSerializer.Serialize(context.Message);

        var customerID = context.Message.customerid;

        Account newAccount = new();
        newAccount.customerID=customerID;
        newAccount.createdAt=DateTime.Now;

        await _accountsService.CreateAsync(newAccount);
        Console.WriteLine(context.Message);
        Console.WriteLine($"New Account Created: {context.Message.customerid}");
        //return Task.CompletedTask;
    }
}
