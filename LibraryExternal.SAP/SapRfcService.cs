using System;

namespace LibraryExternal.SAP;

public class SapRfcService : ISapRfcService
{
    public SapResult CallAccountingFunction(SapAccountingDocument sapDocument)
    {
        Console.WriteLine($"[SapRfcService] Calling accounting function {sapDocument}");
        return new SapResult
        {
            Success = true,
            DocumentNumber = "1234567890"
        };
    }
}
