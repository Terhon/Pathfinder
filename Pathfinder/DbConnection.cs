using System.Data.SqlClient;

namespace Pathfinder.Domain.Persistence.Contexts
{
    public class DbConnection
    {
        private const string connectionString = "Data Source=desktop-pc;" +
                                                "Initial Catalog=PathfinderDb;" +
                                                "Integrated Security=SSPI;";
        private const string checkTablesQuery = "SELECT CASE WHEN (OBJECT_ID('dbo.Countries', 'U') IS NOT NULL AND OBJECT_ID('dbo.NeighbourOf', 'U') IS NOT NULL)" +
                                                " THEN 1 ELSE 0 END";
        private const string createTableCountriesQuery = "CREATE TABLE Countries (ID INTEGER PRIMARY KEY, CountryCode VARCHAR(3)) as NODE; ";
        private const string insertCountriesDataQuery = "INSERT INTO Countries (Id, CountryCode) VALUES" +
                                                        "(1, 'CAN'),(2, 'USA'),(3, 'MEX'),(4, 'BLZ'),(5, 'GTM')," +
                                                        "(6, 'SLV'),(7, 'HND'),(8, 'NIC'),(9, 'CRI'),(10, 'PAN');";
        private const string createNeighboursTableQuery = "CREATE TABLE NeighbourOf AS EDGE;";
        private const string insertNeighboursDataQuery = "INSERT INTO NeighbourOf VALUES" +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'CAN'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'USA'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'USA'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'MEX'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'MEX'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'BLZ'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'MEX'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'GTM'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'BLZ'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'GTM'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'GTM'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'SLV'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'GTM'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'HND'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'SLV'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'HND'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'HND'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'NIC'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'NIC'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'CRI'))," +
                "((SELECT $NODE_ID FROM Countries WHERE CountryCode = 'CRI'), (SELECT $NODE_ID FROM Countries WHERE CountryCode = 'PAN'));";


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

            string[] queries = { createTableCountriesQuery, insertCountriesDataQuery, createNeighboursTableQuery, insertNeighboursDataQuery };
            foreach (var query in queries)
            {
                adap.InsertCommand = new SqlCommand(query, conn);
                adap.InsertCommand.ExecuteNonQuery();
            }
        }
    }
}
