using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Addresses;
using ColeccionaloYa.Persistence.Addresses.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Addresses.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class AddressRepository : IAddressRepository {
	private readonly ICConnection _Connection;

	public AddressRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static void Map(Address obj, ICDataReader rs) {
		obj.Id = rs.GetValue<int>("id_address");
		obj.ClientId = rs.GetValue<int>("id_client");
		obj.Street = rs.GetValue<string>("street");
		obj.Number = rs.GetValue<string>("number");
		obj.City = rs.GetValue<string>("city");
		obj.Department = rs.GetValue<string>("department");
		obj.PostalCode = rs.GetValue<string>("postal_code");
		obj.Type = Enum.Parse<AddressType>(rs.GetValue<string>("type"), ignoreCase: true);
		obj.LogicalDelete = rs.GetValue<bool>("logical_delete");
	}

	public async Task<List<Address>> GetByClientIdAsync(int clientId) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT a.id_address, a.id_client, a.street, a.number, a.city, a.department,
                   a.postal_code, a.type::text AS type, a.logical_delete
            FROM address a
            WHERE a.id_client = @clientId AND a.logical_delete = FALSE
            ORDER BY a.id_address";
		cmd.AddParameter("clientId", clientId);
		return await cmd.ExecuteSelectList<Address>(Map);
	}

	public async Task<Address?> GetByIdAsync(int id) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT a.id_address, a.id_client, a.street, a.number, a.city, a.department,
                   a.postal_code, a.type::text AS type, a.logical_delete
            FROM address a
            WHERE a.id_address = @id AND a.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Address>(Map);
	}

	public async Task CreateAsync(Address address) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO address (id_client, street, number, city, department, postal_code, type, logical_delete)
            VALUES (@clientId, @street, @number, @city, @department, @postalCode, @type::type_address, @logicalDelete)
            RETURNING id_address";
		cmd.AddParameter("clientId", address.ClientId);
		cmd.AddParameter("street", address.Street);
		cmd.AddParameter("number", address.Number);
		cmd.AddParameter("city", address.City);
		cmd.AddParameter("department", address.Department);
		cmd.AddParameter("postalCode", address.PostalCode);
		cmd.AddParameter("type", address.Type.ToString().ToLower());
		cmd.AddParameter("logicalDelete", address.LogicalDelete);

		var newId = await cmd.ExecuteGetValue<int>("id_address");
		address.AssignId(newId);
	}

	public async Task UpdateAsync(Address address) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE address
            SET street = @street,
                number = @number,
                city = @city,
                department = @department,
                postal_code = @postalCode,
                type = @type::type_address
            WHERE id_address = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", address.Id);
		cmd.AddParameter("street", address.Street);
		cmd.AddParameter("number", address.Number);
		cmd.AddParameter("city", address.City);
		cmd.AddParameter("department", address.Department);
		cmd.AddParameter("postalCode", address.PostalCode);
		cmd.AddParameter("type", address.Type.ToString().ToLower());

		await cmd.ExecuteCommandNonQuery();
	}

	public async Task LogicalDeleteAsync(int id) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE address
            SET logical_delete = TRUE
            WHERE id_address = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery();
	}
}
