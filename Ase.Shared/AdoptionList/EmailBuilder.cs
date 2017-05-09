using Ase.Shared.Helpers;
using HandlebarsDotNet;
using MoreLinq;
using System;
using System.Linq;
using System.Reflection;

namespace Ase.Shared.AdoptionList
{
    public delegate string GetPetfinderPhotoUrl(int petID, int width, int height);
    public delegate string GetNoPhotoUrl(string species, int width, int height);

    public class EmailBuilder
    {
        private Func<object, string> htmlBodyTemplate;

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
                    .GetManifestResourceString("Ase.Shared.AdoptionList.EmailBodyHtml.hbs"));
            }

            return htmlBodyTemplate(new
            {
                title = model.Title,
                previewText = model.PreviewText,
                headerLogoUrl = HeaderLogoUrl,
                greetingText = model.GreetingText,
                rows = model.Pets
                    .OrderBy(p => p.Name)
                    .ThenByDescending(p => p.Stay)
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
                            summaryText = pet.SummaryText
                        })
                    })
            });
        }
    }
}
