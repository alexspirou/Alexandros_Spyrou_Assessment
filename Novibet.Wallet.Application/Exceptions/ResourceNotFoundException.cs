using System.Net;

namespace Novibet.Wallet.Application.Exceptions;

public class ResourceNotFoundException : BaseWalletException
{
    public ResourceNotFoundException(string resourceName)
        : base($"{resourceName} was not found.", HttpStatusCode.NotFound)
    {
        ResourceName = resourceName;
    }

    public string ResourceName { get; }
}

