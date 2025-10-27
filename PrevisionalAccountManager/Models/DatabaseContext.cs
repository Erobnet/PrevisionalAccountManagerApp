using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PrevisionalAccountManager.JsonConverters;
using PrevisionalAccountManager.Models.DataBaseEntities;

namespace PrevisionalAccountManager.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<TypesStableHashInfoModel> TypesStableHash { get; set; }

        public string DatabaseFileName { get; set; } = "transactions";
        public string DatabaseFilePath => $"{AppSpecificPath}/{DatabaseFileName}.db";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseFilePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Observations).HasMaxLength(500);
                entity.Property(e => e.Amount);
                entity.Property(e => e.CategoryId);
                entity
                    .HasOne(model => model.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    ;

                entity.HasOne<UserModel>()
                    .WithMany()
                    .HasForeignKey(e => e.OwnerUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.SetNull);
                entity.Property(model => model.Date).IsRequired();
            });

            modelBuilder.Entity<CategoryModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.HasOne<UserModel>()
                    .WithMany()
                    .HasForeignKey(e => e.OwnerUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Salt).HasMaxLength(32).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                // Create unique index on username
                entity.HasIndex(e => e.Username).IsUnique();
            });
            modelBuilder.Entity<TypesStableHashInfoModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TypesStableHash);
            });
        }

        private IReadOnlyList<TModel> GetEntities<TModel>(DbSet<TModel> dbSet)
            where TModel : class
        {
            return GetAllDataSafeViaJson<TModel>(dbSet.EntityType.GetTableName());
        }

        public IReadOnlyList<T> GetAllDataSafeViaJson<T>(string? tableName)
            where T : class
        {
            try
            {
                if ( string.IsNullOrEmpty(tableName) )
                    return Array.Empty<T>();

                // Build JSON with proper property mapping
                var jsonData = BuildCompatibleJson<T>(tableName);

                var options = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    Converters = { new AmountJsonConverter(), new DateTimeJsonConverter(), new BooleanJsonConverter() }
                };

                return System.Text.Json.JsonSerializer.Deserialize<List<T>>(jsonData, options) ?? new List<T>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string BuildCompatibleJson<T>(string tableName)
        {
            var jsonArray = new List<Dictionary<string, object>>();
            var sql = $"SELECT * FROM {tableName}";

            using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            var connection = Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;

            try
            {
                if ( !wasOpen )
                    connection.Open();

                using var reader = command.ExecuteReader();

                while ( reader.Read() )
                {
                    var row = new Dictionary<string, object>();

                    // Add existing columns from database
                    for ( int i = 0; i < reader.FieldCount; i++ )
                    {
                        var columnName = reader.GetName(i);
                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[columnName] = value;
                    }

                    jsonArray.Add(row);
                }
            }
            finally
            {
                if ( !wasOpen && connection.State == System.Data.ConnectionState.Open )
                    connection.Close();
            }

            return System.Text.Json.JsonSerializer.Serialize(jsonArray);
        }

        public ImportData ExportAllData()
        {
            var transactions = GetEntities(Transactions);
            var categories = GetEntities(Categories);
            var users = GetEntities(Users);

            var data = new ImportData {
                Transactions = transactions,
                Categories = categories,
                Users = users
            };
            return data;
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<Amount>()
                .HaveConversion<AmountConverter>();
            base.ConfigureConventions(configurationBuilder);
        }

        // Define the converter class
        private class AmountConverter() : ValueConverter<Amount, double>(v => v.Value, v => v)
        { }

        public class ImportData
        {
            public IReadOnlyList<TransactionModel> Transactions { get; init; } = Array.Empty<TransactionModel>();
            public IReadOnlyList<CategoryModel> Categories { get; init; } = Array.Empty<CategoryModel>();
            public IReadOnlyList<UserModel> Users { get; init; } = Array.Empty<UserModel>();
        }

        public void ResetDatabaseData(ImportData? importData)
        {
            ArgumentNullException.ThrowIfNull(importData);
            ChangeTracker.Clear();
            Database.EnsureDeleted();
            Database.EnsureCreated();

            Users.AddRange(importData.Users);
            Categories.AddRange(importData.Categories);
            Transactions.AddRange(importData.Transactions);
        }

        public void CheckMigration()
        {
            var databaseCtx = this;
            var currentDatabaseModelTypesHash = new TypesStableHashInfoModel {
                Id = 1,
                TypesStableHash = new DatabaseTypesStableHash().TypesStableHash
            };

            databaseCtx.Database.EnsureCreated();
            var previousBuildInfo = databaseCtx.TypesStableHash.FirstOrDefault(b => b.Id == currentDatabaseModelTypesHash.Id);
            if ( previousBuildInfo == null )
            {
                previousBuildInfo = currentDatabaseModelTypesHash;
                AddBuildInfoToDatabase(databaseCtx, currentDatabaseModelTypesHash);
            }

            if ( currentDatabaseModelTypesHash.Equals(previousBuildInfo) )
                return;

            MigrateDatabaseData(currentDatabaseModelTypesHash);
        }

        private void MigrateDatabaseData(TypesStableHashInfoModel currentTypesStableHashInfoModel)
        {
            string backupFilePath = BackupFilePathWithName("MigrationBackup.db");
            try
            {
                Directory.CreateDirectory(BackupFilePathWithName());
                BackupDatabaseAtPath(backupFilePath);
                var exportedData = ExportAllData();
                Database.GetDbConnection().Close();
                ResetDatabaseData(exportedData);
                AddBuildInfoToDatabase(this, currentTypesStableHashInfoModel);
            }
            catch
            {
                ImportDatabaseAtPath(backupFilePath);
                throw;
            }

            string BackupFilePathWithName(string fileNameWithExt = "")
            {
                return Path.Combine(AppSpecificPath, $"backups/{fileNameWithExt}");
            }
        }

        private static void AddBuildInfoToDatabase(DatabaseContext databaseCtx, TypesStableHashInfoModel currentTypesStableHashInfoModel)
        {
            databaseCtx.Add(currentTypesStableHashInfoModel);
            databaseCtx.SaveChanges();
        }

        public void BackupDatabaseAtPath(string saveFilePath)
        {
            Database.ExecuteSqlRaw("PRAGMA wal_checkpoint(TRUNCATE);");
            Database.CloseConnection();

            // Copy the database file
            System.IO.File.Copy(DatabaseFilePath, saveFilePath, true);
        }

        public void ImportDatabaseAtPath(string importSourceFilePath)
        {
            Database.EnsureDeleted();
            Database.CloseConnection();
            File.Copy(importSourceFilePath, DatabaseFilePath, true);
            CheckMigration();
        }
    }
}