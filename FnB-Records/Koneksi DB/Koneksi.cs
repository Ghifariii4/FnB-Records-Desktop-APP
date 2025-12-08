using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnB_Records.Koneksi_DB
{
    public class Koneksi
    {
        private const string DbHost = "localhost";
        private const string DbName = "DBFnBRecords";
        private const string DbUser = "postgres";
        private const string DbPass = "123";
        private const int DbPort = 5432; 

        private readonly string connectionString;


        public Koneksi()
        {
            connectionString = $"Host={DbHost};Port={DbPort};Username={DbUser};Password={DbPass};Database={DbName};";
        }
        public NpgsqlConnection GetKoneksi()
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connectionString);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                Console.WriteLine("Koneksi ke database berhasil dibuka.");
                return conn;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Error saat membuka koneksi database: {ex.Message}");
                throw new Exception("Gagal terhubung ke database. Cek konfigurasi server.", ex);
            }
        }

    }
}
