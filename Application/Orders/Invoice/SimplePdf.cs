using System.Globalization;
using System.Text;

namespace ColeccionaloYa.Application.Orders.Invoice;

/// <summary>
/// Minimal, dependency-free PDF writer. Produces a single-page PDF rendering the given text lines
/// in Helvetica. This is intentionally lightweight (no third-party PDF package is used); for richer
/// invoices a dedicated library would be introduced behind this same call site.
/// </summary>
public static class SimplePdf {
	public static byte[] FromLines(IReadOnlyList<string> lines) {
		var content = BuildContentStream(lines);
		var contentBytes = Encoding.Latin1.GetBytes(content);

		var objects = new List<string> {
			"<< /Type /Catalog /Pages 2 0 R >>",
			"<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
			"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
			"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
		};

		using var ms = new MemoryStream();
		void WriteAscii(string s) {
			var bytes = Encoding.Latin1.GetBytes(s);
			ms.Write(bytes, 0, bytes.Length);
		}

		WriteAscii("%PDF-1.4\n");

		var offsets = new List<long>();

		for (var i = 0; i < objects.Count; i++) {
			offsets.Add(ms.Length);
			WriteAscii($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
		}

		// Content stream object (object 5).
		offsets.Add(ms.Length);
		WriteAscii($"5 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
		ms.Write(contentBytes, 0, contentBytes.Length);
		WriteAscii("\nendstream\nendobj\n");

		var xrefOffset = ms.Length;
		var count = offsets.Count + 1; // including the free object 0
		WriteAscii($"xref\n0 {count}\n");
		WriteAscii("0000000000 65535 f \n");
		foreach (var offset in offsets) {
			WriteAscii($"{offset.ToString("D10", CultureInfo.InvariantCulture)} 00000 n \n");
		}

		WriteAscii($"trailer\n<< /Size {count} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");

		return ms.ToArray();
	}

	private static string BuildContentStream(IReadOnlyList<string> lines) {
		var sb = new StringBuilder();
		sb.Append("BT\n/F1 11 Tf\n15 TL\n72 750 Td\n");
		for (var i = 0; i < lines.Count; i++) {
			var text = Escape(lines[i]);
			if (i == 0) {
				sb.Append($"({text}) Tj\n");
			} else {
				sb.Append($"T* ({text}) Tj\n");
			}
		}
		sb.Append("ET");
		return sb.ToString();
	}

	private static string Escape(string text) {
		return text
			.Replace("\\", "\\\\")
			.Replace("(", "\\(")
			.Replace(")", "\\)")
			.Replace("\r", " ")
			.Replace("\n", " ");
	}
}
