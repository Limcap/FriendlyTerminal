using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.FriendlyTerminal {
	public class MenuDef {
		public struct MenuItemDef {
			public bool HasSeparator;
			public int Priority;
			public string Header;
			public string Command;
			public List<MenuItemDef> Children;

			public override string ToString() {
				var sep = HasSeparator ? "" : " |";
				var pri = Priority == int.MaxValue ? "*" : Priority.ToString();
				var chi = " => " + (Children is null ? "" : string.Join( "  ", Children.Select( c =>
					c.Header
					+ (c.Children is null ? "" : $"({c.Children.Count})")
					+ (c.Command is null ? "" : "(*)")
				) ));
				return $"{pri}{sep} {Header}{chi}";
			}
		}




		public static List<MenuItemDef> GetMenuDef( Dictionary<string, Type> source ) {//

			var mainMenu = new MenuItemDef() { Header = "Menu", Children = new List<MenuItemDef>(), Command = null, HasSeparator = false, Priority = 0 };

			foreach (var entry in source) {
				string rawMenuText = entry.Value.GetConst( "MENU_TEXT" ) as string;
				if (rawMenuText == null) continue;
				var menuParts = rawMenuText.Split( ',' ).Select( p => p.Trim() ).ToList();

				var curParent = mainMenu;
				for (int i = 0; i < menuParts.Count; i++) {
					var cmd = i == menuParts.Count - 1 ? entry.Key : null;
					var itemDef = GetMenuItemDef( menuParts[i], cmd );
					var existingItemDef = FindMenuItemDef( itemDef, curParent.Children );
					if (existingItemDef is null) {
						curParent.Children.Add( itemDef );
						curParent = itemDef;
					}
					else curParent = existingItemDef.Value;
				}
			}

			SortMenu( mainMenu );
			return mainMenu.Children;
		}




		private static MenuItemDef GetMenuItemDef( string rawMenuItemText, string cmd ) {
			var slc = ((PString)rawMenuItemText).GetSlicer( '#' );
			int priority = int.MaxValue;
			if (slc.Count > 1) {
				var configPart = slc.Next();
				priority = configPart.Trim( '|' ).AsString.SafeParseInt( int.MaxValue );
			}
			var def = new MenuItemDef() {
				Header = slc.Remaining().AsString,
				Priority = priority,
				HasSeparator = rawMenuItemText.Contains( "|" ),
				Command = cmd,
				Children = cmd is null ? new List<MenuItemDef>() : null
			};
			return def;
		}




		public static MenuItemDef? FindMenuItemDef( MenuItemDef def, List<MenuItemDef> items ) {
			foreach (MenuItemDef item in items)
				if (item.Header == def.Header) return item;
			return null;
		}




		private static void SortMenu( MenuItemDef menu ) {
			var curParent = menu;
			if (curParent.Children is null) return;
			curParent.Children.Sort( ComparisonByHeader );
			curParent.Children?.Sort( ComparisonByPriority );
			foreach (var item in curParent.Children) SortMenu( item );
		}




		private static readonly Comparison<MenuItemDef> ComparisonByPriority = new Comparison<MenuItemDef>(
			delegate ( MenuItemDef a, MenuItemDef b ) {
				if (a.Priority > b.Priority) return 1;
				else if (a.Priority < b.Priority) return -1;
				else return 0;
			}
		);




		private static readonly Comparison<MenuItemDef> ComparisonByHeader = new Comparison<MenuItemDef>(
			delegate ( MenuItemDef a, MenuItemDef b ) { return string.Compare( a.Header, b.Header ); }
		);

	}
}
