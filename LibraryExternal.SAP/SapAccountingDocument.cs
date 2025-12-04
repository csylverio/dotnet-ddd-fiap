namespace LibraryExternal.SAP;

public class SapAccountingDocument
{
    public string CompanyCode { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentType { get; set; }
    public string Currency { get; set; }
    public string Reference { get; set; }
    public List<SapAccountingItem> Items { get; set; }
}
