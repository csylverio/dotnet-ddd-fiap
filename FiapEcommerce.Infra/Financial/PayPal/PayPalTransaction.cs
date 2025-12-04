using System;

namespace FiapEcommerce.Infra.Financial.PayPal;

public class PayPalTransaction
{
    public int Id { get; set; }

    public string Token { get; set; }

    public double Value { get; set; }
    public string Status { get; set; }
    public string Response { get; set; }

    public string Url { get; set; }
}
