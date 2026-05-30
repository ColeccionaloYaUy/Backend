using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Tags;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Products.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class ProductTagRepository : IProductTagRepository {
	private readonly ICConnection _Connection;

	public ProductTagRepository(ICConnection connection) {
		_Connection = connection;
	}

	public async Task<List<Tag>> GetTagsAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT t.id_tag, t.name, t.is_franchise
            FROM tag_product tp
            INNER JOIN tag t ON t.id_tag = tp.id_tag
            WHERE tp.id_product = @productId
            ORDER BY t.name";
		cmd.AddParameter("productId", productId);
		return await cmd.ExecuteSelectList<Tag>(rs => new Tag {
			Id = rs.GetValue<int>("id_tag"),
			Name = rs.GetValue<string>("name"),
			IsFranchise = rs.GetValue<bool>("is_franchise"),
		}, cancellationToken);
	}

	public async Task<bool> AnyLinkedAsync(int productId, IReadOnlyCollection<int> tagIds, CancellationToken cancellationToken) {
		if (tagIds.Count == 0) {
			return false;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM tag_product tp
            WHERE tp.id_product = @productId AND tp.id_tag = ANY(@tagIds)";
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("tagIds", tagIds.ToArray());
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> LinkExistsAsync(int productId, int tagId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM tag_product tp
            WHERE tp.id_product = @productId AND tp.id_tag = @tagId";
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("tagId", tagId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task AddAsync(int productId, IReadOnlyCollection<int> tagIds, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO tag_product (id_tag, id_product)
            SELECT t, @productId FROM UNNEST(@tagIds) AS t";
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("tagIds", tagIds.ToArray());
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RemoveAsync(int productId, int tagId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM tag_product WHERE id_product = @productId AND id_tag = @tagId";
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("tagId", tagId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
