namespace oliejournal.data.Entities;

public class ChatbotLogEntity
{
    public int Id { get; set; }

    public string ConversationId { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public DateTime Created { get; set; }
    public int ProcessingTime { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public string? Exception { get; set; }
    public string? ResponseId { get; set; }
}

//-- oliejournal.ChatbotLogs definition

//CREATE TABLE "ChatbotLogs" (
//  "Id" int NOT NULL AUTO_INCREMENT,
//  "ConversationId" varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
//  "ServiceId" int NOT NULL,
//  "Created" datetime NOT NULL,
//  "ProcessingTime" int NOT NULL,
//  "InputTokens" int NOT NULL,
//  "OutputTokens" int NOT NULL,
//  "Exception" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
//  "ResponseId" varchar(100) DEFAULT NULL,
//  PRIMARY KEY("Id")
//);