// See https://aka.ms/new-console-template for more information
using CommandLine;
using PluralizeService.Core;
using SqlSugar;

Parser.Default.ParseArguments<Options>(args).WithParsed(Run);

static void Run(Options option)
{
    //Console.WriteLine($"The value for --connectionstring is: {option.ConnectionString}");
    //Console.WriteLine($"The value for --path is: {option.ModelPath}");
    //Console.WriteLine($"The value for --namespace is: {option.ModelNameSpace}");
    using var _db = new SqlSugarScope(new ConnectionConfig()
    {
        DbType = SqlSugar.DbType.MySql,
        ConnectionString = option.ConnectionString,
        InitKeyType = InitKeyType.Attribute,
        IsAutoCloseConnection = true
    });
    foreach (var item in _db.DbMaintenance.GetTableInfoList())
    {
        string entityName = item.Name.ToPascalCase().Pluralize();
        _db.MappingTables.Add(entityName, item.Name);
        foreach (var col in _db.DbMaintenance.GetColumnInfosByTableName(item.Name))
        {
            _db.MappingColumns.Add(col.DbColumnName.ToPascalCase(), col.DbColumnName, entityName);
        }
    }
    _db.DbFirst.IsCreateAttribute().CreateClassFile(option.ModelPath, option.ModelNameSpace);
}
public class Options
{
    [Option('c', "connectionstring", Required = true, HelpText = "資料庫連線字串")]
    public string ConnectionString { get; set; }

    [Option('p', "path", Required = true, HelpText = "Models檔案放置路徑")]
    public string ModelPath { get; set; }

    [Option('n', "namespace", Required = true, HelpText = "Model命名空間")]
    public string ModelNameSpace { get; set; }
}

public static class StringExtension
{
    public static string ToPascalCase(this string value)
    {
        var words = value.Split(new[] { "_", "-", " " }, StringSplitOptions.RemoveEmptyEntries);
        words = words
            .Select(word => char.ToUpper(word[0]) + word.Substring(1))
            .ToArray();
        return string.Join(string.Empty, words);
    }

    public static string Pluralize(this string value)
    {
        return PluralizationProvider.Pluralize(value);
    }
}
