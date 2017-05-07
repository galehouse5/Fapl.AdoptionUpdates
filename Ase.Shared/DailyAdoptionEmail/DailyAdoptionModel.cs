using Humanizer;
using Humanizer.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ase.Shared.DailyAdoptionEmail
{
    public class DailyAdoptionModel
    {
        public class Pet
        {
            public string Species { get; set; }
            public string Name { get; set; }
            public int? PetfinderID { get; set; }
            public TimeSpan? Age { get; set; }
            public TimeSpan? Stay { get; set; }

            public string SummaryText
                => string.Join(" • ", new[]
                {
                    Age.HasValue ? $"{Age.Value.Humanize(maxUnit: TimeUnit.Year)} old" : null,
                    Stay.HasValue ? $"{Stay.Value.Humanize(maxUnit: TimeUnit.Year).Replace("s", null)} stay" : null
                }.Where(v => v != null));
        }

        public DateTime Date { get; set; }
        public IReadOnlyCollection<Pet> Pets { get; set; }
        public IEnumerable<Pet> OrderedPets => Pets
            .OrderBy(p => p.Name)
            .ThenByDescending(p => p.Stay);

        public string Title
            => $"{Date:yyyy-MM-dd}, {Date:dddd}";

        public string PreviewText
            => $"{GetSpeciesAdoptionCountsText(i => i.ToString())} adopted";

        public string GreetingText
            => string.Format("{0} adopted on {1:dddd, MMM d, yyyy}.",
                GetSpeciesAdoptionCountsText(i => i.ToWords()),
                Date)
            .ApplyCase(LetterCasing.Sentence);

        protected string GetSpeciesAdoptionCountsText(Func<int, string> countFormatter)
            => string.Join(", ",
                from pet in Pets
                group pet by pet.Species into petSpecies
                let count = petSpecies.Count()
                orderby count descending
                let species = petSpecies.Key
                select $"{countFormatter(count)} {(count == 1 ? species : species.Pluralize()).ToLower()}");
    }
}
