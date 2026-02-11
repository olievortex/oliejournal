namespace oliejournal.data.Entities;

public class JournalChatbotEntity
{
    public int Id { get; set; }

    public int JournalTranscriptFk { get; set; }
    public string ConversationFk { get; set; } = string.Empty;
    public int ServiceFk { get; set; }

    public DateTime Created { get; set; }
    public int ProcessingTime { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? ResponseId { get; set; }
}

//-- oliejournal.JournalChatbots definition

//CREATE TABLE "JournalChatbots" (
//  "Id" int NOT NULL AUTO_INCREMENT,
//  "JournalTranscriptFk" int NOT NULL,
//  "ConversationFk" varchar(100) NOT NULL,
//  "ServiceFk" int NOT NULL,
//  "Created" datetime NOT NULL,
//  "ProcessingTime" int NOT NULL,
//  "InputTokens" int NOT NULL,
//  "OutputTokens" int NOT NULL,
//  "Message" varchar(8096) DEFAULT NULL,
//  "Exception" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
//  "ResponseId" varchar(100) DEFAULT NULL,
//  PRIMARY KEY("Id"),
//  KEY "JournalChatbots_JournalTranscripts_FK" ("JournalTranscriptFk"),
//  KEY "JournalChatbots_Conversations_FK" ("ConversationFk"),
//  CONSTRAINT "JournalChatbots_Conversations_FK" FOREIGN KEY("ConversationFk") REFERENCES "Conversations" ("Id"),
//  CONSTRAINT "JournalChatbots_JournalTranscripts_FK" FOREIGN KEY("JournalTranscriptFk") REFERENCES "JournalTranscripts" ("Id")
//);