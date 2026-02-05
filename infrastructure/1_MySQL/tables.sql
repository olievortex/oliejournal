use oliejournal;

CREATE TABLE "JournalEntries" (
  "Id" int NOT NULL AUTO_INCREMENT,
  "UserId" varchar(100) NOT NULL,
  "Created" datetime NOT NULL,
  "AudioPath" varchar(320) DEFAULT NULL,
  "Transcript" text,
  "TranscriptionTime" int DEFAULT NULL,
  "ConversationLogFk" varchar(100) DEFAULT NULL,
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
