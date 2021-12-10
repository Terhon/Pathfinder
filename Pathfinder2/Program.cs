using System.Data.SqlClient;

SqlConnection conn = new SqlConnection();
conn.ConnectionString =
  "Data Source=desktop-pc;" +
  "Initial Catalog=PathfinderDb;" +
  "Integrated Security=SSPI;";
conn.Open();
Console.WriteLine("Connection Open!");
conn.Close();
/*
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/{name:alpha}", (string name) =>
{
    var forecast = new Path(name);
    return forecast;
})
.WithName("GetPath");

app.Run();

internal record Path(string Destination)
{
    public string[] List => new string[] {Destination};
}*/