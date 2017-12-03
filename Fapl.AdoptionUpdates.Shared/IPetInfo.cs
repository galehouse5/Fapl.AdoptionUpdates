using System;
using System.Collections.Generic;
using System.Linq;

namespace Fapl.AdoptionUpdates.Shared
{
    public interface IPetInfo
    {
        int ID { get; }
        string ReferenceNumber { get; }
        string Name { get; }
        string Species { get; }
        string Breed { get; }
        DateTime? Dob { get; }
        IReadOnlyCollection<Tuple<string, DateTime>> Stages { get; }
    }

    public static class IPetPointPetInfoExtensions
    {
        public static IReadOnlyCollection<Tuple<string, DateTime>> GetLastStayStages(this IPetInfo info, DateTime now)
            => info.Stages
            .Where(s => s.Item2 <= now)
            .OrderByDescending(s => s.Item2)
            .TakeWhile((s, i) => !"Released".Equals(s.Item1, StringComparison.OrdinalIgnoreCase) || i == 0)
            .ToArray();

        public static TimeSpan? GetLastStayLength(this IPetInfo info, DateTime now)
        {
            var stages = info.GetLastStayStages(now)
                .OrderByDescending(s => s.Item2);
            if (!stages.Any()) return null;

            bool hasEnded = "Released".Equals(stages.First().Item1, StringComparison.OrdinalIgnoreCase);
            if (!hasEnded) return now - stages.Last().Item2;

            return stages.First().Item2 - stages.Last().Item2;
        }
    }
}
