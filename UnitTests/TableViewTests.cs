using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;
using Xunit;
using System.Globalization;

namespace UnitTests {
	public class TableViewTests 
	{

        [Fact]
        public void EnsureValidScrollOffsets_WithNoCells()
        {
            var tableView = new TableView();

            Assert.Equal(0,tableView.RowOffset);
            Assert.Equal(0,tableView.ColumnOffset);

            // Set empty table
            tableView.Table = new DataTable();

            // Since table has no rows or columns scroll offset should default to 0
            tableView.EnsureValidScrollOffsets();
            Assert.Equal(0,tableView.RowOffset);
            Assert.Equal(0,tableView.ColumnOffset);
        }



        [Fact]
        public void EnsureValidScrollOffsets_LoadSmallerTable()
        {
            var tableView = new TableView();
            tableView.Bounds = new Rect(0,0,25,10);

            Assert.Equal(0,tableView.RowOffset);
            Assert.Equal(0,tableView.ColumnOffset);

            // Set big table
            tableView.Table = BuildTable(25,50);

            // Scroll down and along
            tableView.RowOffset = 20;
            tableView.ColumnOffset = 10;

            tableView.EnsureValidScrollOffsets();

            // The scroll should be valid at the moment
            Assert.Equal(20,tableView.RowOffset);
            Assert.Equal(10,tableView.ColumnOffset);

            // Set small table
            tableView.Table = BuildTable(2,2);

            // Setting a small table should automatically trigger fixing the scroll offsets to ensure valid cells
            Assert.Equal(0,tableView.RowOffset);
            Assert.Equal(0,tableView.ColumnOffset);


            // Trying to set invalid indexes should not be possible
            tableView.RowOffset = 20;
            tableView.ColumnOffset = 10;

            Assert.Equal(1,tableView.RowOffset);
            Assert.Equal(1,tableView.ColumnOffset);
        }

        [Fact]
        public void SelectedCellChanged_NotFiredForSameValue()
        {
            var tableView = new TableView(){
                Table = BuildTable(25,50)
            };

            bool called = false;
            tableView.SelectedCellChanged += (e)=>{called=true;};

            Assert.Equal(0,tableView.SelectedColumn);
            Assert.False(called);
            
            // Changing value to same as it already was should not raise an event
            tableView.SelectedColumn = 0;

            Assert.False(called);

            tableView.SelectedColumn = 10;
            Assert.True(called);
        }



        [Fact]
        public void SelectedCellChanged_SelectedColumnIndexesCorrect()
        {
            var tableView = new TableView(){
                Table = BuildTable(25,50)
            };

            bool called = false;
            tableView.SelectedCellChanged += (e)=>{
                called=true;
                Assert.Equal(0,e.OldCol);
                Assert.Equal(10,e.NewCol);
            };
            
            tableView.SelectedColumn = 10;
            Assert.True(called);
        }

        [Fact]
        public void SelectedCellChanged_SelectedRowIndexesCorrect()
        {
            var tableView = new TableView(){
                Table = BuildTable(25,50)
            };

            bool called = false;
            tableView.SelectedCellChanged += (e)=>{
                called=true;
                Assert.Equal(0,e.OldRow);
                Assert.Equal(10,e.NewRow);
            };
            
            tableView.SelectedRow = 10;
            Assert.True(called);
        }

        [Fact]
        public void Test_SumColumnWidth_UnicodeLength()
        {
            Assert.Equal(11,"hello there".Sum(c=>Rune.ColumnWidth(c)));

            // Creates a string with the peculiar (french?) r symbol
            String surrogate = "Les Mise" + Char.ConvertFromUtf32(Int32.Parse("0301", NumberStyles.HexNumber)) + "rables";

            // The unicode width of this string is shorter than the string length! 
            Assert.Equal(14,surrogate.Sum(c=>Rune.ColumnWidth(c)));
            Assert.Equal(15,surrogate.Length);
        }

        [Fact]
        public void IsSelected_MultiSelectionOn_Vertical()
        {
            var tableView = new TableView(){
                Table = BuildTable(25,50),
                MultiSelect = true
            };

            // 3 cell vertical selection
            tableView.SetSelection(1,1,false);
            tableView.SetSelection(1,3,true);

            Assert.False(tableView.IsSelected(0,0));
            Assert.False(tableView.IsSelected(1,0));
            Assert.False(tableView.IsSelected(2,0));

            Assert.False(tableView.IsSelected(0,1));
            Assert.True(tableView.IsSelected(1,1));
            Assert.False(tableView.IsSelected(2,1));

            Assert.False(tableView.IsSelected(0,2));
            Assert.True(tableView.IsSelected(1,2));
            Assert.False(tableView.IsSelected(2,2));

            Assert.False(tableView.IsSelected(0,3));
            Assert.True(tableView.IsSelected(1,3));
            Assert.False(tableView.IsSelected(2,3));

            Assert.False(tableView.IsSelected(0,4));
            Assert.False(tableView.IsSelected(1,4));
            Assert.False(tableView.IsSelected(2,4));
        }


        [Fact]
        public void IsSelected_MultiSelectionOn_Horizontal()
        {
            var tableView = new TableView(){
                Table = BuildTable(25,50),
                MultiSelect = true
            };

            // 2 cell horizontal selection
            tableView.SetSelection(1,0,false);
            tableView.SetSelection(2,0,true);

            Assert.False(tableView.IsSelected(0,0));
            Assert.True(tableView.IsSelected(1,0));
            Assert.True(tableView.IsSelected(2,0));
            Assert.False(tableView.IsSelected(3,0));

            Assert.False(tableView.IsSelected(0,1));
            Assert.False(tableView.IsSelected(1,1));
            Assert.False(tableView.IsSelected(2,1));
            Assert.False(tableView.IsSelected(3,1));
        }



        [Fact]
        public void IsSelected_MultiSelectionOn_BoxSelection()
        {
            var tableView = new TableView(){
                Table = BuildTable(25,50),
                MultiSelect = true
            };

            // 4 cell horizontal in box 2x2
            tableView.SetSelection(0,0,false);
            tableView.SetSelection(1,1,true);

            Assert.True(tableView.IsSelected(0,0));
            Assert.True(tableView.IsSelected(1,0));
            Assert.False(tableView.IsSelected(2,0));

            Assert.True(tableView.IsSelected(0,1));
            Assert.True(tableView.IsSelected(1,1));
            Assert.False(tableView.IsSelected(2,1));

            Assert.False(tableView.IsSelected(0,2));
            Assert.False(tableView.IsSelected(1,2));
            Assert.False(tableView.IsSelected(2,2));
        }
        
        /// <summary>
		/// Builds a simple table of string columns with the requested number of columns and rows
		/// </summary>
		/// <param name="cols"></param>
		/// <param name="rows"></param>
		/// <returns></returns>
		public static DataTable BuildTable(int cols, int rows)
		{
			var dt = new DataTable();

			for(int c = 0; c < cols; c++) {
				dt.Columns.Add("Col"+c);
			}
				
			for(int r = 0; r < rows; r++) {
				var newRow = dt.NewRow();

				for(int c = 0; c < cols; c++) {
					newRow[c] = $"R{r}C{c}";
				}

				dt.Rows.Add(newRow);
			}
			
			return dt;
		}
	}
}