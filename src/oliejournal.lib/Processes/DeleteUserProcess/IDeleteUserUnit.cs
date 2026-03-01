using oliejournal.data.Entities;

namespace oliejournal.lib.Processes.DeleteUserProcess;

public interface IDeleteUserUnit
{
    Task<UserDeleteLogEntity> CreateDeleteLog(string userId, DateTime requested, CancellationToken ct);
    Task UpdateDeleteLog(UserDeleteLogEntity entity, CancellationToken ct);
}
