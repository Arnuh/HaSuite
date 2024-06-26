﻿using System.Windows.Controls;
using HaCreator.MapEditor;

namespace HaCreator.GUI {
	public class TabItemContainer {
		public TabItemContainer(string text, MultiBoard multiBoard, string tooltip,
			ContextMenu menu, Board board) {
			this.text = text;
			this.multiBoard = multiBoard;
			this.tooltip = tooltip;
			this.menu = menu;
			this.board = board;
		}

		private string text;

		public string Text {
			get => text;
			set => text = value;
		}

		private MultiBoard multiBoard;

		public MultiBoard MultiBoard {
			get => multiBoard;
			private set { }
		}

		private string tooltip;

		public string Tooltip {
			get => tooltip;
			private set { }
		}

		private ContextMenu menu;

		public ContextMenu Menu {
			get => menu;
			private set { }
		}

		private Board board;

		public Board Board {
			get => board;
			private set { }
		}
	}
}