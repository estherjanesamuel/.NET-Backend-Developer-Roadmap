using System.Net;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;

var builder = WebApplication.CreateBuilder();
builder.Logging
    .AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning)
    .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
builder.Host.UseOrleans(builder => {
    builder
        .UseLocalhostClustering()
        .AddMemoryGrainStorageAsDefault()
        .UseTransactions()
        .Configure<ClusterOptions>(opt => {
            opt.ClusterId = "dev";
            opt.ServiceId = "http-client";
        })
        .Configure<EndpointOptions>(opt => opt.AdvertisedIPAddress = IPAddress.Loopback)
        .ConfigureApplicationParts(parts => parts
            .AddApplicationPart(typeof(AtmGrain).Assembly).WithReferences()
            .AddApplicationPart(typeof(IAtmGrain).Assembly).WithReferences()
            .AddApplicationPart(typeof(AccountGrain).Assembly).WithReferences()
            .AddApplicationPart(typeof(IAccountGrain).Assembly).WithReferences());
});

var app = builder.Build();

app.MapGet("/", async context =>
{
    var accountNames = new[] {"Xaawo", "Pasqualino", "Derick", "Ida", "Stacy", "Xiao" };
    var random = Random.Shared;

    IGrainFactory client = context.RequestServices.GetService<IGrainFactory>()!;
    var atm = client.GetGrain<IAtmGrain>(0);
    var fromId = random.Next(accountNames.Length);
    var toId = random.Next(accountNames.Length);
    while (toId == fromId)
    {
        // Avoid transfering to/from the same account,
        // Since it would be meaningless Except when you use cash deposit ATM
        toId = (toId + 1) % accountNames.Length;
    }

    var fromName = accountNames[fromId];
    var toName = accountNames[toId];
    var from = client.GetGrain<IAccountGrain>(fromName);
    var to = client.GetGrain<IAccountGrain>(toName);

    try
    {
        // Performs the transfer and query the results
        await atm.Transfer(from,to,100);

        var fromBalance = await from.GetBalance();
        var toBalance = await to.GetBalance();

        //  $"We transfered 100 credits from {fromName} to " +
        //     $"{toName}.\n{fromName} balance: {fromBalance}\n{toName} balance: {toBalance}\n");
    
        await context.Response.WriteAsync(@"<html><head><link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/uikit@3.5.5/dist/css/uikit.min.css"" /></head>");
        await context.Response.WriteAsync("<body>");
        await context.Response.WriteAsync("Orleans Bank.<br>");
        await context.Response.WriteAsync($"<li>There is transfered 100 credits from {fromName} to {toName}.\n{fromName} balance: {fromBalance}\n{toName} balance: {toBalance}\n</li>");
        await context.Response.WriteAsync("<ul>");
        foreach (var name in accountNames)
        {
            var accountBalance = client.GetGrain<IAccountGrain>(name);
            var balance = await accountBalance.GetBalance();
            await context.Response.WriteAsync($"<li> {name} Balance: {balance}\n</li>");
        }
        // await context.Response.WriteAsync($"<li>We transfered 100 credits from {fromName} to {toName}.\n{fromName} balance: {fromBalance}\n{toName} balance: {toBalance}\n</li>");
        await context.Response.WriteAsync("</ul>");
        await context.Response.WriteAsync("</body></html>");
    }
    catch (System.Exception exception)
    {

        if (exception.InnerException is { } inner)
        {
            await context.Response.WriteAsync(@"<html><head><link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/uikit@3.5.5/dist/css/uikit.min.css"" /></head>");
            await context.Response.WriteAsync("<body>");
            await context.Response.WriteAsync("Orleans Bank.<br>");
            await context.Response.WriteAsync($"<p> Error transfering 100 credits from {fromName} to {toName}: {inner.Message}</p>");
            await context.Response.WriteAsync("</body></html>");
        }
        else
        {
            await context.Response.WriteAsync(@"<html><head><link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/uikit@3.5.5/dist/css/uikit.min.css"" /></head>");
            await context.Response.WriteAsync("<body>");
            await context.Response.WriteAsync("Orleans Bank.<br>");
            await context.Response.WriteAsync($"<p> Error transfering 100 credits from {fromName} to {toName}: {exception.Message}</p>");
            await context.Response.WriteAsync("</body></html>");
        }
    }
});

app.Run();

public interface IAccountGrain : IGrainWithStringKey
{
    [Transaction(TransactionOption.Join)]
    Task Withdraw(uint amount);

    [Transaction(TransactionOption.Join)]
    Task Deposit(uint amount);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<uint> GetBalance();
}

public interface IAtmGrain : IGrainWithIntegerKey
{
    [Transaction(TransactionOption.Create)]
    Task Transfer(
        IAccountGrain fromAccount,
        IAccountGrain toAccount,
        uint amountToTransfer
    );
}

public class AccountGrain : Grain, IAccountGrain
{
    private readonly ITransactionalState<Balance> _balance;

    public AccountGrain(
        [TransactionalState("balance")]
        ITransactionalState<Balance> balance
    ) => _balance = balance ?? throw new ArgumentNullException(nameof(balance));

    public Task Deposit(uint amount) => 
        _balance.PerformUpdate(
            balance => balance.Value += amount
        );
    public Task<uint> GetBalance() => _balance.PerformRead(balance => balance.Value);
    public Task Withdraw(uint amount) =>
        _balance.PerformUpdate(
            balance =>
            {
                if (balance.Value <= amount)
                    throw new InvalidOperationException(
                        $"Withdrawing {amount} credits from account " +
                        $"\"{this.GetPrimaryKeyString()}\" would overdraw it." +
                        $" This account has {balance.Value} credits."
                    );

                balance.Value -= amount;
            } 
        );
}

[Serializable]
public record class Balance
{
    public uint Value { get; set; } = 1_000_000;
}

[StatelessWorker]
public class AtmGrain : Grain, IAtmGrain
{
    public Task Transfer(
        IAccountGrain fromAccount, 
        IAccountGrain toAccount, 
        uint amountToTransfer) =>
            Task.WhenAll(
                fromAccount.Withdraw(amountToTransfer),
                toAccount.Deposit(amountToTransfer)
            );
}