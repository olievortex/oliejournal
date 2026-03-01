using oliejournal.data;
using oliejournal.data.Entities;

namespace oliejournal.lib.Processes.DeleteUser;

public class DeleteUserUnit(IMyRepository repo) : IDeleteUserUnit
{
    public async Task<UserDeleteLogEntity> CreateDeleteLog(string userId, DateTime requested, CancellationToken ct)
    {
        var entity = new UserDeleteLogEntity
        {
            UserId = userId,
            Requested = requested,
            DeleteViaApi = true,
        };

        await repo.UserDeleteLogCreate(entity, ct);

        return entity;
    }

    public async Task UpdateDeleteLog(UserDeleteLogEntity entity, CancellationToken ct)
    {
        entity.Completed = DateTime.UtcNow;

        await repo.UserDeleteLogUpdate(entity, ct);
    }
}
