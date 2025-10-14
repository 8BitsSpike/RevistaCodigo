﻿using MongoDB.Driver;
using Artigo.DbContext.PersistenceModels; // CRITICAL: Re-adding this namespace reference
using System.ComponentModel.DataAnnotations;

namespace Artigo.DbContext.Config
{
    // ... (MongoDBSettings class remains the same)
}

namespace Artigo.DbContext.Data
{
    using Artigo.DbContext.Config;
    using Artigo.DbContext.Interfaces;

    /// <sumario>
    /// Implementação do contexto de dados. Centraliza a conexão do MongoClient
    /// e a inicialização de todas as coleções do projeto.
    /// </sumario>
    public class MongoDbContext : Artigo.DbContext.Interfaces.IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, string databaseName)
        {
            _database = client.GetDatabase(databaseName);
        }

        // FIX: The collection properties must use the unqualified Persistence Model type.
        // The return type is correctly inferred by the using Artigo.DbContext.PersistenceModels;
        // which now matches the IMongoDbContext contract's expectations.
        public IMongoCollection<ArtigoModel> Artigos => _database.GetCollection<ArtigoModel>(nameof(ArtigoModel));
        public IMongoCollection<AutorModel> Autores => _database.GetCollection<AutorModel>(nameof(AutorModel));
        public IMongoCollection<EditorialModel> Editoriais => _database.GetCollection<EditorialModel>(nameof(EditorialModel));
        public IMongoCollection<InteractionModel> Interactions => _database.GetCollection<InteractionModel>(nameof(InteractionModel));
        public IMongoCollection<ArtigoHistoryModel> ArtigoHistories => _database.GetCollection<ArtigoHistoryModel>(nameof(ArtigoHistoryModel));
        public IMongoCollection<PendingModel> Pendings => _database.GetCollection<PendingModel>(nameof(PendingModel));
        public IMongoCollection<StaffModel> Staffs => _database.GetCollection<StaffModel>(nameof(StaffModel));
        public IMongoCollection<VolumeModel> Volumes => _database.GetCollection<VolumeModel>(nameof(VolumeModel));
    }
}