using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace oliejournal.data.Entities;

public class ConversationEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime? Deleted { get; set; }
}

//-- oliejournal.Conversations definition

//CREATE TABLE "Conversations" (
//  "Id" varchar(100) NOT NULL,
//  "UserId" varchar(100) NOT NULL,
//  "Created" datetime NOT NULL,
//  "Timestamp" datetime NOT NULL,
//  "Deleted" datetime DEFAULT NULL,
//  PRIMARY KEY("Id")
//);