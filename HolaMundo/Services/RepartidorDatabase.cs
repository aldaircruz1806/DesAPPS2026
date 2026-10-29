using SQLite;
using UltimaMilla.Repartidor.Models;
using RepartidorModel = global::UltimaMilla.Repartidor.Models.Repartidor;

namespace UltimaMilla.Repartidor.Services;

public class RepartidorDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public RepartidorDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "paradasUltMill.db3");
    }

    private async Task InitAsync()
    {
        if (_database is not null) return;
        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<RepartidorModel>();
    }

    public async Task<List<RepartidorModel>> GetRepartidoresAsync()
    {
        await InitAsync();
        var repartidores = await _database!.Table<RepartidorModel>().ToListAsync();

        if (repartidores.Count == 0)
        {
            await _database.InsertAsync(new RepartidorModel { Nombre = "Miguel Torres" });
            await _database.InsertAsync(new RepartidorModel { Nombre = "Ana Delgado" });
            repartidores = await _database.Table<RepartidorModel>().ToListAsync();
        }

        return repartidores;
    }

    public async Task<RepartidorModel?> GetRepartidorAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<RepartidorModel>()
            .Where(r => r.Id == id).FirstOrDefaultAsync();
    }
}
