CalDAVNet
====================================

CalDAVNet is a project to access CalDAV servers and a fork of https://github.com/markatk/CalDAV.NET.

[![Build status](https://ci.appveyor.com/api/projects/status/0rsrutr4in3chb3k?svg=true)](https://ci.appveyor.com/project/SeppPenner/caldavnet)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/CalDAVNet.svg)](https://github.com/SeppPenner/CalDAVNet/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/CalDAVNet.svg)](https://github.com/SeppPenner/CalDAVNet/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/CalDAVNet.svg)](https://github.com/SeppPenner/CalDAVNet/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/SeppPenner/CalDAVNet/master/License.txt)
[![Nuget](https://img.shields.io/badge/CalDAVNet-Nuget-brightgreen.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HaemmerElectronics.SeppPenner.CalDAVNet.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/CalDAVNet/badge.svg)](https://snyk.io/test/github/SeppPenner/CalDAVNet)
[![Gitter](https://img.shields.io/matrix/CalDAVNet%3Agitter.im?server_fqdn=matrix.org)](https://matrix.to/#/#CalDAVNet:gitter.im)
[![Blogger](https://img.shields.io/badge/Follow_me_on-blogger-orange)](https://franzhuber23.blogspot.de/)
[![Patreon](https://img.shields.io/badge/Patreon-F96854?logo=patreon&logoColor=white)](https://patreon.com/SeppPennerOpenSourceDevelopment)
[![PayPal](https://img.shields.io/badge/PayPal-00457C?logo=paypal&logoColor=white)](https://paypal.me/th070795)

## Available for
* Net 8.0
* Net 9.0

## Net Core and Net Framework latest and LTS versions
* https://dotnet.microsoft.com/download/dotnet

## Basic usage
```csharp
using Ical.Net.CalendarComponents;

/// <summary>
/// The main method.
/// </summary>
public static async Task Main()
{
    // Create client.
    var calDavClient = new Client("http://192.168.2.2/caldav.php/user/someid", "user", "password");

    // Get all calendars for the user.
    var calendars = await calDavClient.GetAllCalendars();

    // Get the calendar by the uid.
    var calendarByUid = await calDavClient.GetCalendarByUid("/caldav.php/user/uniqueid/");

    // Get the default calendar.
    var defaultCalendar = await calDavClient.GetDefaultCalendar();

    // Add an event.
    var calendarEvent = new CalendarEvent();
    var added = await calDavClient.AddOrUpdateEvent(calendarEvent, new Ical.Net.Calendar());

    // Delete an event.
    var deleted = await calDavClient.DeleteEvent(calendarEvent);
}
```

## NuGet
The project can be found on [nuget](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/).

## Install

```bash
dotnet add package HaemmerElectronics.SeppPenner.CalDAVNet
```

Change history
--------------

See the [Changelog](https://github.com/SeppPenner/CalDAVNet/blob/master/Changelog.md).
