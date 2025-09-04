using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace CryptoMonitoring.DataProcessor.Models
{
    [Table("Snapshots")]
    public class EnrichedMarket : CoinMarket
    {
        [Key]
        [Column("Id")]
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("Sma7")]
        public decimal? Sma7 { get; set; }

        [Column("Sma21")]
        public decimal? Sma21 { get; set; }

        [Column("SupportLevel")]
        public decimal? SupportLevel { get; set; }

        [Column("ResistanceLevel")]
        public decimal? ResistanceLevel { get; set; }

        [Column("Volatility")]
        public double? Volatility { get; set; }

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}