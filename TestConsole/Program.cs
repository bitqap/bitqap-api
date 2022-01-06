// See https://aka.ms/new-console-template for more information
using System.Data.SQLite;

Console.WriteLine("Hello, World!");

a aa = new a();
Console.WriteLine(aa);

string cs = @"URI=file:C:\Users\sulkhayev\Documents\Bitqap.Middleware.DB";

using var con = new SQLiteConnection(cs);
con.Open();

using var cmd = new SQLiteCommand(con);
cmd.CommandText = "INSERT INTO test(id, name,surname) VALUES(@id,@name, @surname)";

cmd.Parameters.AddWithValue("@id", 9);
cmd.Parameters.AddWithValue("@name", "BMjhhW");
cmd.Parameters.AddWithValue("@surname", "asasjkjhha");
cmd.Prepare();

cmd.ExecuteNonQuery();
con.Close();

Console.WriteLine("row inserted");

Console.ReadLine();


public class a
{

}