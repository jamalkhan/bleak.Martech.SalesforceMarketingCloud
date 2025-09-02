# bleak.Martech.SalesforceMarketingCloud

## Download SfmcApp for macOS

Choose your architecture:
- **Apple Silicon (M1/M2, ARM64):** [Download DMG](https://github.com/jamalkhan/bleak.Martech.SalesforceMarketingCloud/releases/download/v1.1.5/SfmcApp-macOS-arm64-1.1.5.dmg)
- **Intel (x64):** [Download DMG](https://github.com/jamalkhan/bleak.Martech.SalesforceMarketingCloud/releases/download/v1.1.5/SfmcApp-macOS-x64-1.1.5.dmg)

## ConsoleApp

This is the bread and butter of this application.

In order to run this, you will need to create a file in your application directory called config.json. By Default, the build process will try to copy a config.json.user file to the output directory as config.json; however, config.json.user is .gitignored.

The structure of said config.json file is as follows:

```
{
    "OutputFolder":"/YOURDOWNLOADFOLDER",
    "Subdomain":"<SFMC-SubDomain>",
    "ClientId":"<SFMC-ClientID>",
    "ClientSecret":"<SFMC-ClientSecret>",
    "MemberId":"123456",
    "PageSize":500,
    "Debug":false
}
```

I'll add details on how to get the SFMC API Keys at some point.

There are currently 4 options for this app.

1. Downloading Content
2. Enumerating the Data Extension Folders
3. Enumerating the Data Extensions
4. Creating a file with all of the Data Extensions and the Folders they belong to as seen while browsing SFMC.

#4 is currently the fastest and most useful.
#1 is slow because of the REST API. *sigh* Scott Dorsey are you reading this? It's time to make things easier for your users.