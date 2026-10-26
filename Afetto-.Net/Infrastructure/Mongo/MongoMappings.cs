using Afetto_.Net.Domain.Entities;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace Afetto_.Net.Infrastructure.Mongo
{
    /// <summary>
    /// Mapeamento BSON das entidades. Fica na Infrastructure para a entidade de domínio
    /// (AuditLog) não depender do driver do MongoDB.
    /// </summary>
    public static class MongoMappings
    {
        private static readonly object Lock = new();

        public static void Register()
        {
            lock (Lock)
            {
                if (BsonClassMap.IsClassMapRegistered(typeof(AuditLog)))
                    return;

                BsonClassMap.RegisterClassMap<AuditLog>(map =>
                {
                    map.AutoMap();
                    map.MapIdMember(a => a.Id)
                       .SetSerializer(new StringSerializer(BsonType.ObjectId))
                       .SetIdGenerator(StringObjectIdGenerator.Instance);
                    map.MapMember(a => a.OcorridoEm)
                       .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
                });
            }
        }
    }
}
