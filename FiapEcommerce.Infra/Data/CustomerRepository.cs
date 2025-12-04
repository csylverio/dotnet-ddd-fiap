using System;
using FiapEcommerce.Domain.CustomerRelationshipManagement;

namespace FiapEcommerce.Infra.Data;

public class CustomerRepository : ICustomerRepository
{
    public Customer GetById(int customerId)
    {
        Console.WriteLine("CustomerRepository.GetById");
        return OrderFakerGenerator.CustomerFaker;
    }
}
