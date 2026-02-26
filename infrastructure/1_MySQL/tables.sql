use oliejournal;

CREATE TABLE "Conversations" (
  "Id" varchar(100) NOT NULL,
  "UserId" varchar(100) NOT NULL,
  "Created" datetime NOT NULL,
  "Timestamp" datetime NOT NULL,
  PRIMARY KEY ("Id")
);

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
  "AudioHash" varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "Latitude" float DEFAULT NULL,
  "Longitude" float DEFAULT NULL,
  "Response" text,
  "ResponseCreated" datetime DEFAULT NULL,
  "ResponsePath" varchar(320) DEFAULT NULL,
  "ResponseLength" int DEFAULT NULL,
  "ResponseDuration" int DEFAULT NULL,
  PRIMARY KEY ("Id")
);

CREATE TABLE "JournalTranscripts" (
  "Id" int NOT NULL AUTO_INCREMENT,
  "JournalEntryFk" int NOT NULL,
  "Created" datetime NOT NULL,
  "Transcript" varchar(8096) DEFAULT NULL,
  "ProcessingTime" int NOT NULL,
  "Cost" int NOT NULL,
  "Exception" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  "ServiceFk" int NOT NULL,
  PRIMARY KEY ("Id"),
  KEY "JournalTranscripts_JournalEntries_FK" ("JournalEntryFk"),
  CONSTRAINT "JournalTranscripts_JournalEntries_FK" FOREIGN KEY ("JournalEntryFk") REFERENCES "JournalEntries" ("Id")
);

CREATE TABLE "ChatbotLogs" (
  "Id" int NOT NULL AUTO_INCREMENT,
  "ConversationId" varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "ServiceId" int NOT NULL,
  "Created" datetime NOT NULL,
  "ProcessingTime" int NOT NULL,
  "InputTokens" int NOT NULL,
  "OutputTokens" int NOT NULL,
  "Exception" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  "ResponseId" varchar(100) DEFAULT NULL,
  PRIMARY KEY ("Id"),
  KEY "JournalChatbots_Conversations_FK" ("ConversationId")
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

-- oliejournal.v_JournalEntryList source

CREATE VIEW "oliejournal"."v_JournalEntryList" AS
select
    "je"."Id" AS "Id",
    "je"."UserId" AS "UserId",
    "je"."Created" AS "Created",
    "jt"."Transcript" AS "Transcript",
    "je"."Response" AS "ResponseText",
    "je"."ResponsePath" AS "ResponsePath",
    "je"."Latitude" AS "Latitude",
    "je"."Longitude" AS "Longitude"
from
    ("oliejournal"."JournalEntries" "je"
left join (
    select
        "oliejournal"."JournalTranscripts"."Id" AS "Id",
        "oliejournal"."JournalTranscripts"."JournalEntryFk" AS "JournalEntryFk",
        "oliejournal"."JournalTranscripts"."Transcript" AS "Transcript"
    from
        "oliejournal"."JournalTranscripts"
    where
        ("oliejournal"."JournalTranscripts"."Transcript" is not null)) "jt" on
    (("je"."Id" = "jt"."JournalEntryFk")));
