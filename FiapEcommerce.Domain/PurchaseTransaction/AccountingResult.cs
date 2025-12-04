using System;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public class AccountingResult
{
    public bool Success { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime AccountingDate { get; set; }
    public string Message { get; set; }
}
