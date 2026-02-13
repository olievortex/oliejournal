using System.ComponentModel.DataAnnotations.Schema;

namespace oliejournal.data.Entities;

[Table("v_JournalEntryList")]
public class JournalEntryListEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string? Transcript { get; set; }
    public string? ResponsePath { get; set; }
    public string? ResponseText { get; set; }
}

//-- oliejournal.v_JournalEntryList source

//CREATE VIEW "oliejournal"."v_JournalEntryList" AS
//select
//    "je"."Id" AS "Id",
//    "je"."UserId" AS "UserId",
//    "je"."Created" AS "Created",
//    "jt"."Transcript" AS "Transcript",
//    "jc"."Message" AS "ResponseText",
//    "je"."ResponsePath" AS "ResponsePath"
//from
//    (("oliejournal"."JournalEntries" "je"
//left join (
//    select
//        "oliejournal"."JournalTranscripts"."Id" AS "Id",
//        "oliejournal"."JournalTranscripts"."JournalEntryFk" AS "JournalEntryFk",
//        "oliejournal"."JournalTranscripts"."Transcript" AS "Transcript"
//    from
//        "oliejournal"."JournalTranscripts"
//    where
//        ("oliejournal"."JournalTranscripts"."Transcript" is not null)) "jt" on
//    (("je"."Id" = "jt"."JournalEntryFk")))
//left join(
//    select
//        "oliejournal"."JournalChatbots"."JournalTranscriptFk" AS "JournalTranscriptFk",
//        "oliejournal"."JournalChatbots"."Message" AS "Message"
//    from
//        "oliejournal"."JournalChatbots"
//    where
//        ("oliejournal"."JournalChatbots"."Message" is not null)) "jc" on
//    (("jt"."Id" = "jc"."JournalTranscriptFk")));