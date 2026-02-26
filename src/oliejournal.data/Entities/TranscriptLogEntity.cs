namespace oliejournal.data.Entities;

public class TranscriptLogEntity
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public DateTime Created { get; set; }
    public int ProcessingTime { get; set; }
    public int Cost { get; set; }
    public string? Exception { get; set; }

}

//-- oliejournal.TranscriptLogs definition

//CREATE TABLE "TranscriptLogs" (
//  "Id" int NOT NULL AUTO_INCREMENT,
//  "ServiceId" int NOT NULL,
//  "Created" datetime NOT NULL,
//  "ProcessingTime" int NOT NULL,
//  "Cost" int NOT NULL,
//  "Exception" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
//  PRIMARY KEY("Id")
//);