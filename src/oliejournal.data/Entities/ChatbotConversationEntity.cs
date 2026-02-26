using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace oliejournal.data.Entities;

public class ChatbotConversationEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Timestamp { get; set; }
}

//-- oliejournal.ChatbotConversations definition

//CREATE TABLE "ChatbotConversations" (
//  "Id" varchar(100) NOT NULL,
//  "UserId" varchar(100) NOT NULL,
//  "Created" datetime NOT NULL,
//  "Timestamp" datetime NOT NULL,
//  PRIMARY KEY("Id")
//);