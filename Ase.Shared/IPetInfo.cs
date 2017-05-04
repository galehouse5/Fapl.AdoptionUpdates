using System;
using System.Collections.Generic;
using System.Linq;

namespace Ase.Shared
{
    public interface IPetInfo
    {
        int ID { get; }
        string ReferenceNumber { get; }
        string Name { get; }
        string Species { get; }
        string Breed { get; }
        DateTime? Dob { get; }
        IReadOnlyCollection<Tuple<string, DateTime>> StageTimestamps { get; }
    }

    public static class IPetPointPetInfoExtensions
    {
        public static TimeSpan? GetLastStayLength(this IPetInfo info, DateTime now)
        {
            DateTime? lastNewArrivalTimestamp = info.StageTimestamps
                .Where(s => s.Item1.Equals("New Arrival", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()?.Item2;
            if (!lastNewArrivalTimestamp.HasValue)
                return null;

            DateTime? lastReleasedTimestamp = info.StageTimestamps
                .Where(s => s.Item1.Equals("Released", StringComparison.OrdinalIgnoreCase))
                .Where(s => s.Item2 > lastNewArrivalTimestamp)
                .FirstOrDefault()?.Item2;

            return (lastReleasedTimestamp ?? now) - lastNewArrivalTimestamp;
        }
    }
}
