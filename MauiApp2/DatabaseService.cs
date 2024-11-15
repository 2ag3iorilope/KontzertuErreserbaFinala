using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MauiApp2
{
    internal class DatabaseService
    {

        /// <summary>
        /// Gure SQL konexioa.
        /// </summary>
        private readonly string connectionString = "Server=localhost;Port=3306;Database=kontzertuerreserba;User Id=root;Password=mysql;";

        /// <summary>
        /// Datubasera Konektatzeko Metodoa
        /// </summary>
        /// <returns>
        /// Gure datubasearea konexioa egin den eta zein datubasera konektatu garen erakusten du dena ondo joan bada 
        /// edo errore mezu bat arazo bat egon bada
        /// </returns>
        public async Task<string> ConectarBaseDatosAsync()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();


                    string database = connection.Database;
                    string server = connection.DataSource;

                    return $"Konektatuta ondorengo datubasera: '{database}' ondorengo zerbitzarian '{server}'";
                }
                catch (Exception ex)
                {
                    return $"Errorea konektatzean: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Hiri jakin bati lotutako erreserba-kopurua lortzen du.
        /// </summary>
        /// <param name="city">Erreserbak kontatu nahi diren herria.</param>
        /// <returns>
        /// Zenbaki oso bat, zehaztutako hirirako aurkitutako erreserben kopurua adierazten duena.
        /// </returns>
        public async Task<int> GetReservasCountAsync(string city)
        {
            int reservas = 0;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT COUNT(idErreserbak) FROM erreserbak inner join kontzertuak on Kontzertuak_idKontzertua = idKontzertua and Hiria = @City";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@City", city);

                    try
                    {
                        await connection.OpenAsync();
                        reservas = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            return reservas;
        }


    }
}
