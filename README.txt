
# Municipal Connect (ASP.NET Core MVC, .NET 8)

Citizen-facing **municipal services** prototype focused on **Report Issues**.

This project uses (no arrays/lists): `SortedDictionary<string, IssueReport>` in Session, `HashSet<string>` for attachment paths, and a `Dictionary<IssueCategory,string>` for tracking‑ID prefixes.

**Status**: Part 1 — *Report Issues* is implemented. *Local Events & Announcements* and *Service Request Status* are visible but disabled (to be implemented).

-----------------
## Features
-----------------

- **Main menu** with three options.
- **Report Issues** form: Location, Category, Description, multi‑file attachments.
- **Engagement UX**: client‑side completeness meter, success screen with clear next steps.
- **Human‑friendly Tracking ID**: `MC-<prefix>-yyyyMMdd-####` (e.g., `MC-WT-20250909-0001`).
- **Session-backed storage**: map + set.
- **File uploads** saved to `wwwroot/uploads/{TrackingId}` with links on the success page.

-----------------
## Prerequisites
-----------------

- **.NET SDK 8.0** — https://dotnet.microsoft.com/download
- **Visual Studio 2022** (17.8+) with “ASP.NET and web development” 
- HTTPS dev cert (CLI will prompt to trust on first run)

-----------------
## How to Run
-----------------

### Option A — Clone Github Repo - Visual Studio 2022

1. Open Visual Studio Community 2022
2. Select Clone GitHub Repository
3. Copy and past the GitHub repository link below 
4. Choose where to save local folder
5. Project will open
6. Click small green play button at the top and project will open and run.
7. If prompted for SSL certificate, select yes options to install
8. Program will run in localhost

### Option B — Open from Zip File - Visual Studio 2022

1. Unzip the project file once downloaded
2. Open Microsoft Visual Studio Community 2022
3. Select open local folder
4. Open the project folder you have unzipped.
5. Click small green play button at the top and project will open and run.
6. If prompted for SSL certificate, select yes options to install
7. Program will run in localhost

-----------------
## Important Links
-----------------

- GITHUB - https://github.com/VCCT-PROG7312-2025-G1/Shaun_Faulkner_ST10034664_PROG7312_MunicipalConnect
