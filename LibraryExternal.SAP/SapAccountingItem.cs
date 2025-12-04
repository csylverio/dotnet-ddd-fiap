using System;

namespace LibraryExternal.SAP;

public class SapAccountingItem
{
    public string MaterialNumber { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string CostCenter { get; set; }
    public string GlAccount { get; set; }
    public string TaxCode { get; set; }
}
