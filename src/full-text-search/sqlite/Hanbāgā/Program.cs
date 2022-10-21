// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using SQLitePCL;

Console.WriteLine("Hello, World!");
// adjust this path to where your sqlite3.so is stored
string sqliteLibPath = Path.GetFullPath(@"external\sqlite\sqlite3");

// external\sqlite\.libs\libsqlite3.so
SQLite3Provider_dynamic_cdecl.Setup("sqlite3", new NativeLibraryAdapter(sqliteLibPath));
SQLitePCL.raw.SetProvider(new SQLite3Provider_dynamic_cdecl());


string databasePath = Path.GetFullPath(@"data\dictionary.db");
using var connection = new SqliteConnection($"Data Source={databasePath}");

// open the database connection
connection.Open();

// close the database connection when we quit the app using Ctrl+C
Console.CancelKeyPress += delegate
{
    connection.Close();
    Environment.Exit(0);
};

// ask the user for a word to look up until they quit the app
while (true)
{
    Console.Write("== Enter a word (ctrl+c to cancel): ");
    var word = Console.ReadLine();

    // ask again if the word is empty
    if (string.IsNullOrEmpty(word))
    {
        Console.WriteLine("Word can't be empty!");
        continue;
    }

    // create a command to search the database and bind the user input
    // to the `@word` parameter
    var command = connection.CreateCommand();
    command.CommandText = $@"
    SELECT
      Dictionary.EnglishText,
      Dictionary.JapaneseText
    FROM Dictionary
    WHERE Dictionary.JapaneseText IN (
        SELECT DictionaryFTS.JapaneseText FROM DictionaryFTS
        WHERE DictionaryFTS MATCH @word
    );";
    command.Parameters.Add(new SqliteParameter("@word", word));

    using var reader = command.ExecuteReader();

    // prints a message when no result was found
    if (!reader.HasRows)
    {
        Console.WriteLine("No result was found!");
        continue;
    }

    // print the results in a list
    while (reader.Read())
    {
        Console.WriteLine($"- {reader["EnglishText"]} => {reader["JapaneseText"]}");
    }
}



public class NativeLibraryAdapter : IGetFunctionPointer
{
    readonly IntPtr _library;

    public NativeLibraryAdapter(string name)
        => _library = NativeLibrary.Load(name);

    public IntPtr GetFunctionPointer(string name)
        => NativeLibrary.TryGetExport(_library, name, out var address)
            ? address
            : IntPtr.Zero;
}