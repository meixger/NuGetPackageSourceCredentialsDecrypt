using Spectre.Console;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

var roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var nugetConfigPath = Path.Combine(roamingAppDataPath, "NuGet", "NuGet.Config");
AnsiConsole.MarkupLine($"Reading [green]'{nugetConfigPath}'[/]");
Console.WriteLine();
var xml = XDocument.Load(nugetConfigPath);

var packageSources = xml.Root?.Element("packageSources");
if (packageSources != null)
{
    var table = new Table
    {
        Title = new TableTitle("PackageSources",Style.Parse("blue")),
        Expand = true,
        ShowRowSeparators = true,
    };
    table.AddColumn("Name");
    table.AddColumn("Location");
    foreach (var e in packageSources.Elements())
    {
        var key = e.Attribute("key")?.Value ?? "<null>";
        var value = e.Attribute("value")?.Value ?? "<null>";
        table.AddRow(key, value);
    }
    AnsiConsole.Write(table);
}

Console.WriteLine();

var packageSourceCredentials = xml.Root?.Element("packageSourceCredentials");
if (packageSourceCredentials != null)
{
    var table = new Table
    {
        Title = new TableTitle("PackageSourceCredentials", Style.Parse("blue")),
        Expand = true,
        ShowRowSeparators = true,
    };
    table.AddColumn("Name");
    table.AddColumn("Username");
    table.AddColumn("Password"); 
    foreach (var e in packageSourceCredentials.Elements())
    {
        var username = e.Elements().FirstOrDefault(x => x.Name == "add" && x.Attribute("key")?.Value == "Username")?.Attribute("value")?.Value ?? "<null>";
        var password = e.Elements().FirstOrDefault(x => x.Name == "add" && x.Attribute("key")?.Value == "Password")?.Attribute("value")?.Value;
        string decryptedData;
        if (password == null)
        {
            decryptedData = "<null>";
        }
        else
        {
            try
            {
                decryptedData = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(password), Encoding.UTF8.GetBytes("NuGet"), DataProtectionScope.CurrentUser));
            }
            catch (Exception exception)
            {
                decryptedData = $"Error: {exception.Message}";
            }
        }
        table.AddRow(e.Name.ToString(), username, $"[green]{decryptedData}[/]");
    }
    AnsiConsole.Write(table);
}

Console.WriteLine();
Console.WriteLine("Press any key to exit ...");
Console.ReadKey();
