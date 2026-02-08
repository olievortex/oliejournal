use oliejournal;

CREATE TABLE "JournalEntries" (
  "Id" int NOT NULL AUTO_INCREMENT,
  "UserId" varchar(100) NOT NULL,
  "Created" datetime NOT NULL,
  "AudioPath" varchar(320) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "AudioLength" int NOT NULL,
  "AudioDuration" int NOT NULL,
  "AudioChannels" int NOT NULL,
  "AudioSampleRate" int NOT NULL,
  "AudioBitsPerSample" int NOT NULL,
  "Transcript" varchar(8096) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  "TranscriptProcessingTime" int DEFAULT NULL,
  "TranscriptCost" int NOT NULL,
  PRIMARY KEY ("Id")
);

CREATE TABLE "Conversations" (
  "Id" varchar(100) NOT NULL,
  "UserId" varchar(100) NOT NULL,
  "Created" datetime NOT NULL,
  "Timestamp" datetime NOT NULL,
  "Deleted" datetime DEFAULT NULL,
  PRIMARY KEY ("Id")
);

CREATE TABLE "ConversationLogs" (
  "Id" varchar(100) NOT NULL,
  "ConversationFk" varchar(100) NOT NULL,
  "Created" datetime NOT NULL,
  "InputTokens" int NOT NULL,
  "OutputTokens" int NOT NULL,
  "Instructions" varchar(1024) DEFAULT NULL,
  "Input" varchar(4096) NOT NULL,
  "Output" varchar(4096) NOT NULL,
  "Duration" int NOT NULL,
  PRIMARY KEY ("Id"),
  KEY "ConversationLogs_Conversations_FK" ("ConversationFk"),
  CONSTRAINT "ConversationLogs_Conversations_FK" FOREIGN KEY ("ConversationFk") REFERENCES "Conversations" ("Id")
);
