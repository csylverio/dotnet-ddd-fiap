using System;

namespace LibraryExternal.SAP;

public interface ISapRfcService
{
    SapResult CallAccountingFunction(SapAccountingDocument sapDocument);
}
