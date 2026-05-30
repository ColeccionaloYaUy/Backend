namespace ColeccionaloYa.Domain.Books;

public class Book {
	public int ProductId { get; internal set; }
	public BookType Type { get; internal set; }

	internal Book() { }

	public static Book Create(int productId, BookType type) {
		return new Book {
			ProductId = productId,
			Type = type,
		};
	}

	public void ChangeType(BookType type) {
		Type = type;
	}
}
