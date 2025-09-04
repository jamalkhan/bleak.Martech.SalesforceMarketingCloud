namespace bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos
{
    public partial class SfmcFolder
    {
        public FolderObject ToFolderObject()
        {
            return new FolderObject()
            {
                ParentId = parentId,
                Id = id,           
                Description = description,
                EnterpriseId = enterpriseId,
                MemberId = memberId,
                Name = name,
                CategoryType = categoryType
            };
        }
    }
}