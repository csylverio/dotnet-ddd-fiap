using System;

namespace LibraryExternal.SAP;

public interface ISapBapiService
{
    SapResult CreateAccountingDocument(SapAccountingDocument sapDocument);
}