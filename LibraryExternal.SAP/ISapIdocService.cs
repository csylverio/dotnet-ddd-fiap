using System;

namespace LibraryExternal.SAP;

public interface ISapIdocService
{
    SapResult SendAccountingDocument(SapAccountingDocument sapDocument);
}
