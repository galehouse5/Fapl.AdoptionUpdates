using Fapl.AdoptionUpdates.Shared.Helpers;
using HandlebarsDotNet;
using MoreLinq;
using System;
using System.Linq;
using System.Reflection;

namespace Fapl.AdoptionUpdates.Shared.AdoptionList
{
    public delegate string GetPetfinderPhotoUrl(int petID, int width, int height);
    public delegate string GetNoPhotoUrl(string species, int width, int height);

    public class EmailBuilder : IEmailBuilder<Model>
    {
        private Func<object, string> htmlBodyTemplate;
        private Func<object, string> textBodyTemplate;

        public string HeaderLogoUrl { get; set; }
        public GetPetfinderPhotoUrl GetPetfinderPhotoUrl { get; set; }
        public GetNoPhotoUrl GetNoPhotoUrl { get; set; }

        public string GenerateSubject(Model model)
            => model.Title;

        public string GenerateHtmlBody(Model model)
        {
            if (htmlBodyTemplate == null)
            {
                htmlBodyTemplate = Handlebars.Compile(Assembly.GetExecutingAssembly()
                    .GetManifestResourceString("Fapl.AdoptionUpdates.Shared.AdoptionList.EmailBodyHtml.hbs"));
            }

            return htmlBodyTemplate(new
            {
                title = model.Title,
                previewText = model.PreviewText,
                headerLogoUrl = HeaderLogoUrl,
                greetingText = model.GreetingText,
                rows = model.Pets
                    .Batch(2).Select(row => new
                    {
                        pets = row.Select((pet, i) => new
                        {
                            align = i == 0 ? "left" : "right",
                            petfinderProfileUrl = !pet.PetfinderID.HasValue ? null
                                : $"https://www.petfinder.com/petdetail/{pet.PetfinderID}",
                            photoUrl = !pet.PetfinderID.HasValue ? GetNoPhotoUrl(pet.Species, 600, 400)
                                : GetPetfinderPhotoUrl(pet.PetfinderID.Value, 600, 400),
                            name = pet.Name,
                            summaryText = pet.GetSummaryText(" • ")
                        })
                    })
            });
        }

        public string GenerateTextBody(Model model)
        {
            if (textBodyTemplate == null)
            {
                textBodyTemplate = Handlebars.Compile(Assembly.GetExecutingAssembly()
                    .GetManifestResourceString("Fapl.AdoptionUpdates.Shared.AdoptionList.EmailBodyText.hbs"));
            }

            return textBodyTemplate(new
            {
                greetingText = model.GreetingText,
                pets = model.Pets
                    .OrderBy(p => p.Name)
                    .ThenByDescending(p => p.Stay)
                    .Select(pet => new
                    {
                        name = pet.Name,
                        species = pet.Species,
                        summaryText = pet.GetSummaryText(", "),
                        petfinderProfileUrl = !pet.PetfinderID.HasValue ? null
                            : $"https://www.petfinder.com/petdetail/{pet.PetfinderID}"
                    })
            });
        }
    }
}
