namespace oliejournal.data.Entities;

public class JournalTranscriptEntity
{
    public int Id { get; set; }

    public int JournalEntryFk { get; set; }
    public int ServiceFk { get; set; }

    public DateTime Created { get; set; }
    public string? Transcript { get; set; }
    public int ProcessingTime { get; set; }
    public int Cost { get; set; }
    public string? Exception { get; set; }

}

//-- oliejournal.JournalTranscripts definition

//CREATE TABLE "JournalTranscripts" (
//  "Id" int NOT NULL AUTO_INCREMENT,
//  "JournalEntryFk" int NOT NULL,
//  "Created" datetime NOT NULL,
//  "Transcript" varchar(8096) DEFAULT NULL,
//  "ProcessingTime" int NOT NULL,
//  "Cost" int NOT NULL,
//  "Exception" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
//  "ServiceFk" int NOT NULL,
//  PRIMARY KEY("Id"),
//  KEY "JournalTranscripts_JournalEntries_FK" ("JournalEntryFk"),
//  CONSTRAINT "JournalTranscripts_JournalEntries_FK" FOREIGN KEY("JournalEntryFk") REFERENCES "JournalEntries" ("Id")
//);