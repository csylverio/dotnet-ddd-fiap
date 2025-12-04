using System;

namespace FiapEcommerce.Infra.Financial.PagSeguro;

public class PagSeguroService
{
    public string GenerateFile()
    {
        return "filename.txt";
    }

    public bool SendFile(decimal amount)
    {
        Console.WriteLine($"[PagSeguro] Abrindo conexao com SFTP (dados da conexão no appSettings?).");
        Console.WriteLine($"[PagSeguro] Criando arquivo SFTP.");
        Console.WriteLine($"[PagSeguro] Pagamento de ${amount} enviado para SFTP.");
        return true;
    }
}
