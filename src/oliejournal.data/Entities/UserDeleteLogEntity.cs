namespace oliejournal.data.Entities;

public class UserDeleteLogEntity
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public DateTime Requested { get; set; }
    public bool DeleteViaApi { get; set; }
    public DateTime? Completed { get; set; }
    // public string? Notes { get; set; } // Notes field is currently unused, but we may want to add information here in the future about the deletion (e.g. if there were any issues during the process)
}

//-- oliejournal.UserDeleteLogs definition

//CREATE TABLE "UserDeleteLogs" (
//  "Id" int NOT NULL AUTO_INCREMENT,
//  "UserId" varchar(100) NOT NULL,
//  "Requested" datetime NOT NULL,
//  "DeleteViaApi" tinyint(1) NOT NULL,
//  "Completed" datetime DEFAULT NULL,
//  "Notes" text,
//  PRIMARY KEY("Id")
//);