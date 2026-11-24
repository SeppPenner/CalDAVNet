cd .\src

@ECHO off
cls

FOR /d /r . %%d in (bin,obj) DO (
	IF EXIST "%%d" (		 	 
		ECHO %%d | FIND /I "\node_modules\" > Nul && ( 
			ECHO.Skipping: %%d
		) || (
			ECHO.Deleting: %%d
			rd /s/q "%%d"
		)
	)
)

@ECHO on
@ECHO.Building solution...
@dotnet restore
@dotnet build -c Release
@cd .\CalDAVNet\bin\Release
@ECHO.Build successful.
@REM The four push lines are prefixed with @ on purpose. Without it, cmd echoes the command
@REM line after the variable expansion, which writes both API keys to the console and into
@REM every log or transcript of the run.
@dotnet nuget push *.nupkg -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
@dotnet nuget push *.snupkg -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
@dotnet nuget push *.nupkg -s "github" --skip-duplicate --api-key "%GITHUB_API_KEY%"
@dotnet nuget push *.snupkg -s "github" --skip-duplicate --api-key "%GITHUB_API_KEY%"
@ECHO.Upload success. Press any key to exit.
PAUSE