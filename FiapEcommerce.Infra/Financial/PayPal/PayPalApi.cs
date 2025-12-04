using System;

namespace FiapEcommerce.Infra.Financial.PayPal;

public class PayPalApi
{
    public Guid Login(string user, string password)
    {
        Console.WriteLine($"[PayPal] Login realizado com sucesso.");
        return Guid.NewGuid();
    }

    public PayPalTransaction OpenTransaction(string token)
    {
        Console.WriteLine($"[PayPal] Transação aberta com sucesso para token: {token}");

        // Simulando retorno de uma chamada a API
        PayPalTransaction transaction = new PayPalTransaction();
        transaction.Id = new Random().Next(1000, 9999);
        transaction.Token = token;

        return transaction;
    }

    public PayPalTransaction SendPayment(PayPalTransaction transaction)
    {
        Console.WriteLine($"[PayPal] Pagamento de ${transaction.Value} enviado com sucesso para transação: {transaction.Id}.");

        transaction.Status = "OK";
        transaction.Response = "raw_response";
        transaction.Url = "https://www.google.com";

        return transaction;
    }
}
