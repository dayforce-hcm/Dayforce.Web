namespace System.Web.Mvc;

public abstract class AreaRegistration
{
    public abstract string AreaName { get; }

    public abstract void RegisterArea(AreaRegistrationContext context);
}

public class AreaRegistrationContext
{
    public void MapRoute(string name, string url, object defaults)
    {
        throw new NotImplementedException();
    }
}

public static class UrlParameter
{
    public static readonly object Optional;
}