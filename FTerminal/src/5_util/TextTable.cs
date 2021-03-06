using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Limcap.FriendlyTerminal {
	public class TextTable {

		public enum RowStyle { Content, OneCell, Line, LineTop, LineBottom, LineSimple, Empty, Blank }
		public static Brush defaultColor = Brushes.White;

		public int[] columnsSizes;
		public bool[] columnsFlex;
		public string[] headers;
		public Brush gridColor;
		public Brush headerColor;
		public Brush textColor;
		public readonly List<Cell[]> rows;
		private Cell[] currentRow;
		private int currentCol;
		public string noContentText = "Nada a exibir";

		public TextTable( params int[] columnsSizes ) {
			this.columnsSizes = columnsSizes;
			headers = new string[columnsSizes.Length];
			gridColor = headerColor = textColor = defaultColor;
			rows = new List<Cell[]>();
			// Define se o tamanho da coluna é flexivel
			columnsFlex = new bool[columnsSizes.Length];
			for (int i = 0; i < columnsSizes.Length; i++)
				columnsFlex[i] = columnsSizes[i] == 0;
		}

		public TextTable( params string[] headers )
		: this( headers.Select( s => 0 ).ToArray() ) {
			SetHeaders( headers );
			for (int i = 0; i < headers.Count(); i++) {
				columnsSizes[i] = headers[i].Length;
				columnsFlex[i] = true;
			}
		}

		public int TotalWidth { get => columnsSizes.Aggregate( 0, ( a, o ) => a += o ) + (columnsSizes.Length - 1) * 3; }

		public TextTable SetHeaders( params string[] headers ) {
			for (int i = 0; i < columnsSizes.Length; i++) {
				if (headers.Length - 1 >= i) this.headers[i] = headers[i];
			}
			return this;
		}

		public TextTable SetGridColor( Brush gridColor ) {
			this.gridColor = gridColor;
			return this;
		}

		public TextTable SetTextColor( Brush textColor ) {
			this.textColor = textColor;
			return this;
		}

		public TextTable SetHeaderColor( Brush headerColor ) {
			this.headerColor = headerColor;
			return this;
		}




		public Cell[] GetRow( int lineNumber = -1 ) {
			Cell[] line;
			if (lineNumber < 0) {
				line = new Cell[columnsSizes.Length];
				rows.Add( line );
			}
			else {
				if (lineNumber > rows.Count - 1)
					throw new IndexOutOfRangeException( "Line index is out of range: " + lineNumber );
				else
					line = rows[lineNumber];
			}
			return line;
		}



		public TextTable Row( int rowIndex, params Cell[] lineContent ) {
			var row = GetRow( rowIndex );
			for (int i = 0; i < row.Length; i++) {
				row[i] = lineContent.Length > i ? lineContent[i] : string.Empty;
			}
			currentRow = row;
			currentCol = 0;
			return this;
		}
		public TextTable Row( params Cell[] lineContent ) {
			return Row( -1, lineContent );
		}
		// Linha junta: A primeira cell tem o texto e a segunda é null quer dizer que a primeira cell deve ser do tamanho 
		// da tabela
		public TextTable Row( RowStyle style, string text = null ) {
			return Row( -1, new Cell[] { new Cell( text, style ), null } );
		}


		public TextTable Col( int colIndex, Brush color, string text ) {
			currentCol = colIndex == -1 ? currentCol : colIndex;
			currentRow[currentCol] = new Cell( color, text );
			
			// aumenta a largura da coluna se ela for do tipo 'largura variável'
			if (columnsFlex[currentCol])
				columnsSizes[currentCol] = Math.Max( columnsSizes[currentCol], text?.Length ?? 0);
			
			currentCol++;
			return this;
		}
		public TextTable Col( int colIndex, string text ) {
			return Col( colIndex, textColor, text );
		}
		public TextTable Col( Brush color, string text = null ) {
			return Col( -1, color, text );
		}
		public TextTable Col( string text = null ) {
			return Col( -1, textColor, text );
		}


		public TextTable SeparatorLine() {
			rows.Add( null );
			//Row();
			//for (int column = 0; column < columnsSizes.Length; column++)
			//	Col( "─".PadRight( columnsSizes[column], '─' ) );
			return this;
		}




		public string Print( bool showHeader = true, bool showTopBar = true ) {
			// remove os sinais de negativo do columnsizes
			//for (int i = 0; i < columnsSizes.Length; i++)
			//	if (columnsSizes[i] < 0) columnsSizes[i] = columnsSizes[i]*-1;
			// imprime boda inicial
			StringBuilder sb = new StringBuilder();
			//Console.ForegroundColor = gridColor;
			if( showTopBar ) 
				sb.AppendLine( string.Empty.PadRight( TotalWidth, '═' ) );

			// imprime cabeçalho
			if( showHeader ) {
				for (int i = 0; i < headers.Length; i++) {
					var head = headers[i];
					var colSize = columnsSizes[i] < 0 ? columnsSizes[i] * -1 : columnsSizes[i];
					//Console.ForegroundColor = headerColor;
					sb.Append( head.FixedSize( colSize ) );
					if (i < headers.Length - 1) {
						//Console.ForegroundColor = gridColor;
						sb.Append( " │ " );
					}
					else
						sb.AppendLine();
				}

				// imprime divisoria
				//Console.ForegroundColor = gridColor;
				for (int i = 0; i < columnsSizes.Length; i++) {
					var colSize = columnsSizes[i] < 0 ? columnsSizes[i] * -1 : columnsSizes[i];
					sb.Append( string.Empty.FixedSize( colSize, '─' ) );
					sb.Append( i < headers.Length - 1 ? "─┼─" : "\n" );
				}
			}

			// imprime conteudo
			for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++) {
				var row = rows[rowIdx];
				if (row is null) PrintLine( sb );
				else if (row.Count() == 2 ) {//&& row[1] is null
					if ( row[0].style == RowStyle.OneCell ) PrintRowAsTitle( row, sb );
					else if( row[0].style == RowStyle.Blank ) sb.AppendLine();
					else if( row[0].style == RowStyle.Empty ) PrintEmptyLine( sb );
					else if( row[0].style == RowStyle.Content ) PrintRowAsText( row, sb );
					else PrintLine( sb, row[0].style );
				}
				else PrintRowAsText( row, sb );
			}

			// imprime finalização
			//Console.ForegroundColor = gridColor;
			if (rows.Count == 0) {
				sb.AppendLine( noContentText.FixedSize( TotalWidth ) );
				sb.AppendLine( string.Empty.FixedSize( TotalWidth ) );
			}
			else {
				//for (int i = 0; i < columnsSizes.Length; i++) {
				//	sb.Append( string.Empty.FixedSize( columnsSizes[i], ' ' ) );
				//	sb.Append( i < columnsSizes.Length - 1 ? " │ " : "\n" );
				//}
			}

			// imprime borda final
			//Console.ForegroundColor = gridColor;
			sb.AppendLine( string.Empty.PadRight( TotalWidth, '═' ) );
			return sb.ToString();
		}




		private void PrintLine( StringBuilder sb, RowStyle style = RowStyle.Line ) {
			var connector =
				style == RowStyle.LineTop ? "┬" :
				style == RowStyle.LineBottom ? "┴" :
				style == RowStyle.LineSimple ? "─" :
				style == RowStyle.Line ? "┼" : "*";
			for (int colIdx = 0; colIdx < columnsSizes.Length; colIdx++) {
				var colSize = columnsSizes[colIdx] < 0 ? columnsSizes[colIdx] * -1 : columnsSizes[colIdx];
				sb.Append( "─".PadRight( colSize, '─' ) );
				if (colIdx < columnsSizes.Length - 1) {
					//Console.ForegroundColor = gridColor;
					sb.Append( "─" + connector + "─" );
				}
				else
					sb.AppendLine();
			}
		}




		private void PrintRowAsTitle(Cell[] row, StringBuilder sb, bool topLine = true, bool bottomLine = true ) {
			//var tableSize = columnsSizes.Aggregate( 0, ( ag, it ) => ag + (it < 0 ? it * -1 : it) );
			//if (topLine) PrintSeparatorLine( sb, true );
			//sb.AppendLine();
			var text = row[0].text;
			if (text.Length > TotalWidth) text = text.Substring( 0, TotalWidth );
			sb.AppendLine( text );
			//if (bottomLine) PrintSeparatorLine( sb, true );
		}




		private void PrintRowAsText(Cell[] row, StringBuilder sb) {
			for (int column = 0; column < columnsSizes.Length; column++) {
				var colSize = columnsSizes[column] < 0 ? columnsSizes[column] * -1 : columnsSizes[column];
				//var cell = rows[rowIdx][column];
				var cell = row[column];
				//Console.ForegroundColor = cell.color ?? textColor;
				sb.Append( cell.text.FixedSize( colSize ) );
				if (column < columnsSizes.Length - 1) {
					//Console.ForegroundColor = gridColor;
					sb.Append( " │ " );
				}
				else
					sb.AppendLine();
			}
		}




		private void PrintEmptyLine( StringBuilder sb ) {
			for (int column = 0; column < columnsSizes.Length; column++) {
				var colSize = columnsSizes[column] < 0 ? columnsSizes[column] * -1 : columnsSizes[column];
				sb.Append( string.Empty.FixedSize( colSize ) );
				if (column < columnsSizes.Length - 1)
					sb.Append( " │ " );
				else
					sb.AppendLine();
			}
		}



		public class Cell {
			public string text;
			public Brush color;
			public RowStyle style;
			//public enum Style { Content, Full, Line, Empty }

			public Cell( Brush color = null, string text = null, RowStyle style = RowStyle.Content ) {
				this.color = color;
				this.text = text ?? string.Empty;
			}
			public Cell( string text, RowStyle style = RowStyle.Content ) {
				this.color = null;
				this.text = text ?? string.Empty;
				this.style = style;
			}

			public static implicit operator Cell( string text ) { return new Cell( text ); }
			public static implicit operator Cell( Brush color ) { return new Cell( color ); }
		}





	}
}
