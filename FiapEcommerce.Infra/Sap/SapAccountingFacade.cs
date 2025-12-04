using System;
using LibraryExternal.SAP;
using FiapEcommerce.Domain.PurchaseTransaction;

namespace FiapEcommerce.Infra.Sap;

public class SapAccountingFacade : IAccountingService
{
    private readonly ISapBapiService _sapBapiService;
    private readonly ISapIdocService _sapIdocService;
    private readonly ISapRfcService _sapRfcService;

    public SapAccountingFacade(ISapBapiService sapBapiService, ISapIdocService sapIdocService, ISapRfcService sapRfcService)
    {
        _sapBapiService = sapBapiService;
        _sapIdocService = sapIdocService;
        _sapRfcService = sapRfcService;
    }

    public AccountingResult RegisterSale(Order order)
    {
        try
        {
            // 1. Converter o pedido para o formato SAP
            var sapDocument = ConvertToSapDocument(order);

            // 2. Tentar registrar via BAPI (método preferencial)
            var bapiResult = _sapBapiService.CreateAccountingDocument(sapDocument);

            if (bapiResult.Success)
            {
                return new AccountingResult
                {
                    Success = true,
                    DocumentNumber = bapiResult.DocumentNumber,
                    AccountingDate = DateTime.UtcNow
                };
            }

            // 3. Se BAPI falhar, tentar via IDOC
            var idocResult = _sapIdocService.SendAccountingDocument(sapDocument);

            if (idocResult.Success)
            {
                return new AccountingResult
                {
                    Success = true,
                    DocumentNumber = idocResult.DocumentNumber,
                    AccountingDate = DateTime.UtcNow
                };
            }

            // 4. Como último recurso, tentar via RFC
            var rfcResult = _sapRfcService.CallAccountingFunction(sapDocument);

            return new AccountingResult
            {
                Success = rfcResult.Success,
                DocumentNumber = rfcResult.DocumentNumber,
                AccountingDate = DateTime.UtcNow,
                Message = "Contabilidade aplicada no sistema RFC"
            };
        }
        catch (Exception ex)
        {
            return new AccountingResult
            {
                Success = false,
                Message = $"Falha na integração com SAP: {ex.Message}"
            };
        }
    }

    private SapAccountingDocument ConvertToSapDocument(Order order)
    {
        return new SapAccountingDocument
        {
            CompanyCode = "BR01",
            DocumentDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            DocumentType = "RV", // Tipo documento de receita
            Currency = "BRL",
            Reference = order.Id.ToString(),
            Items = order.Items.Select(i => new SapAccountingItem
            {
                MaterialNumber = i.Product.Id.ToString(),
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                CostCenter = GetCostCenter(i),
                GlAccount = GetGlAccount(i),
                TaxCode = GetTaxCode(i)
            }).ToList()
        };
    }

    private string GetCostCenter(Item item) => /* lógica para determinar centro de custo */ "CC1000";
    private string GetGlAccount(Item item) => /* lógica para conta contábil */ "41100001";
    private string GetTaxCode(Item item) => /* lógica para código de imposto */ "ICMS10";

}
