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
    public string? Response { get; set; }
    public DateTime? ResponseCreated { get; set; }
    public string? VoiceoverPath { get; set; }
    public DateTime? VoiceoverCreated { get; set; }
    public int? VoiceoverLength { get; set; }
    public int? VoiceoverDuration { get; set; }
    public string? Transcript { get; set; }
    public DateTime? TranscriptCreated { get; set; }
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
//  "Response" text,
//  "ResponseCreated" datetime DEFAULT NULL,
//  "VoiceoverPath" varchar(320) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
//  "VoiceoverCreated" datetime DEFAULT NULL,
//  "VoiceoverLength" int DEFAULT NULL,
//  "VoiceoverDuration" int DEFAULT NULL,
//  "Transcript" text,
//  "TranscriptCreated" datetime DEFAULT NULL,
//  PRIMARY KEY("Id")
//);