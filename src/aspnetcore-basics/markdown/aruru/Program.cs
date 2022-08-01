using System.Text.Json;
using Markdig;
using Microsoft.AspNetCore.Html;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions {WebRootPath = "Posts"});
builder.Services.AddSingleton<IBlogStorage, JsonStorage>();
builder.Services.AddSingleton<RenderService>();

var app = builder.Build();

app.Run(context => {
    var requestPath = context.Request.Path;
    var postsService = app.Services.GetService<IBlogStorage>()!;
    var posts = postsService.GetPosts(5);

    var renderService = app.Services.GetService<RenderService>();
    if (requestPath == "/")
    {
        foreach (var post in posts)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(renderService.RenderMarkdown(post).ToString());
        }
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

app.Run();



string ProduceMarkdown(string path)
{
    return "";
}

public class Post
{
    public string ID { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }
    public DateTime PubDate { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsPublished { get; set; }
    public List<string> Categories { get; set; }
    public List<Comment> Comments { get; set; } = new();

    public string GetLink()
    {
        return $"/post/{Slug}";
    }
}

public class Comment
{
    public string ID { get; set; }
    public string Author { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
    public string Content { get; set; }
    public bool IsAdmin { get; set; }
}

interface IBlogStorage
{
    List<Post> GetPosts(int take);
}

class JsonStorage : IBlogStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly string _folder;
    private List<Post> _cache = new();

    public JsonStorage(IWebHostEnvironment env) 
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _folder = Path.Combine(_env.ContentRootPath,"Posts");

        init();
    }

    private void init()
    {
        foreach (string file in Directory.EnumerateFiles(_folder, "*.json", SearchOption.TopDirectoryOnly))
        {
            string json = File.ReadAllText(file);
            var post = JsonSerializer.Deserialize<Post>(json);
            
            post.ID = Path.GetFileNameWithoutExtension(file);
            post.Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque cursus malesuada bibendum. Pellentesque arcu neque, maximus non imperdiet et, tincidunt vitae nisi. Aliquam ac vulputate risus. In quis erat tortor. Integer blandit posuere eros in malesuada. Fusce blandit fermentum nunc, at ullamcorper leo suscipit sit amet. Maecenas et diam sodales sapien hendrerit sagittis. Duis iaculis ac eros ut faucibus. Curabitur ornare tincidunt est eu tempor. Integer a blandit sapien, ac tempus sem. Praesent vitae augue ante.\r\n\r\n> Nullam suscipit felis quis justo mattis pretium. Praesent convallis risus nec dolor mattis, et tempor lorem feugiat. Donec elementum sollicitudin elit, venenatis blandit est commodo sed. In et tincidunt erat. Etiam rhoncus posuere rutrum.\r\n\r\n<p style=\"color: darkgreen\">ost</p>\r\n\r\n### Headline 3\r\nNunc mattis congue arcu eget dapibus. Mauris varius convallis massa ac tempor. Curabitur sollicitudin mauris at [sapien malesuada](http://foo.com) congue. Proin in scelerisque est. Duis laoreet elit sem, ut blandit mauris consectetur eget. Donec at risus in risus molestie fringilla. Morbi sit amet arcu egestas, consequat magna vel, sodales ex. Nullam lacus tortor, malesuada eget eleifend eget, tristique ac ligula. Mauris at fermentum urna. Vestibulum tincidunt nulla et congue porta. Ut nulla sem, congue ac tincidunt non, aliquet non diam. Nullam suscipit felis quis justo mattis pretium. Praesent convallis risus nec dolor mattis, et tempor lorem feugiat. Donec elementum sollicitudin elit, venenatis blandit est commodo sed. In et tincidunt erat. Etiam rhoncus posuere rutrum.\r\n\r\n#### Headline 4\r\nVivamus tincidunt molestie nisi, at suscipit urna lacinia at. Integer porta felis at sapien scelerisque lobortis. Nam dignissim blandit dui at dictum. Praesent bibendum massa enim. Sed ultricies augue urna, in volutpat erat ultrices vitae. Phasellus ullamcorper vestibulum enim, quis dapibus velit finibus a. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Mauris ultricies imperdiet mauris. Donec vitae eros nec metus sodales tristique. Pellentesque molestie tempus nulla eu fermentum.";
            post.Slug = "a-great-blog-post";
            post.Excerpt = null;
            post.PubDate = DateTime.UtcNow;
            post.LastModified = DateTime.UtcNow;
            post.IsPublished = true;
            post.Categories = new List<string>{"one","two"};
            post.Comments = new List<Comment>();

            _cache.Add(post);
        }

        _cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
    }

    public List<Post> GetPosts(int take) => _cache.Take(take).ToList();

}

public class RenderService
{
    private MarkdownPipeline _pipeline;

    public RenderService() => BuildPipeline(true);

    public HtmlString RenderMarkdown(Post post)
    {
        string html = Markdown.ToHtml(post.Content, _pipeline);

        return new HtmlString(html);
    }
    private void BuildPipeline(bool allowHtml)
    {
        var builder = new MarkdownPipelineBuilder()
        .UseDiagrams()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter();

        if (!allowHtml)
        {
            builder.DisableHtml();
        }

        _pipeline = builder.Build();
    }
}