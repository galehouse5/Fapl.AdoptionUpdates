using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fapl.AdoptionUpdates.Shared.PetPoint
{
    public class PetPointPetInfo : IPetInfo
    {
        public string Breed { get; private set; }
        public DateTime? Dob { get; private set; }
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string ReferenceNumber { get; private set; }
        public string Species { get; private set; }
        public IReadOnlyCollection<Tuple<string, DateTime>> Stages { get; private set; }

        public override string ToString()
            => $"{Name} ({ID})";

        public static PetPointPetInfo Create(string content)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);

            // e.g. Retriever, Labrador/Mix, Tan/White, Medium,
            var breedColorSizeValues = document.GetElementbyId("cphWorkArea_lblLine1")
                .ChildNodes.Single().InnerText
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            // e.g. <b>2 y 0 m 4 d </b>, DOB: <b>4/26/2015</b>, Spayed/Neutered: <b>Yes</b>
            string dobValue = document.GetElementbyId("cphWorkArea_lblLine2")
                .Elements("b").Skip(1).FirstOrDefault()?.InnerText;

            return new PetPointPetInfo
            {
                Breed = string.Join(", ", breedColorSizeValues
                    .Take(breedColorSizeValues.Count() - 2)),
                Dob = string.IsNullOrEmpty(dobValue) ?
                    (DateTime?)null : DateTime.Parse(dobValue),
                ID = int.Parse(document.GetElementbyId("cphWorkArea_lblAnimalNum").InnerText.TrimStart('A')),
                Name = document.GetElementbyId("cphWorkArea_lblName").InnerText,
                ReferenceNumber = document.GetElementbyId("cphWorkArea_lblARN").InnerText,
                Species = document.GetElementbyId("cphWorkArea_lblSpecies").InnerText,
                Stages = document.GetElementbyId("cphWorkArea_dgStage")
                    .Elements("tr").Skip(1) // Skip the header row.
                    .Select(r => new Tuple<string, DateTime>(
                        /* Stage */ r.Elements("td").ElementAt(0).InnerText.Trim(),
                        /* From (Date/Time) */ DateTime.Parse(r.Elements("td").ElementAt(1).InnerText)))
                    .ToArray()
            };
        }
    }
}
