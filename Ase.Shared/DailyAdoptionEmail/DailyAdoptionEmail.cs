using RazorEngine;
using RazorEngine.Templating;
using System.Collections.Generic;
using System.IO;

namespace Ase.Shared.DailyAdoptionEmail
{
    public delegate string GetPetfinderPhotoUrl(int petID, int width, int height);
    public delegate string GetNoPhotoUrl(string species, int width, int height);

    public class DailyAdoptionEmail
    {
        private string htmlBodySource;

        public string HeaderLogoUrl { get; set; }
        public GetPetfinderPhotoUrl GetPetfinderPhotoUrl { get; set; }
        public GetNoPhotoUrl GetNoPhotoUrl { get; set; }

        public string GenerateSubject(DailyAdoptionModel model)
            => model.Title;

        public string GenerateHtmlBody(DailyAdoptionModel model)
            => Engine.Razor.RunCompile(
                templateSource: htmlBodySource ?? (htmlBodySource = File.ReadAllText(@"DailyAdoptionEmail\HtmlBody.cshtml")),
                name: "DailyAdoptionEmailHtmlBody",
                modelType: model.GetType(),
                model: model,
                viewBag: new DynamicViewBag(new Dictionary<string, object>
                {
                    { "HeaderLogoUrl", HeaderLogoUrl },
                    { "GetPetfinderPhotoUrl", GetPetfinderPhotoUrl },
                    { "GetNoPhotoUrl", GetNoPhotoUrl }
                }));
    }
}
