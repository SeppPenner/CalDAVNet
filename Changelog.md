Change history
--------------

* **Version 1.1.0.0 (2026-11-24)** : Removed support for Net8.0 as it is out of support, added support for Net11.0, updated NuGet packages, the calendars are found through the calendar home set of the principal now (fixes SOGo), the event query filters on VEVENT, calendar data is decoded before it reaches Ical.Net, added GetEvents with a time range. The new public API is why the minor version is raised.
* **Version 1.0.4.0 (2026-08-11)** : Removed support for Net9.0, added support for Net10.0, updated the NuGet packages (the update to Ical.Net 5.2.3 is a breaking change), fixed the credentials and the headers of the shared HTTP client, the uid of a calendar without an owner, the event url, the culture dependent date parsing and ToJsonString, added a test project.
* **Version 1.0.3.0 (2024-11-24)** : Removed support for Net6.0, added support for Net9.0, updated NuGet packages.
* **Version 1.0.2.0 (2024-05-16)** : Removed support for Net7.0.
* **Version 1.0.1.0 (2023-12-07)** : Updated NuGet packages, added support for Net8.0.
* **Version 1.0.0.0 (2023-11-11)** : 1.0 release.