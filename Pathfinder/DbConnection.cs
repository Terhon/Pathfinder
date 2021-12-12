using System.Data.SqlClient;
using System.Configuration;

namespace Pathfinder
{
    public class DbConnection
    {
        private const string connectionString = "Data Source=pathfinderdbserver.database.windows.net,1433;Initial Catalog=Pathfinder_db;User Id=pathadmin@pathfinderdbserver;Password=Abcd1234";

        private const string checkTablesQuery = "SELECT CASE WHEN (OBJECT_ID('dbo.Countries', 'U') IS NOT NULL AND OBJECT_ID('dbo.NeighbourOf', 'U') IS NOT NULL)" +
                                                " THEN 1 ELSE 0 END";
        private const string createTableCountriesQuery = "CREATE TABLE Countries (ID INTEGER PRIMARY KEY, CountryCode VARCHAR(3)) as NODE; ";
        private const string insertCountriesDataQuery = "INSERT INTO Countries (Id, CountryCode) VALUES" +
                                                        "(1, 'CAN'),(2, 'USA'),(3, 'MEX'),(4, 'BLZ'),(5, 'GTM')," +
                                                        "(6, 'SLV'),(7, 'HND'),(8, 'NIC'),(9, 'CRI'),(10, 'PAN');";
        private const string createNeighboursTableQuery = "CREATE TABLE NeighbourOf AS EDGE;";

        private static readonly Tuple<string, string>[] edges = { Tuple.Create( "CAN", "USA"), Tuple.Create("USA", "MEX" ),Tuple.Create("MEX", "BLZ" ),
                                                                 Tuple.Create("MEX", "GTM" ),Tuple.Create("BLZ", "GTM" ),Tuple.Create("GTM", "SLV" ),
                                                                 Tuple.Create("GTM", "HND" ),Tuple.Create("SLV", "HND" ),Tuple.Create("HND", "NIC" ),
                                                                 Tuple.Create("NIC", "CRI" ),Tuple.Create("CRI", "PAN" )};

        private static SqlConnection GetSqlConnection()
        {
            var conn = new SqlConnection();
            conn.ConnectionString = connectionString;

            return conn;
        }

        public static async Task<List<string>> GetPath(string DestinationCountryCode, string StartCountryCode)
        {
            using var conn = GetSqlConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(GetPathQuery(DestinationCountryCode, StartCountryCode), conn);
            using var dreader = await cmd.ExecuteReaderAsync();

            var path = new List<string>();
            while (await dreader.ReadAsync())
            {
                path.Add(dreader.GetString(0));
            }

            return path;
        }
        private static string GetPathQuery(string DestinationCountryCode, string StartCountryCode)
        {
            return "SELECT * FROM STRING_SPLIT((" +
            "SELECT DestinationCode + '_' + Path" +
            " FROM(" +
                "SELECT" +
                    " Destination.CountryCode AS DestinationCode, " +
                    " STRING_AGG(Ctr.CountryCode, '_') WITHIN GROUP(GRAPH PATH) AS Path," +
                    " LAST_VALUE(Ctr.CountryCode) WITHIN GROUP(GRAPH PATH) AS LastNode" +
                " FROM Countries AS Destination," +
                    " NeighbourOf FOR PATH AS no," +
                    " Countries FOR PATH AS Ctr" +
                " WHERE MATCH(SHORTEST_PATH(Destination(-(no)->Ctr)+))" +
                    " AND Destination.CountryCode = '" + DestinationCountryCode + "') AS Q" +
            " WHERE Q.LastNode = '" + StartCountryCode + "')," +
            "'_');";
        }

        public static void InitData()
        {
            using var conn = GetSqlConnection();
            conn.Open();

            using (var tablesExist = new SqlCommand(checkTablesQuery, conn))
            {
                if (tablesExist.ExecuteScalar().Equals(1))
                {
                    return;
                }
            }

            using var adap = new SqlDataAdapter();

            string[] queries = { createTableCountriesQuery, insertCountriesDataQuery, createNeighboursTableQuery, GetInsertNeighboursQuery() };
            foreach (var query in queries)
            {
                adap.InsertCommand = new SqlCommand(query, conn);
                adap.InsertCommand.ExecuteNonQuery();
            }
        }

        private static string GetInsertNeighboursQuery()
        {
            var edgeList = new List<string>();
            foreach (var edge in edges)
            {
                edgeList.Add(GetNeighboursQuery(edge.Item1, edge.Item2) + "," + GetNeighboursQuery(edge.Item2, edge.Item1));
            }
            var query = "INSERT INTO NeighbourOf VALUES" + String.Join(",", edgeList.ToArray()) + ";";

            return query;
        }

        private static string GetNeighboursQuery(string countryA, string countryB)
        {
            return "((SELECT $NODE_ID FROM Countries WHERE CountryCode = '" + countryA + "'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = '" + countryB + "'))";
        }

    }
}
