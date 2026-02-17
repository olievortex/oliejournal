namespace oliejournal.data.Entities;

public class JournalEntryEntity
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string AudioPath { get; set; } = string.Empty;
    public int AudioLength { get; set; }
    public int AudioDuration { get; set; }
    public int AudioChannels { get; set; }
    public int AudioSampleRate { get; set; }
    public int AudioBitsPerSample { get; set; }
    public string AudioHash { get; set; } = string.Empty;
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public string? ResponsePath { get; set; }
    public int? ResponseLength { get; set; }
    public int? ResponseDuration { get; set; }
    public int? ResponseProcessingTime { get; set; }
    public DateTime? ResponseCreated { get; set; }
}

//-- oliejournal.JournalEntries definition

//CREATE TABLE "JournalEntries" (
//  "Id" int NOT NULL AUTO_INCREMENT,
//  "UserId" varchar(100) NOT NULL,
//  "Created" datetime NOT NULL,
//  "AudioPath" varchar(320) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
//  "AudioLength" int NOT NULL,
//  "AudioDuration" int NOT NULL,
//  "AudioChannels" int NOT NULL,
//  "AudioSampleRate" int NOT NULL,
//  "AudioBitsPerSample" int NOT NULL,
//  "AudioHash" varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
//  "Latitude" float DEFAULT NULL,
//  "Longitude" float DEFAULT NULL,
//  "ResponsePath" varchar(320) DEFAULT NULL,
//  "ResponseLength" int DEFAULT NULL,
//  "ResponseDuration" int DEFAULT NULL,
//  "ResponseProcessingTime" int DEFAULT NULL,
//  "ResponseCreated" datetime DEFAULT NULL,
//  PRIMARY KEY("Id")
//);