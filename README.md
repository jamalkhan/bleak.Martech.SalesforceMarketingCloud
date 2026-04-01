# bleak.Martech.SalesforceMarketingCloud

Tools for working with Salesforce Marketing Cloud content, assets, and data extensions.

This repository currently contains:

- `bleak.Martech.SalesforceMarketingCloud`: the shared SFMC client library.
- `bleak.Martech.SalesforceMarketingCloud.Console`: a CLI for bulk exports and metadata pulls.
- `SfmcApp`: a .NET MAUI desktop app for browsing connections, assets, and data extensions.
- `bleak.Martech.SalesforceMarketingCloud.MockService`: a local ASP.NET Core mock for the SFMC auth, REST, and SOAP endpoints used by this repo.
- `bleak.Martech.SalesforceMarketingCloud.Tests`: unit tests around helpers and model conversions.

## SfmcApp (.NET MAUI)

The MAUI app is the main interactive UI for the project. Right now it supports:

- saving and reusing SFMC connections locally
- browsing into an SFMC instance from a saved connection
- listing asset folders and assets
- searching assets by name or customer key
- downloading asset content and binary assets
- listing data extension folders and data extensions
- searching data extensions by name
- downloading data extension rows to CSV

The app currently targets:

- macOS via Mac Catalyst
- Windows when built on Windows

Release artifacts are published from the GitHub Releases page:

- [GitHub Releases](https://github.com/jamalkhan/bleak.Martech.SalesforceMarketingCloud/releases)

Saved MAUI connections also support optional endpoint overrides:

- `Auth Base URL`
- `REST Base URL`
- `SOAP Base URL`

Those are primarily intended for local development against the mock service.

## Mock Service

The mock service lets the existing clients run without real SFMC credentials. It currently covers:

- auth token requests
- asset folders and asset listing/search
- data extension folder retrieval
- data extension retrieval and row downloads
- data extension create/import via SOAP
- query definition retrieval
- open, click, and sent tracking retrieval
- describe requests

Run it locally with:

```bash
dotnet run --project bleak.Martech.SalesforceMarketingCloud.MockService/bleak.Martech.SalesforceMarketingCloud.MockService.csproj --urls http://127.0.0.1:5099
```

To point the MAUI app at it, use a saved connection with any placeholder SFMC credentials and set:

- `Auth Base URL`: `http://127.0.0.1:5099`
- `REST Base URL`: `http://127.0.0.1:5099`
- `SOAP Base URL`: `http://127.0.0.1:5099`

## Console App

The console app is still useful for bulk export workflows and SOAP-heavy operations.

To run it, create a `config.json` file in the console project directory. The build attempts to copy `config.json.user` to the output as `config.json`, but `config.json.user` is gitignored and must be provided locally.

Example `config.json`:

```json
{
  "OutputFolder": "/YOURDOWNLOADFOLDER",
  "Subdomain": "<SFMC-SubDomain>",
  "ClientId": "<SFMC-ClientID>",
  "ClientSecret": "<SFMC-ClientSecret>",
  "MemberId": "123456",
  "AuthBaseUrl": "",
  "RestBaseUrl": "",
  "SoapBaseUrl": "",
  "PageSize": 500,
  "MaxDegreeOfParallelism": 4,
  "Debug": false
}
```

To run the console app against the mock service, set the three base URL fields to `http://127.0.0.1:5099`. The credential values can be any non-empty placeholders in that mode.

Current console menu options:

1. Download content
2. Enumerate data extension folders
3. Enumerate data extensions
4. Write a full-path file for data extensions
5. Write a full-path file for shared data extensions
6. Download all query definitions
7. Download opens for the last 180 days
8. Download clicks for the last 180 days
9. Download sents for the last 180 days
10. Download images
11. Describe an SFMC object via SOAP

The CLI is still best when you want a batch export rather than interactive browsing.
