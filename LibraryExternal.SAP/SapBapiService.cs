using System;

namespace LibraryExternal.SAP;

public class SapBapiService : ISapBapiService
{
    public SapResult CreateAccountingDocument(SapAccountingDocument sapDocument)
    {
        Console.WriteLine($"[SapBapiService] Calling accounting function {sapDocument}");
        return new SapResult
        {
            Success = true,
            DocumentNumber = "1234567890"
        };
    }
}
