using System;

namespace FiapEcommerce.Domain.CustomerRelationshipManagement;

public interface ICustomerRepository
{
    Customer GetById(int customerId);
}
