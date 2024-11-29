using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;

namespace bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos
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