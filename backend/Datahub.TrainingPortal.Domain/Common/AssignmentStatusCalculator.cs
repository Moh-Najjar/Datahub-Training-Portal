using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Domain.Common;

public static class AssignmentStatusCalculator
{
    public static AssignmentStatus Compute(DateTime opensAt, DateTime closesAt, DateTime now)
    {
        if (now < opensAt) return AssignmentStatus.Upcoming;
        if (now > closesAt) return AssignmentStatus.Closed;
        return AssignmentStatus.Open;
    }
}
