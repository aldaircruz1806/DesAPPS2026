using SQLite;
using UltimaMilla.Repartidor.Models;
namespace UltimaMilla.Repartidor.Services
{
    public class ParadasDatabase
    {
        private readonly SesionActual _sesion;
        private SQLiteAsyncConnection? _database;
        private readonly string _dbPath;
        public ParadasDatabase(SesionActual sesion)
        {
            _sesion = sesion;
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "paradasUltMill.db3");

        }

        private async Task InitAsync()
        {
            if (_database != null)
                return;
            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<Parada>();
        }

        public async Task<List<Parada>> GetParadasAsync(bool entregadasTambien)
        {
            await InitAsync();

            var query = _database!
                .Table<Parada>()
                .Where(p => p.RepartidorId == _sesion.RepartidorId);

            if (!entregadasTambien)
            {
                query = query.Where(p => p.Estado != "Entregado");
            }

            return await query.OrderBy(p => p.Numero).ToListAsync();
        }


        public async Task<Parada> GetParadaAsync(Parada parada)
        {
            await InitAsync();
            return await _database!.Table<Parada>().Where(p => p.Id == parada.Id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveParadaAsync(Parada parada)
        {
            await InitAsync();
            if (parada.Id != 0)

                return await _database!.UpdateAsync(parada);
            else

                return await _database!.InsertAsync(parada);
        }

        public async Task<int> DeleteParadaAsync(Parada parada)
        {
            await InitAsync();
            return await _database!.DeleteAsync(parada);
        }

      
    }
}