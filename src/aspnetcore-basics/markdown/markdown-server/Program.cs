var builder = WebApplication.CreateBuilder(new WebApplicationOptions{WebRootPath = "markdown"});
var app = builder.Build();

app.Run(context => {
    var requestPath = context.Request.Path;

    if (requestPath == "/")
    {
        var defaultMd = Path.Combine(app.Environment.WebRootPath, "index.md");
        if (!File.Exists(defaultMd))
        {
            context.Response.StatusCode = 404;
            return context.Response.WriteAsync("File not found");
        }

        context.Response.ContentType = "text/html";
        return context.Response.WriteAsync(ProduceMarkdown(defaultMd));
    }
    var localPath = requestPath.ToString().Replace('/', '\\').TrimStart(new char[]{'\\'}) + ".md";
    var md = Path.Combine(app.Environment.WebRootPath, localPath);
    if (!File.Exists(md))
    {
        context.Response.StatusCode = 404;
        return context.Response.WriteAsync("File not found");
    }

    context.Response.ContentType = "text/html";
    return context.Response.WriteAsync(ProduceMarkdown(md));
});

string ProduceMarkdown(string path)
{
    var md = File.ReadAllText(path);
    var res = Markdig.Markdown.ToHtml(md);
    return res;
}

app.Run();
