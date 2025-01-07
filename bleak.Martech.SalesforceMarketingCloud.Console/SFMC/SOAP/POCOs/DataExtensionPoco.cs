namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap.DataExtensions
{
    public class DataExtensionPoco : BasePoco
    {
        /// <summary>
        /// The ID of the folder that contains the data extension.
        /// </summary>
        public long CategoryID { get; set; }
        public string ObjectID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSendable { get; set; }
        public bool IsTestable { get; set; }

        /// <summary>
        /// The full path as found in SFMC's UI.
        /// </summary>
        public string FullPath { get; set; }
    }
}