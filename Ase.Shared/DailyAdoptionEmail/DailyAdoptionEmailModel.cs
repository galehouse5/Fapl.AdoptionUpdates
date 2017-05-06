using System;
using System.Collections.Generic;

namespace Ase.Shared.DailyAdoptionEmail
{
    public class DailyAdoptionEmailModel
    {
        public class Pet
        {
            public string Species { get; set; }
            public string Name { get; set; }
            public int? PetfinderID { get; set; }
            public TimeSpan? Age { get; set; }
            public TimeSpan? Stay { get; set; }
        }

        public DateTime Date { get; set; }
        public IReadOnlyCollection<Pet> Pets { get; set; }
    }
}
