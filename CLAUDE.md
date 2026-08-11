# Project rules for Claude

## What this is

CalDAVNet is a small .NET class library that talks to CalDAV servers over HTTP. It is a fork of
[markatk/CalDAV.NET](https://github.com/markatk/CalDAV.NET). It is published on NuGet as
[HaemmerElectronics.SeppPenner.CalDAVNet](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/),
so every change to the public API is a breaking change for consumers.

One solution `src/CalDAVNet.sln` with exactly three projects:

- `src/CalDAVNet/CalDAVNet.csproj`: the library. Assembly, root namespace and folder are all
  `CalDAVNet`, the package id is `HaemmerElectronics.SeppPenner.CalDAVNet`.
- `src/CalDAVNet.Examples/CalDAVNet.Examples.csproj`: `OutputType` `Exe`, a demo that calls every
  public method of `Client` once against a hard coded server address. It is not packed and it is
  not meant to be run without a CalDAV server.
- `src/CalDAVNet.Tests/CalDAVNet.Tests.csproj`: MSTest, added in version 1.0.4.0.

Layout inside `src/CalDAVNet`:

- `Client.cs`, `Client.Calendar.cs`, `Client.Events.cs`: the public entry point. One `partial class
  Client` in three files. `Client.cs` has the constructors and the shared fields,
  `Client.Calendar.cs` the calendar queries (`GetAllCalendars`, `GetCalendarByUid`,
  `GetDefaultCalendar` plus the private helpers), `Client.Events.cs` the event methods
  (`AddOrUpdateEvent`, `DeleteEvent`). Keep new members in that split.
- `CalDAVClient.cs`: internal. Wraps the `HttpClient`, holds the credentials and turns a uri into a
  `Request<T>` for GET, PUT, DELETE, PROPFIND and REPORT.
- `Model/`: `Request<T>`, `Response`, `ResourceResponse` and `Resource`, all internal. `Request<T>`
  is a fluent builder (`WithMethod`, `WithContent`, `WithXmlContent`, `WithHeader`, `Send`),
  `ResourceResponse.Parse` turns the multistatus XML into `Resource` objects with a
  `XName` to `string` property dictionary.
- `Model/Public/Calendar.cs`: the only public model, a `sealed record class`.
- `Constants/`: `DocumentConstants`, `ElementNames`, `HeaderNames`, `HeaderValues`, `HttpMethods`
  and `Namespaces`. All internal except `Namespaces`, which is public. Never write an element name,
  header or namespace as a literal at the call site, it belongs in one of these classes.
- `Extensions/`: `CalendarEventExtensions` (public, `ToJsonString`), `XDocumentExtensions` and
  `XElementExtensions` (internal).
- `GlobalUsings.cs`: all usings of the project.

The whole public surface is `Client`, `Model.Public.Calendar`, `CalendarEventExtensions`,
`Namespaces` and the Ical.Net types that appear in those signatures (`CalendarEvent`,
`Ical.Net.Calendar`). Everything else is internal on purpose, do not widen it without a reason.

`src/CalDAVNet.Examples` holds only `Program.cs` and its own `GlobalUsings.cs`.

Layout inside `src/CalDAVNet.Tests`:

- `CalDAVClientTests.cs`: the headers and the credentials that every request carries, that a second
  client does not take over the requests of the first one, the depth of zero, the uri combination
  and the HTTP methods.
- `RequestTests.cs`: the replacing `WithHeader`, the XML content and its missing declaration, and
  `Send` against a fake handler.
- `ResourceResponseTests.cs`: the multistatus parsing, the dropped properties of a failed propstat,
  the inner XML of a property, a body that is no XML, the successful status codes.
- `XElementExtensionsTests.cs`: the local name lookup, the inner XML, the status code and the
  description.
- `ClientTests.cs`, `CalendarEventExtensionsTests.cs`: the two constructors and the argument check,
  the JSON of an event.
- `TestDataProvider.cs`: the credentials, the base uri and the multistatus answer both response
  tests work on. `FakeHttpMessageHandler.cs`: answers with a canned response and remembers the
  request, no test opens a connection.
- `GlobalUsings.cs`: all usings of the test project.

The test project reaches the internal types through `<InternalsVisibleTo Include="CalDAVNet.Tests" />`
in the library csproj, that is where `Request<T>`, `ResourceResponse` and the extensions live.

Repository root: `README.md` (the only user documentation, with the usage sample), `Changelog.md`,
`Updating.md` (the release steps), `License.txt` (MIT), `Icon.png` (the package icon),
`Calendar.ico`, `BuildAndPushPackage.bat`, `Delete-BIN-OBJ-Folders.bat`, `.gitattributes` and
`.gitignore`. The editorconfig lives in `src/.editorconfig`. There is no `.github` folder, no
`HowToUse.md` and no screenshots.

## Build

```powershell
dotnet build src/CalDAVNet.sln
```

```powershell
dotnet test src/CalDAVNet.sln
```

- The library multi targets `net8.0;net10.0`, the example project is `net10.0` only. Nothing in the
  code is Windows specific, the two batch files are.
- All build properties live directly in the two `.csproj` files. There is **no**
  `Directory.Build.props` in this repository, unlike the sibling repository
  `CSharpLanguageManager`.
- `GeneratePackageOnBuild` is on, so **every** build packs: a `.nupkg` and a `.snupkg` appear in
  `src/CalDAVNet/bin/<configuration>`, in Debug too.
- `TreatWarningsAsErrors` is enabled in both projects, so every warning breaks the build, NuGet
  warnings (`NU****`) from restore and pack included. A clean build reports zero warnings, keep it
  that way.
- `NU1803` (HTTP source usage during restore) is the one warning suppressed via `NoWarn`. Fix
  warnings instead of extending that list. `NuGetAudit` and `NuGetAuditMode=all` are on in the
  library project, so a vulnerable transitive package fails the build too.
- `GenerateDocumentationFile` is on in the library project, so the package ships `CalDAVNet.xml`
  next to the assembly and a wrong `<param>` name is a build error.
- Versions come from GitVersion.MsBuild out of the git tags, for example `1.0.4-3` for the third
  commit after tag `1.0.3`. Never edit a version property or an assembly version by hand.
- Restore needs nuget.org. If a private feed is configured globally on the machine and answers 404
  for public packages, restore fails with `NU1301`. Then build with an explicit source:
  `dotnet build src/CalDAVNet.sln --source https://api.nuget.org/v3/index.json`.
- `Delete-BIN-OBJ-Folders.bat` deletes all `bin` and `obj` folders below the repository root.
- Tests are MSTest, in the single test project `src/CalDAVNet.Tests`, which follows the same package
  set as the sibling repositories: `Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`,
  `MSTest.TestFramework`, `coverlet.collector` and `GitVersion.MsBuild`. `dotnet test` runs 31
  tests, they need no network: requests are built and inspected without being sent, and the one
  test that sends uses a fake message handler. Never claim a test run happened without running it.
- Everything below `GetAllCalendars`, `GetDefaultCalendar` and `AddOrUpdateEvent` needs a server to
  answer, the tests stop at the request. A change to a request or to the parsing of an answer is
  verified against a real CalDAV server or a captured response.

## Code conventions

Follow the surrounding code, it is consistent throughout every file:

- File header comment block with `<copyright file="..." company="Hämmer Electronics">` and a
  `<summary>`, then the file-scoped namespace.
- XML doc comments on every type and every member, private members included, no exceptions.
- `Nullable`, `ImplicitUsings` and `LangVersion latest` are enabled.
- New `using` directives go into the `GlobalUsings.cs` of the respective project, inside the
  existing `#pragma warning disable IDE0065` block, never at the top of a file. The editorconfig
  requires usings inside the namespace (`csharp_using_directive_placement=inside_namespace:warning`),
  which global usings cannot satisfy, that is what the pragma is for. Do not add other pragmas. The
  comment text in that block is German because Visual Studio generated it, leave it alone.
- Fields, properties, methods and events are always accessed with `this.` qualification
  (`dotnet_style_qualification_for_*` at severity `warning`).
- Regular expressions are source generated: a `[GeneratedRegex]` partial method plus a
  `readonly Regex` field that calls it. That is why `XElementExtensions` is `partial`, `Client` is
  partial anyway because it is split over three files.
- `src/.editorconfig` also enforces braces everywhere, no multiple blank lines, four spaces, CRLF,
  UTF-8, file scoped namespaces, `System` usings sorted first and `IDE0005` as warning. Analyzer
  warnings are fixed, not silenced.

## Known quirks

Do not silently "clean up" these, they are existing behaviour:

- **`GetCalendarByUid` does not take the uid it hands out.** It forwards its argument unchanged to
  the private `GetCalendarWithUri`, so the caller has to pass a uri such as
  `/caldav.php/user/uniqueid/`, exactly as the sample in `README.md` does. The `Calendar.Uid`
  property is something else, `DeserializeCalendarResource` builds it from the uri by removing the
  owner and all slashes. Renaming the parameter or making the two match is a breaking change.
- **The XML declaration is built and then dropped.** `Request<T>.WithXmlContent` creates an
  `XDocument` with an `XDeclaration("1.0", "UTF-8", null)`, but `XDocument.ToString()` never writes
  a declaration, so the body that goes out starts with the root element. Servers accept that, the
  declaration is dead weight. `DocumentConstants` exists only for it.
- **`Depth` is a default header.** `CalDAVClient` sets `Depth: 1` for every request, the two calls
  that need `Depth: 0` override it per request with `WithHeader`. A new request that needs another
  depth has to do the same.
- **Two identical regexes.** `Client.tagRegex` (instance field, from `Client.cs`) and
  `Client.hrefRegex` (static field, from `Client.Calendar.cs`) both compile `<[^>]*(>|$)` and both
  strip XML tags out of a property value. They come from the upstream fork.
- **`Namespaces.ICalNs` is public and unused.** It is part of the public API, removing it is a
  breaking change.
- **`Calendar.ico` is tracked and referenced nowhere.** `Icon.png` is the package icon,
  the `.ico` file is a leftover.
- **The example prints nothing.** `Program.Main` assigns every result to a local variable and never
  uses it. Judge a run by the requests that go out, not by stdout. The address
  `http://192.168.2.2/caldav.php/user/someid` is a placeholder.
- **`ToJsonString` writes a projection, not the event.** The `CalendarEvent` itself cannot be handed
  to `JsonSerializer`, its object graph carries a `System.Text.Encoding`, whose `Preamble` the
  serializer refuses, so the extension writes the fields that describe the event. Up to version
  1.0.3.0 it passed the event itself and threw an `InvalidOperationException` every single time.
- **Ical.Net types are part of the public API.** `AddOrUpdateEvent`, `DeleteEvent` and
  `Calendar.Events` speak `CalendarEvent` and `Ical.Net.Calendar`, so a major version bump of
  Ical.Net is a breaking change for consumers, not an internal update.
- **AppVeyor badge without CI in the repository.** `README.md` links an AppVeyor build that is
  configured outside of this repository. There is no `.github` folder and no pipeline file here.
- **`.gitattributes` sets `* text=auto`** and every rule of the Visual Studio template below it is
  commented out. A binary file that git could misread needs its own rule.

## Releasing

The steps are the ones in `Updating.md`:

1. Make the change.
2. Update `PackageReleaseNotes` in `src/CalDAVNet/CalDAVNet.csproj` to the new version line.
3. Add an entry at the top of `Changelog.md` in the existing format:
   `* **Version 1.0.4.0 (2026-08-11)** : Short description.`
4. Commit that.
5. Tag the commit with the plain version number, no `v` prefix (`1.0.3`, `1.0.2`, ...). The existing
   tags are lightweight tags, create new ones the same way. GitVersion.MsBuild derives the assembly
   and package version from the tag.
6. `BuildAndPushPackage.bat` cleans `bin` and `obj`, builds Release and pushes `.nupkg` and
   `.snupkg` to nuget.org and to github. It needs `NUGET_API_KEY` and `GITHUB_API_KEY` in the
   environment. Never run it unless explicitly asked to publish.

The version in `Changelog.md` and in `PackageReleaseNotes` has four parts (`1.0.4.0`), the tag has
three (`1.0.4`). An untagged commit produces something like `1.0.4-3`.

Also update the "Available for" list in `README.md` when the target frameworks change.

## Git

- **Never amend a commit.** No `git commit --amend`, not for a typo in the message, not to add a
  forgotten file, not even when the commit is still local. Write a follow-up commit instead. The
  release versions come from tags on exact commits, an amended commit leaves its tag pointing at a
  commit that no longer exists in the branch.

## Writing style

- Commit messages are written **in English only**: short, precise subject line, explanatory body
  when needed.
- Code comments and comments in project files such as `.csproj` are **always English**, regardless
  of the language used in the conversation.
- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments nor
  documentation. Use a regular hyphen, comma, colon, parentheses or a separate sentence.
- German texts (documentation, chat replies) always use real umlauts and ß, never ASCII
  transliterations such as `ae`, `oe`, `ue` or `ss`. Identifiers, file names and configuration keys
  stay unchanged where umlauts are technically undesirable.
