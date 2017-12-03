using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fapl.AdoptionUpdates.Shared
{
    public struct PetIDMapping
    {
        private static readonly Regex PetPointReferenceNumberMatcher = new Regex("\\w+", RegexOptions.Compiled);

        public string PetPointReferenceNumber { get; set; }
        public int PetfinderID { get; set; }
        public string PetPointReferenceNumbers { get; set; }

        public override string ToString()
            => $"{PetPointReferenceNumber} => {PetfinderID}";

        // When pets are brought to the shelter in a group, from the same household or litter, they're
        // sometimes listed together, under a single Petfinder profile.
        public static IReadOnlyCollection<PetIDMapping> Create(string petPointReferenceNumbers, int petfinderID)
        {
            var matches = PetPointReferenceNumberMatcher.Matches(petPointReferenceNumbers);
            if (matches.Count == 0)
                throw new ArgumentException($"Unable to parse PetPoint reference number: \"{petPointReferenceNumbers}\".");

            return matches.Cast<Match>()
                .Select(m => new PetIDMapping
                {
                    PetPointReferenceNumber = m.Value,
                    PetfinderID = petfinderID,
                    PetPointReferenceNumbers = petPointReferenceNumbers
                }).ToArray();
        }
    }
}
