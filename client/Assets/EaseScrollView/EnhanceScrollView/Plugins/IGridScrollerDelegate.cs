using UnityEngine;

namespace UnityEngine.UI
{
	/// <summary>
	/// All scripts that handle the scroller's callbacks should inherit from this interface
	/// </summary>
	public interface IGridScrollerDelegate
	{
		/// <summary>
		/// Gets the number of cells in a list of data
		/// </summary>
		/// <returns></returns>
		int GetNumberOfCells();

		/// <summary>
		/// Gets the cell view that should be used for the data index. Your implementation
		/// of this function should request a new cell from the scroller so that it can
		/// properly recycle old cells.
		/// </summary>
		/// <param name="scroller"></param>
		/// <param name="dataIndex"></param>
		/// <param name="cellIndex"></param>
		/// <returns></returns>
		RectTransform GetCellView(int dataIndex, int cellIndex);

		void RecycleCellView(RectTransform cellView, int dataIndex);

		bool IsVisible(ref Rect child, int dataIndex, bool isInContainer);
		Rect GetClipRect();
		void SetRichDelta(float xMin, float yMin, float xMax, float yMax);
	}
}