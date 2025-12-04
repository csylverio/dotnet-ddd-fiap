using System;

namespace LibraryExternal.SAP;

public class SapIdocService : ISapIdocService
{
    public SapResult SendAccountingDocument(SapAccountingDocument sapDocument)
    {
        Console.WriteLine($"[SapIdocService] Calling accounting function {sapDocument}");
        return new SapResult
        {
            Success = true,
            DocumentNumber = "1234567890"
        };
    }
}
