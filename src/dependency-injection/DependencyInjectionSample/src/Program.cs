using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IOperationTransient, Operation>();
builder.Services.AddScoped<IOperationScoped, Operation>();
builder.Services.AddSingleton<IOperationSingleton, Operation>();
builder.Services.AddSingleton<IOperationSingletonInstance>(new Operation(Guid.Empty));
builder.Services.AddTransient<OperationService, OperationService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

public class OperationsController : Controller
    {
        private readonly OperationService _operationService;
        private readonly IOperationTransient _transientOperation;
        private readonly IOperationScoped _scopedOperation;
        private readonly IOperationSingleton _singletonOperation;
        private readonly IOperationSingletonInstance _singletonInstanceOperation;

        public OperationsController(OperationService operationService,
            IOperationTransient transientOperation,
            IOperationScoped scopedOperation,
            IOperationSingleton singletonOperation,
            IOperationSingletonInstance singletonInstanceOperation)
        {
            _operationService = operationService;
            _transientOperation = transientOperation;
            _scopedOperation = scopedOperation;
            _singletonOperation = singletonOperation;
            _singletonInstanceOperation = singletonInstanceOperation;
        }

        public IActionResult Index()
        {
            // ViewBag contains controller-requested services
            ViewBag.Transient = _transientOperation.OperationId;
            ViewBag.Scoped = _scopedOperation.OperationId;
            ViewBag.Singleton = _singletonOperation.OperationId;
            ViewBag.SingletonInstance = _singletonInstanceOperation.OperationId;

            // Operation service has its own requested services
            ViewBag.Service1 = _operationService.TransientOperation.OperationId;
            ViewBag.Service2 = _operationService.ScopedOperation.OperationId;
            ViewBag.Service3 = _operationService.SingletonOperation.OperationId;
            ViewBag.Service4 = _operationService.SingletonInstanceOperation.OperationId;
            return View();
        }
    } 
public interface IOperation
{
    Guid OperationId {get;}
}

public interface IOperationScoped : IOperation
{
}

public interface IOperationTransient : IOperation
{
}

public interface IOperationSingleton : IOperation
{
}

public interface IOperationSingletonInstance : IOperation
{
}

public class Operation : IOperationTransient, IOperationScoped, IOperationSingleton, IOperationSingletonInstance
{
    Guid _guid;
    public Operation() : this(Guid.NewGuid())
    {
    }

    public Operation(Guid guid)
    {
        _guid = guid;
    }
    public Guid OperationId => _guid;
}

public class OperationService
{
    public IOperationTransient TransientOperation { get; }
    public IOperationScoped ScopedOperation { get; }
    public IOperationSingleton SingletonOperation { get; }
    public IOperationSingletonInstance SingletonInstanceOperation { get; }

    public OperationService(IOperationTransient transientOperation,
        IOperationScoped scopedOperation,
        IOperationSingleton singletonOperation,
        IOperationSingletonInstance instanceOperation)
    {
        TransientOperation = transientOperation;
        ScopedOperation = scopedOperation;
        SingletonOperation = singletonOperation;
        SingletonInstanceOperation = instanceOperation;
    }
}
