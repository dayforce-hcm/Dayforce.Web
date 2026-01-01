The test web applications AspNetTest and AspNetCoreTest are built and published outside of their respective home directories.
This is done on purpose to mimic the behavior of our enterprise application. However, it means
that the AspNetTest web application cannot be started from VS IDE, because of how the latter
configures the solution applicationHost.config file. We use small in-house developed VS extension
to correct it, but including this extension is off scope for this repository.

To see that all works correctly run `dotnet build` and then `dotnet test -tl:off --no-build`. This would run the integration
tests for both Asp.Net and Asp.Net Core endpoints, which includes spawning IIS Express and Kestrel and subsequently stopping them.

Of course, one can run Kestrel and IIS Express manually or Kestrel from VS IDE. It is only running IIS Express from VS IDE that
is broken.