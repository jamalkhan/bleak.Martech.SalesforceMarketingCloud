using System.IO;

namespace SfmcApp.Models
{
    public class SfmcConnection
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }
        public required string Subdomain { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string MemberId { get; set; }

        public string DirectoryName
        {
            get
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                var safeName = new string(Name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
                return safeName.Replace(' ', '_');
            }
        }

    }
}